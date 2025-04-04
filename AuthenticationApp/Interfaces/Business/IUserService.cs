using AuthenticationApp.Domain.DTOs;

namespace AuthenticationApp.Interfaces.Business
{
    public interface IUserService
    {
        Task CreateUser(CreateUserDTO user);
        Task<UserDTO> GetUserByCredentials(string username, string password);
        //Task<UserDTO> GetUserById(int id);
        //Task<IEnumerable<UserDTO>> GetAllUsers();
        //Task UpdateUser(int id, UpdateUserDTO user);
        //Task DeleteUser(int id);
    }
}
