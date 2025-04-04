using AuthenticationApp.Domain.DTOs;

namespace AuthenticationApp.Interfaces.DataAccess
{
    public interface IUserRepository
    {
        Task CreateUser(CreateUserDTO userDTO);
        //Task<bool> UserExists(string username);
        Task<UserDTO> GetUserByCredentials(string username, string password);
    }
}
