namespace AuthenticationApp.Domain.DTOs
{
    public class UserDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }

    }
}
