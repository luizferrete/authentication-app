using AuthenticationApp.Domain.Models;
using AuthenticationApp.Interfaces.DataAccess;
using MongoDB.Driver;

namespace AuthenticationApp.DataAccess.Context
{
    public class MongoDbContext : IMongoDbContext
    {
        public IMongoDatabase Database { get; }

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            Database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return Database.GetCollection<T>(collectionName);
        }

        public IMongoCollection<User> Users => Database.GetCollection<User>("User");
    }
}
