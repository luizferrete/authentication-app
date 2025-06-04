namespace AuthenticationApp.Infra.Interfaces
{
    public interface IQueuePublisher
    {
        void Publish(string routingKey, string message);
    }
}
