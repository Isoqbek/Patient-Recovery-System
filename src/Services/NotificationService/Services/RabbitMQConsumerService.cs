using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using NotificationService.Events;
using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Services;

public class RabbitMQConsumerService : IRabbitMQConsumerService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _exchangeName = "patient_recovery_events";
    private readonly string _queueName = "notification_service_queue";

    public RabbitMQConsumerService(
        ILogger<RabbitMQConsumerService> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Topic, durable: true);

            // Declare queue
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);

            // Bind queue to exchange for alert events
            _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: "monitoring.alertcreated");

            _logger.LogInformation("RabbitMQ Consumer connected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ Consumer connection");
            throw;
        }
    }

    public void StartConsuming()
    {
        try
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                _logger.LogInformation("Received message with routing key: {RoutingKey}", routingKey);

                try
                {
                    await ProcessMessage(routingKey, message);
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {Message}", message);
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("Started consuming messages from RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting RabbitMQ consumer");
            throw;
        }
    }

    public void StopConsuming()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
            _logger.LogInformation("Stopped consuming messages from RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping RabbitMQ consumer");
        }
    }

    private async Task ProcessMessage(string routingKey, string message)
    {
        try
        {
            if (routingKey == "monitoring.alertcreated")
            {
                var alertEvent = JsonConvert.DeserializeObject<AlertCreatedEvent>(message);
                if (alertEvent != null)
                {
                    await HandleAlertCreatedEvent(alertEvent);
                }
            }
            else
            {
                _logger.LogWarning("Unknown routing key: {RoutingKey}", routingKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message with routing key: {RoutingKey}", routingKey);
            throw;
        }
    }

    private async Task HandleAlertCreatedEvent(AlertCreatedEvent alertEvent)
    {
        try
        {
            _logger.LogInformation("Processing AlertCreatedEvent for Alert: {AlertId}", alertEvent.AlertId);

            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // Parse recipient roles
            var recipientRoles = new List<string>();
            if (!string.IsNullOrEmpty(alertEvent.RecipientRoles))
            {
                try
                {
                    recipientRoles = JsonConvert.DeserializeObject<List<string>>(alertEvent.RecipientRoles) ?? new List<string>();
                }
                catch
                {
                    recipientRoles = new List<string> { "Physician", "Nurse" }; // Default
                }
            }

            // Create notifications for each recipient role
            foreach (var role in recipientRoles)
            {
                await CreateNotificationForRole(notificationService, alertEvent, role);
            }

            _logger.LogInformation("Successfully processed AlertCreatedEvent for Alert: {AlertId}", alertEvent.AlertId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling AlertCreatedEvent for Alert: {AlertId}", alertEvent.AlertId);
            throw;
        }
    }

    private async Task CreateNotificationForRole(INotificationService notificationService, AlertCreatedEvent alertEvent, string role)
    {
        try
        {
            var priority = GetNotificationPriority(alertEvent.Severity);
            var notificationType = NotificationType.Alert;
            var channel = GetNotificationChannel(role, priority);

            var subject = $"🚨 {alertEvent.Severity} Alert: {alertEvent.Title}";
            var message = $"Patient Alert: {alertEvent.Description}\n\n" +
                         $"Severity: {alertEvent.Severity}\n" +
                         $"Time: {alertEvent.AlertDateTime:yyyy-MM-dd HH:mm:ss}\n" +
                         $"Alert ID: {alertEvent.AlertId}";

            var createNotificationDto = new CreateNotificationDto
            {
                PatientId = alertEvent.PatientId,
                RecipientType = role,
                RecipientId = GetRecipientId(role),
                RecipientEmail = GetRecipientEmail(role),
                RecipientPhone = GetRecipientPhone(role),
                NotificationType = notificationType,
                Channel = channel,
                Subject = subject,
                Message = message,
                RelatedEntityId = alertEvent.AlertId,
                RelatedEntityType = "Alert",
                Priority = priority
            };

            var notification = await notificationService.CreateNotificationAsync(createNotificationDto);

            // Automatically send the notification
            await notificationService.SendNotificationAsync(notification.Id);

            _logger.LogInformation("Created and sent notification for {Role} regarding Alert: {AlertId}", role, alertEvent.AlertId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification for {Role} regarding Alert: {AlertId}", role, alertEvent.AlertId);
        }
    }

    private static NotificationPriority GetNotificationPriority(string severity)
    {
        return severity?.ToLower() switch
        {
            "critical" => NotificationPriority.Critical,
            "warning" => NotificationPriority.High,
            "information" => NotificationPriority.Normal,
            _ => NotificationPriority.Normal
        };
    }

    private static NotificationChannel GetNotificationChannel(string role, NotificationPriority priority)
    {
        // For development/testing, use console
        // In production, use Email for most cases, SMS for critical
        if (priority == NotificationPriority.Critical)
            return NotificationChannel.Console; // Could be SMS in production

        return NotificationChannel.Console; // Could be Email in production
    }

    private static string GetRecipientId(string role)
    {
        return role.ToLower() switch
        {
            "physician" => "physician-001",
            "nurse" => "nurse-001",
            "patient" => "patient-001",
            "administrator" => "admin-001",
            _ => "unknown"
        };
    }

    private static string GetRecipientEmail(string role)
    {
        return role.ToLower() switch
        {
            "physician" => "michael.johnson@hospital.com",
            "nurse" => "jane.smith@hospital.com",
            "patient" => "john.doe@patient.com",
            "administrator" => "admin@hospital.com",
            _ => "support@hospital.com"
        };
    }

    private static string GetRecipientPhone(string role)
    {
        return role.ToLower() switch
        {
            "physician" => "+998901234567",
            "nurse" => "+998907654321",
            "patient" => "+998901111111",
            "administrator" => "+998909999999",
            _ => "+998900000000"
        };
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ Consumer");
        }
    }
}