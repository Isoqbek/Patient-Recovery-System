namespace MonitoringService.Services;

public class MonitoringBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MonitoringBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public MonitoringBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<MonitoringBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Monitoring Background Service started");

        var intervalMinutes = _configuration.GetValue<int>("Monitoring:IntervalMinutes", 5);
        var interval = TimeSpan.FromMinutes(intervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var monitoringService = scope.ServiceProvider.GetRequiredService<IClinicalDataMonitoringService>();

                _logger.LogInformation("Starting monitoring cycle");
                await monitoringService.MonitorAllPatientsAsync();
                _logger.LogInformation("Monitoring cycle completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during monitoring cycle");
            }

            await Task.Delay(interval, stoppingToken);
        }

        _logger.LogInformation("Monitoring Background Service stopped");
    }
}