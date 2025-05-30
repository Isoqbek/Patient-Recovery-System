using NotificationService.DTOs;

namespace NotificationService.Services;

public interface INotificationService
{
    Task<IEnumerable<NotificationLogDto>> GetNotificationsAsync(NotificationFilterDto filter);
    Task<NotificationLogDto?> GetNotificationByIdAsync(Guid id);
    Task<IEnumerable<NotificationLogDto>> GetNotificationsByPatientIdAsync(Guid patientId, int page = 1, int pageSize = 10);
    Task<NotificationLogDto> CreateNotificationAsync(CreateNotificationDto createDto);
    Task<bool> SendNotificationAsync(Guid notificationId);
    Task<bool> RetryNotificationAsync(Guid notificationId);
    Task<bool> CancelNotificationAsync(Guid notificationId);
    Task<int> GetPendingNotificationCountAsync();
    Task<IEnumerable<NotificationLogDto>> GetFailedNotificationsAsync(int page = 1, int pageSize = 10);
}