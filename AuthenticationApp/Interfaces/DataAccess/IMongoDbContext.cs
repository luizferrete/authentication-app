using AuthenticationApp.Domain.Models;
using MongoDB.Driver;

namespace AuthenticationApp.Interfaces.DataAccess
{
    public interface IMongoDbContext
    {
        IMongoDatabase Database { get; }
        IMongoCollection<User> Users { get; }

        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}