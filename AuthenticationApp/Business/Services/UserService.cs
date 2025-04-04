using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Interfaces.Business;
using AuthenticationApp.Interfaces.DataAccess;

namespace AuthenticationApp.Business.Services
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        
        public async Task CreateUser(CreateUserDTO userDTO)
        {
            await userRepository.CreateUser(userDTO);
        }

        public async Task<UserDTO> GetUserByCredentials(string username, string password)
        {
            return await userRepository.GetUserByCredentials(username, password);
        }
    }
}
