using AuthenticationApp.Domain.DTOs;

namespace AuthenticationApp.Interfaces.DataAccess
{
    public interface IUserRepository
    {
        Task CreateUser(CreateUserDTO userDTO);
        //Task<bool> UserExists(string username);
        Task<LoginUserDTO> GetUserByCredentials(string username);
        Task UpdateUser(UserDTO userDTO);
        Task<UserDTO> GetUserByUsername(string username);
        Task<UserDTO> GetUserByRefreshToken(string refreshToken);
        Task<int> ChangePassord(LoginUserDTO user);
    }
}
