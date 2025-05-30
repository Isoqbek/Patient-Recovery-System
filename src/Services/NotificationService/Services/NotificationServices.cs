using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Services;

public class NotificationServices : INotificationService
{
    private readonly NotificationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationServices> _logger;
    private readonly INotificationSenderService _notificationSender;

    public NotificationServices(
        NotificationDbContext context,
        IMapper mapper,
        ILogger<NotificationServices> logger,
        INotificationSenderService notificationSender)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _notificationSender = notificationSender;
    }

    public async Task<IEnumerable<NotificationLogDto>> GetNotificationsAsync(NotificationFilterDto filter)
    {
        try
        {
            var query = _context.NotificationLogs.AsQueryable();

            // Apply filters
            if (filter.PatientId.HasValue)
                query = query.Where(n => n.PatientId == filter.PatientId.Value);

            if (!string.IsNullOrEmpty(filter.RecipientType))
                query = query.Where(n => n.RecipientType.Contains(filter.RecipientType));

            if (filter.NotificationType.HasValue)
                query = query.Where(n => n.NotificationType == filter.NotificationType.Value);

            if (filter.Channel.HasValue)
                query = query.Where(n => n.Channel == filter.Channel.Value);

            if (filter.Status.HasValue)
                query = query.Where(n => n.Status == filter.Status.Value);

            if (filter.Priority.HasValue)
                query = query.Where(n => n.Priority == filter.Priority.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(n => n.CreatedAt <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.RelatedEntityType))
                query = query.Where(n => n.RelatedEntityType == filter.RelatedEntityType);

            // Apply sorting
            if (filter.SortBy?.ToLower() == "priority")
                query = filter.SortDescending ? query.OrderByDescending(n => n.Priority) : query.OrderBy(n => n.Priority);
            else if (filter.SortBy?.ToLower() == "status")
                query = filter.SortDescending ? query.OrderByDescending(n => n.Status) : query.OrderBy(n => n.Status);
            else if (filter.SortBy?.ToLower() == "sentat")
                query = filter.SortDescending ? query.OrderByDescending(n => n.SentAt) : query.OrderBy(n => n.SentAt);
            else // Default: CreatedAt
                query = filter.SortDescending ? query.OrderByDescending(n => n.CreatedAt) : query.OrderBy(n => n.CreatedAt);

            // Apply pagination
            var notifications = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} notifications with filter", notifications.Count);

            return _mapper.Map<IEnumerable<NotificationLogDto>>(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notifications with filter");
            throw;
        }
    }

    public async Task<NotificationLogDto?> GetNotificationByIdAsync(Guid id)
    {
        try
        {
            var notification = await _context.NotificationLogs.FindAsync(id);

            if (notification == null)
                return null;

            return _mapper.Map<NotificationLogDto>(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notification with ID: {NotificationId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<NotificationLogDto>> GetNotificationsByPatientIdAsync(Guid patientId, int page = 1, int pageSize = 10)
    {
        try
        {
            var notifications = await _context.NotificationLogs
                .Where(n => n.PatientId == patientId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<NotificationLogDto>>(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notifications for patient: {PatientId}", patientId);
            throw;
        }
    }

    public async Task<NotificationLogDto> CreateNotificationAsync(CreateNotificationDto createDto)
    {
        try
        {
            var notification = _mapper.Map<NotificationLog>(createDto);
            notification.Id = Guid.NewGuid();
            notification.Status = NotificationStatus.Pending;

            _context.NotificationLogs.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new notification with ID: {NotificationId}", notification.Id);

            return _mapper.Map<NotificationLogDto>(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating notification");
            throw;
        }
    }

    public async Task<bool> SendNotificationAsync(Guid notificationId)
    {
        try
        {
            var notification = await _context.NotificationLogs.FindAsync(notificationId);

            if (notification == null || notification.Status != NotificationStatus.Pending)
                return false;

            var success = await _notificationSender.SendNotificationAsync(notification);

            if (success)
            {
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
                notification.ErrorMessage = null;
            }
            else
            {
                notification.Status = NotificationStatus.Failed;
                notification.RetryCount++;
                notification.ErrorMessage = "Failed to send notification";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification {NotificationId} send result: {Success}", notificationId, success);

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending notification with ID: {NotificationId}", notificationId);

            // Update notification status to failed
            var notification = await _context.NotificationLogs.FindAsync(notificationId);
            if (notification != null)
            {
                notification.Status = NotificationStatus.Failed;
                notification.RetryCount++;
                notification.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync();
            }

            return false;
        }
    }

    public async Task<bool> RetryNotificationAsync(Guid notificationId)
    {
        try
        {
            var notification = await _context.NotificationLogs.FindAsync(notificationId);

            if (notification == null || notification.Status != NotificationStatus.Failed)
                return false;

            // Reset status to pending
            notification.Status = NotificationStatus.Pending;
            notification.ErrorMessage = null;
            await _context.SaveChangesAsync();

            // Try to send again
            return await SendNotificationAsync(notificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrying notification with ID: {NotificationId}", notificationId);
            return false;
        }
    }

    public async Task<bool> CancelNotificationAsync(Guid notificationId)
    {
        try
        {
            var notification = await _context.NotificationLogs.FindAsync(notificationId);

            if (notification == null || notification.Status != NotificationStatus.Pending)
                return false;

            notification.Status = NotificationStatus.Cancelled;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cancelled notification with ID: {NotificationId}", notificationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling notification with ID: {NotificationId}", notificationId);
            throw;
        }
    }

    public async Task<int> GetPendingNotificationCountAsync()
    {
        try
        {
            return await _context.NotificationLogs
                .CountAsync(n => n.Status == NotificationStatus.Pending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting pending notification count");
            throw;
        }
    }

    public async Task<IEnumerable<NotificationLogDto>> GetFailedNotificationsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var notifications = await _context.NotificationLogs
                .Where(n => n.Status == NotificationStatus.Failed)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<NotificationLogDto>>(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving failed notifications");
            throw;
        }
    }
}