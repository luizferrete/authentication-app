using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Models;
using AuthenticationApp.Interfaces.DataAccess;
using MongoDB.Driver;

namespace AuthenticationApp.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        public UserRepository(IMongoDatabase database)
        {
            _users = database.GetCollection<User>(nameof(User));
        }

        public async Task CreateUser(CreateUserDTO userDTO)
        {
            var user = new User
            {
                Username = userDTO.Username,
                Password = userDTO.Password,
                Email = userDTO.Email
            };
            await _users.InsertOneAsync(user);
        }

        public async Task<LoginUserDTO> GetUserByCredentials(string username)
        {
            var user = await _users.Find(x => x.Username == username).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }
            return new LoginUserDTO
            {
                Username = user.Username,
                Password = user.Password,
                Email = user.Email,
                JwtToken = user.JwtToken,
                RefreshToken = user.RefreshToken
            };
        }
    }
}
