using AuthenticationApp.Infra.Interfaces;
using RabbitMQ.Client;
using System.Text;

namespace AuthenticationApp.Infra
{
    public class QueuePublisher : IQueuePublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName = "auth_app_exchange";

        public QueuePublisher(string hostName, int port, string userName, string password)
        {
            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Topic, durable: true);
        }


        public void Publish(string routingKey, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(_exchangeName, routingKey, properties, body);
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
