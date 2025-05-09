using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Request;

namespace AuthenticationApp.Interfaces.Business
{
    public interface IUserService
    {
        Task CreateUser(CreateUserRequest user);
        Task<UserDTO> GetUserByCredentials(string username, string password);
        Task<UserDTO> GetUserByRefreshToken(string refreshToken);
        Task ChangePassword(ChangePasswordRequest changePasswordRequest);
        Task<UserDTO> GetUserByUsername(string username);
    }
}
