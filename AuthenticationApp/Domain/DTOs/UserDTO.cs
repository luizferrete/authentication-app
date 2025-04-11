using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthenticationApp.Domain.DTOs
{
    public class UserDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }

    }
}
