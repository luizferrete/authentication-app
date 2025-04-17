using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Request;
using AuthenticationApp.Interfaces.Business;
using AuthenticationApp.Interfaces.DataAccess;
using BCrypt.Net;
using System.Security.Authentication;
using System.Security.Claims;

namespace AuthenticationApp.Business.Services
{
    public class UserService(IUserRepository userRepository, IHttpContextAccessor httpContext, IUnitOfWork unitOfWork) : IUserService
    {
        
        public async Task CreateUser(CreateUserDTO userDTO)
        {
            try
            {
                unitOfWork.StartTransaction();
                var user = await unitOfWork.Users.GetUserByCredentials(userDTO.Username, unitOfWork.Session);
                if (user is not null)
                {
                    throw new InvalidCredentialException("Usuário já existe.");
                }

                userDTO.Password = HashPassword(userDTO.Password);

                await unitOfWork.Users.CreateUser(userDTO, unitOfWork.Session);
                await unitOfWork.CommitAsync();
            }
            finally
            {
                unitOfWork.Dispose();
            }
            
        }

        public async Task<UserDTO> GetUserByCredentials(string username, string password)
        {
            var user = await userRepository.GetUserByCredentials(username);

            if (user is null || !VerifyPassword(password, user.Password))
            {
                throw new InvalidCredentialException("Usuário ou senha inválida.");
            }

            var loggedUser = new UserDTO
            {
                Username = user.Username,
                Email = user.Email,
                RefreshToken = user.RefreshToken
            };
            return loggedUser;
        }

        public async Task UpdateRefreshToken(string username, string newToken)
        {
            try
            {
                unitOfWork.StartTransaction();
                var user = await userRepository.GetUserByUsername(username);

                if (user is null)
                {
                    throw new InvalidCredentialException("Usuário não encontrado.");
                }

                user.RefreshToken = newToken;

                await userRepository.UpdateUser(user);
                await unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                unitOfWork.Dispose();
                throw;
            }
            
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

        public async Task ChangePassword(ChangePasswordRequest changePasswordRequest)
        {
            var claims = httpContext?.HttpContext?.User.Claims;
            var username = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            if (username is null)
            {
                throw new InvalidCredentialException("Usuário não encontrado.");
            }
            var user = await userRepository.GetUserByCredentials(username);
            if (user is null)
            {
                throw new InvalidCredentialException("Usuário não encontrado.");
            }
            if (!VerifyPassword(changePasswordRequest.OldPassword, user.Password))
            {
                throw new InvalidOperationException("A senha atual não coincide com a informada.");
            }
            if (VerifyPassword(changePasswordRequest.NewPassword, user.Password))
            {
                throw new InvalidOperationException("A nova senha não pode ser igual à senha atual.");
            }
            
            user.Password = HashPassword(changePasswordRequest.NewPassword);
            user.RefreshToken = string.Empty;

            try
            {
                unitOfWork.StartTransaction();
                await userRepository.ChangePassord(user, unitOfWork.Session);
                await unitOfWork.CommitAsync();
            }
            catch
            {
                unitOfWork.Dispose();
            }
            
        }

        //usar Rfc2898DeriveBytes de using system.security.cryptography
        private static string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        private static bool VerifyPassword(string password, string hashedPassword) =>
            BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
