using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Request;
using AuthenticationApp.Domain.Response;
using AuthenticationApp.Infra.Interfaces;
using AuthenticationApp.Interfaces.Business;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AuthenticationApp.Business.Services
{
    public class AuthService(IUserService userService
        , IConfiguration configuration
        , IUserContextService userContext
        , IDistributedCache cache
        , IConnectionMultiplexer redis
        , IQueuePublisher queuePublisher) : IAuthService
    {
        public async Task<LoginResponse> Login(LoginRequest login)
        {
            var user = await userService.GetUserByCredentials(login.Username, login.Password);

            LoginResponse loginResponse = BuildLoginResponse(user);
            await UpdateUserRefreshToken(user, loginResponse);

            queuePublisher.Publish("Email.Login", JsonSerializer.Serialize(new
            {
                user.Username,
                user.Email,
                Ip = userContext.UserIpAddress
            }));

            return loginResponse;
        }

        private async Task UpdateUserRefreshToken(UserDTO user, LoginResponse loginResponse)
        {
            await cache.RemoveAsync($"refresh:{user.RefreshToken}");

            user.RefreshToken = loginResponse.RefreshToken;

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(configuration.GetValue<int>("RedisSettings:ExpirationMinutes"))
            };

            var ip = userContext.UserIpAddress;

            // Set the cache key to retrieve logged user
            var cacheKey = $"refresh:{loginResponse.RefreshToken}";
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user), cacheOptions);

            // Set the cache key to indicate the user is logged in
            var cacheKeyUser = $"loggedUser:{user.Email}:{ip}";
            await cache.SetStringAsync(cacheKeyUser, loginResponse.RefreshToken, cacheOptions);
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
            var cacheKey = $"refresh:{request.RefreshToken}";

            var userJson = await cache.GetStringAsync(cacheKey) 
                ?? throw new InvalidCredentialException("Refresh token inválido.");
            
            var user = JsonSerializer.Deserialize<UserDTO>(userJson);

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

        public async Task<bool> Logout(RefreshTokenRequest request)
        {
            var name = userContext.UserName;

            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            var user = await userService.GetUserByUsername(name);

            await cache.RemoveAsync($"refresh:{request.RefreshToken}");
            await cache.RemoveAsync($"loggedUser:{user.Email}:{userContext.UserIpAddress}");

            return true;
        }

        public async Task<bool> MassLogout()
        {
            var name = userContext.UserName;

            var user = await userService.GetUserByUsername(name);

            var ip = userContext.UserIpAddress;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            var instanceName = "AuthenticationApp:";
            var cacheKey = $"{instanceName}loggedUser:{user.Email}:*";
            var server = redis.GetServer(redis.GetEndPoints().First());

            foreach (var key in server.Keys(pattern: cacheKey))
            {
                var completeKey = key.ToString().Substring(instanceName.Length);
                var refreshToken = await cache.GetStringAsync(completeKey);
                await cache.RemoveAsync($"refresh:{refreshToken}");
                await cache.RemoveAsync(completeKey);
            }

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
