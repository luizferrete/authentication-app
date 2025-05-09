namespace AuthenticationApp.Domain.Settings
{
    public class RedisSettings
    {
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
