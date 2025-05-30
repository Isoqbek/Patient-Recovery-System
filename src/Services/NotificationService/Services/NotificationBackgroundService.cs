using NotificationService.DTOs;

namespace NotificationService.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;
    private readonly IRabbitMQConsumerService _rabbitMQConsumer;

    public NotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<NotificationBackgroundService> logger,
        IRabbitMQConsumerService rabbitMQConsumer)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMQConsumer = rabbitMQConsumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Background Service started");

        // Start RabbitMQ consumer
        _rabbitMQConsumer.StartConsuming();

        // Process pending notifications periodically
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotifications();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Check every minute
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification background service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("Notification Background Service stopped");
    }

    private async Task ProcessPendingNotifications()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var pendingCount = await notificationService.GetPendingNotificationCountAsync();

            if (pendingCount > 0)
            {
                _logger.LogInformation("Processing {PendingCount} pending notifications", pendingCount);

                // Get pending notifications and try to send them
                var filter = new NotificationFilterDto
                {
                    Status = Models.NotificationStatus.Pending,
                    Page = 1,
                    PageSize = 50
                };

                var pendingNotifications = await notificationService.GetNotificationsAsync(filter);

                foreach (var notification in pendingNotifications)
                {
                    try
                    {
                        await notificationService.SendNotificationAsync(notification.Id);
                        await Task.Delay(100); // Small delay between sends
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send pending notification {NotificationId}", notification.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending notifications");
        }
    }

    public override void Dispose()
    {
        _rabbitMQConsumer?.StopConsuming();
        base.Dispose();
    }
}