namespace AuthenticationApp.Domain.Settings
{
    public class RabbitMQSettings
    {
        public required string Host { get; set; }
        public int Port { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
