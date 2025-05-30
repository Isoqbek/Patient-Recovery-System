using NotificationService.Models;

namespace NotificationService.Services;

public interface INotificationSenderService
{
    Task<bool> SendNotificationAsync(NotificationLog notification);
    Task<bool> SendEmailAsync(string to, string subject, string body);
    Task<bool> SendSMSAsync(string phoneNumber, string message);
    Task<bool> SendPushNotificationAsync(string recipientId, string title, string message);
    Task<bool> SendInAppNotificationAsync(string recipientId, string title, string message);
}