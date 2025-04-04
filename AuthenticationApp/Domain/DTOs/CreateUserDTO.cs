namespace AuthenticationApp.Domain.DTOs
{
    public class CreateUserDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
    }
}
