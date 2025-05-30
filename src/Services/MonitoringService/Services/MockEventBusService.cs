namespace MonitoringService.Services;

public class MockEventBusService : IEventBusService
{
    private readonly ILogger<MockEventBusService> _logger;

    public MockEventBusService(ILogger<MockEventBusService> logger)
    {
        _logger = logger;
    }

    public async Task PublishAsync<T>(T eventMessage, string eventType) where T : class
    {
        try
        {
            _logger.LogInformation("Mock Event Published - Type: {EventType}, Message: {Message}",
                eventType,
                System.Text.Json.JsonSerializer.Serialize(eventMessage));

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mock event publishing");
        }
    }

    public void Dispose()
    {
        _logger.LogInformation("MockEventBusService disposed");
    }
}