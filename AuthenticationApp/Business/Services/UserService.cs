using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Interfaces.Business;
using AuthenticationApp.Interfaces.DataAccess;
using BCrypt.Net;
using System.Security.Authentication;

namespace AuthenticationApp.Business.Services
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        
        public async Task CreateUser(CreateUserDTO userDTO)
        {
            var user = await userRepository.GetUserByCredentials(userDTO.Username);
            if(user is not null)
            {
                throw new InvalidCredentialException("Usuário já existe.");
            }

            userDTO.Password = HashPassord(userDTO.Password);

            await userRepository.CreateUser(userDTO);
        }

        public async Task<UserDTO> GetUserByCredentials(string username, string password)
        {
            var user = await userRepository.GetUserByCredentials(username);

            if(user is null || !VerifyPassword(password, user.Password))
            {
                throw new InvalidCredentialException("Usuário ou senha inválida.");
            }

            var loggedUser = new UserDTO
            {
                Username = user.Username,
                Email = user.Email,
                JwtToken = user.JwtToken,
                RefreshToken = user.RefreshToken
            };

            return loggedUser;
        }

        public async Task UpdateRefreshToken(string username, string newToken)
        {
            var user = await userRepository.GetUserByUsername(username);

            if (user is null)
            {
                throw new InvalidCredentialException("Usuário não encontrado.");
            }

            user.RefreshToken = newToken;

            await userRepository.UpdateUser(user);
        }

        public async Task<UserDTO> GetUserByRefreshToken(string refreshToken)
        {
            var user = await userRepository.GetUserByRefreshToken(refreshToken);
            if (user is null)
            {
                throw new InvalidCredentialException("Refresh token inválido.");
            }
            return user;
        }

        private static string HashPassord(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        private static bool VerifyPassword(string password, string hashedPassword) =>
            BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
