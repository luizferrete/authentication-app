using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Request;
using MongoDB.Driver;

namespace AuthenticationApp.Interfaces.DataAccess
{
    public interface IUserRepository
    {
        Task CreateUser(CreateUserRequest userDTO, IClientSessionHandle? session = null);
        //Task<bool> UserExists(string username);
        Task<LoginUserDTO> GetUserByCredentials(string username, IClientSessionHandle? session = null);
        Task UpdateUser(UserDTO userDTO, IClientSessionHandle? session = null);
        Task<UserDTO> GetUserByUsername(string username, IClientSessionHandle? session = null);
        Task<UserDTO> GetUserByRefreshToken(string refreshToken, IClientSessionHandle? session = null);
        Task<int> ChangePassord(LoginUserDTO user, IClientSessionHandle? session = null);
    }
}
