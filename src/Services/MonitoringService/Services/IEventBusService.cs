namespace MonitoringService.Services;

public interface IEventBusService
{
    Task PublishAsync<T>(T eventMessage, string eventType) where T : class;
    void Dispose();
}