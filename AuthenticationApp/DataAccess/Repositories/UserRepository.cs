using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Models;
using AuthenticationApp.Interfaces.DataAccess;
using MongoDB.Driver;
using System.Security.Authentication;

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
                RefreshToken = user.RefreshToken
            };
        }

        public async Task<UserDTO> GetUserByRefreshToken(string refreshToken)
        {
            var user = await _users.Find(x => x.RefreshToken == refreshToken).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }

            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RefreshToken = user.RefreshToken
            };
        }

        public async Task<UserDTO> GetUserByUsername(string username)
        {
            var user = await _users.Find(x => x.Username == username).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }

            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RefreshToken = user.RefreshToken
            };
        }

        public async Task UpdateUser(UserDTO userDTO)
        {
            var user = await _users.Find(x => x.Id == userDTO.Id).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new Exception("User not found");
            }
            user.Username = userDTO.Username;
            user.Email = userDTO.Email;
            user.RefreshToken = userDTO.RefreshToken;
            var filter = Builders<User>.Filter.Eq(u => u.Id, userDTO.Id);
            var update = Builders<User>.Update
                .Set(x => x.Username, user.Username)
                .Set(x => x.Email, user.Email)
                .Set(x => x.RefreshToken, user.RefreshToken);
            var result = await _users.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                throw new Exception("User not found");
            }
        }

        public async Task<int> ChangePassord(LoginUserDTO user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Username, user.Username);
            var update = Builders<User>.Update
                .Set(u => u.Password, user.Password)
                .Set(u => u.RefreshToken, user.RefreshToken);
            var result = await _users.UpdateOneAsync(filter, update);
            if (result.MatchedCount == 0)
            {
                throw new InvalidCredentialException("User not found");
            }
            return (int)result.ModifiedCount;
        }
    }
}
