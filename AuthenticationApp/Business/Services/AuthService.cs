﻿using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Request;
using AuthenticationApp.Domain.Response;
using AuthenticationApp.Interfaces.Business;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationApp.Business.Services
{
    public class AuthService(IUserService userService, IConfiguration configuration, IHttpContextAccessor context) : IAuthService
    {
        public async Task<LoginResponse> Login(LoginRequest login)
        {
            var user = await userService.GetUserByCredentials(login.Username, login.Password);

            LoginResponse loginResponse = BuildLoginResponse(user);
            await UpdateUserRefreshToken(user, loginResponse);

            return loginResponse;
        }

        private async Task UpdateUserRefreshToken(UserDTO user, LoginResponse loginResponse)
        {
            user.RefreshToken = loginResponse.RefreshToken;
            await userService.UpdateRefreshToken(user.Username, loginResponse.RefreshToken);
        }

        private string GenerateToken(UserDTO user)
        {
            var handler = new JwtSecurityTokenHandler();
            var privateKey = configuration["JWTToken:PrivateKey"] ?? "";
            int expirationMinutes = configuration.GetValue<int>("JWTToken:ExpirationMinutes");
            var issuer = configuration["JWTToken:Issuer"] ?? "";
            var audience = configuration["JWTToken:Audience"] ?? "";

            var token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = GenerateClaims(user),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey)), SecurityAlgorithms.HmacSha256Signature)
            });

            return handler.WriteToken(token);
        }

        private static ClaimsIdentity GenerateClaims(UserDTO user)
        {
            return new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "Admin")
                ]);
        }

        public async Task<LoginResponse> RefreshToken(RefreshTokenRequest request)
        {
            var user = await userService.GetUserByRefreshToken(request.RefreshToken);

            if (user is null)
            {
                throw new InvalidOperationException("Refresh token inválido.");
            }

            LoginResponse loginResponse = BuildLoginResponse(user);
            await UpdateUserRefreshToken(user, loginResponse);

            return loginResponse;
        }

        private LoginResponse BuildLoginResponse(UserDTO user)
        {
            return new LoginResponse
            {
                JwtToken = GenerateToken(user),
                RefreshToken = Guid.NewGuid().ToString()
            };
        }

        public async Task<bool> Logout()
        {
            var httpContext = context.HttpContext;
            if(httpContext is null)
            {
                return false;
            }
            var name = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;

            if(string.IsNullOrEmpty(name))
            {
                return false;
            }

            await userService.UpdateRefreshToken(name, string.Empty);
            return true;
        }

        public async Task<bool> ValidateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var privateKey = configuration["JWTToken:PrivateKey"] ?? "";
            var issuer = configuration["JWTToken:Issuer"] ?? "";
            var audience = configuration["JWTToken:Audience"] ?? "";
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true
            };
            try
            {
                handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
