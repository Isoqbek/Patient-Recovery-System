namespace NotificationService.Services;

public interface IRabbitMQConsumerService
{
    void StartConsuming();
    void StopConsuming();
}