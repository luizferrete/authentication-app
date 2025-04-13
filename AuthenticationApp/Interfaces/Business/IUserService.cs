using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Request;

namespace AuthenticationApp.Interfaces.Business
{
    public interface IUserService
    {
        Task CreateUser(CreateUserDTO user);
        Task<UserDTO> GetUserByCredentials(string username, string password);
        Task UpdateRefreshToken(string username, string newToken);
        Task<UserDTO> GetUserByRefreshToken(string refreshToken);
        Task ChangePassword(ChangePasswordRequest changePasswordRequest);
    }
}
