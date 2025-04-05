using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Interfaces.Business;
using DnsClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationApp.Business.Services
{
    public class LoginService(IUserService userService, IConfiguration configuration) : ILoginService
    {
        public async Task<UserDTO> Login(LoginDTO login)
        {
            var user = await userService.GetUserByCredentials(login.Username, login.Password);

            user.JwtToken = GenerateToken(user);


            return user;
        }

        private string GenerateToken(UserDTO user)
        {
            var handler = new JwtSecurityTokenHandler();
            var privateKey = configuration["JWTToken:PrivateKey"] ?? "";


            var token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = GenerateClaims(user),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey)), SecurityAlgorithms.HmacSha256Signature)
            });

            return handler.WriteToken(token);
        }

        private static ClaimsIdentity GenerateClaims(UserDTO user)
        {
            return new ClaimsIdentity(new[]
                            {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "Admin")
                });
        }
    }
}
