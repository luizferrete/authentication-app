
using AuthenticationApp.Domain.Settings;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AuthenticationApp.Infra
{
    public class QueueInitializer : IHostedService
    {
        private readonly RabbitMQSettings _cfg;

        public QueueInitializer(IOptions<RabbitMQSettings> opts)
        => _cfg = opts.Value;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _cfg.Host,
                Port = _cfg.Port,
                UserName = _cfg.UserName,
                Password = _cfg.Password
            };

            using var conn = factory.CreateConnection();
            using var channel = conn.CreateModel();

            channel.QueueDeclare(
                queue: "email_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
