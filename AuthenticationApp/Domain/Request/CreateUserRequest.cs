﻿namespace AuthenticationApp.Domain.Request
{
    public class CreateUserRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
    }
}
