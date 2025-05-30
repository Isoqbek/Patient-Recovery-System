using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Models;
using NotificationService.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Get notifications with filtering
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "NotificationReadPolicy")]
    public async Task<ActionResult<IEnumerable<NotificationLogDto>>> GetNotifications([FromQuery] NotificationFilterDto filter)
    {
        try
        {
            var notifications = await _notificationService.GetNotificationsAsync(filter);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notifications");
            return StatusCode(500, "An error occurred while retrieving notifications.");
        }
    }

    /// <summary>
    /// Get notification by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "NotificationReadPolicy")]
    public async Task<ActionResult<NotificationLogDto>> GetNotificationById(Guid id)
    {
        try
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);

            if (notification == null)
                return NotFound($"Notification with ID {id} not found.");

            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notification with ID: {NotificationId}", id);
            return StatusCode(500, "An error occurred while retrieving the notification.");
        }
    }

    /// <summary>
    /// Get notifications by patient ID
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Policy = "NotificationReadPolicy")]
    public async Task<ActionResult<IEnumerable<NotificationLogDto>>> GetNotificationsByPatientId(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters. Page must be >= 1 and PageSize must be between 1 and 100.");

            var notifications = await _notificationService.GetNotificationsByPatientIdAsync(patientId, page, pageSize);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notifications for patient: {PatientId}", patientId);
            return StatusCode(500, "An error occurred while retrieving notifications.");
        }
    }

    /// <summary>
    /// Get pending notification count
    /// </summary>
    [HttpGet("count/pending")]
    [Authorize(Policy = "NotificationReadPolicy")]
    public async Task<ActionResult<object>> GetPendingNotificationCount()
    {
        try
        {
            var count = await _notificationService.GetPendingNotificationCountAsync();
            return Ok(new { pendingNotificationCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving pending notification count");
            return StatusCode(500, "An error occurred while retrieving pending notification count.");
        }
    }

    /// <summary>
    /// Get failed notifications
    /// </summary>
    [HttpGet("failed")]
    [Authorize(Policy = "NotificationReadPolicy")]
    public async Task<ActionResult<IEnumerable<NotificationLogDto>>> GetFailedNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters. Page must be >= 1 and PageSize must be between 1 and 100.");

            var notifications = await _notificationService.GetFailedNotificationsAsync(page, pageSize);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving failed notifications");
            return StatusCode(500, "An error occurred while retrieving failed notifications.");
        }
    }

    /// <summary>
    /// Create a new notification manually
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "NotificationWritePolicy")]
    public async Task<ActionResult<NotificationLogDto>> CreateNotification([FromBody] CreateNotificationDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var notification = await _notificationService.CreateNotificationAsync(createDto);

            return CreatedAtAction(
                nameof(GetNotificationById),
                new { id = notification.Id },
                notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating notification");
            return StatusCode(500, "An error occurred while creating the notification.");
        }
    }

    /// <summary>
    /// Send a specific notification
    /// </summary>
    [HttpPost("{id:guid}/send")]
    [Authorize(Policy = "NotificationWritePolicy")]
    public async Task<ActionResult> SendNotification(Guid id)
    {
        try
        {
            var success = await _notificationService.SendNotificationAsync(id);

            if (!success)
                return NotFound($"Notification with ID {id} not found or cannot be sent.");

            return Ok(new { message = "Notification sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending notification with ID: {NotificationId}", id);
            return StatusCode(500, "An error occurred while sending the notification.");
        }
    }

    /// <summary>
    /// Retry a failed notification
    /// </summary>
    [HttpPost("{id:guid}/retry")]
    [Authorize(Policy = "NotificationWritePolicy")]
    public async Task<ActionResult> RetryNotification(Guid id)
    {
        try
        {
            var success = await _notificationService.RetryNotificationAsync(id);

            if (!success)
                return NotFound($"Notification with ID {id} not found or cannot be retried.");

            return Ok(new { message = "Notification retried successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrying notification with ID: {NotificationId}", id);
            return StatusCode(500, "An error occurred while retrying the notification.");
        }
    }

    /// <summary>
    /// Cancel a pending notification
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "NotificationWritePolicy")]
    public async Task<ActionResult> CancelNotification(Guid id)
    {
        try
        {
            var success = await _notificationService.CancelNotificationAsync(id);

            if (!success)
                return NotFound($"Notification with ID {id} not found or cannot be cancelled.");

            return Ok(new { message = "Notification cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling notification with ID: {NotificationId}", id);
            return StatusCode(500, "An error occurred while cancelling the notification.");
        }
    }

    /// <summary>
    /// Get all available notification types
    /// </summary>
    [HttpGet("types")]
    [Authorize(Policy = "NotificationReadPolicy")]
    public ActionResult<object> GetNotificationTypes()
    {
        try
        {
            var types = Enum.GetValues<NotificationType>()
                .Select(t => new {
                    Value = (int)t,
                    Name = t.ToString(),
                    DisplayName = GetTypeDisplayName(t)
                });

            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notification types");
            return StatusCode(500, "An error occurred while retrieving notification types.");
        }
    }

    /// <summary>
    /// Get all available notification channels
    /// </summary>
    [HttpGet("channels")]
    [Authorize(Policy = "NotificationReadPolicy")]
    public ActionResult<object> GetNotificationChannels()
    {
        try
        {
            var channels = Enum.GetValues<NotificationChannel>()
                .Select(c => new {
                    Value = (int)c,
                    Name = c.ToString(),
                    DisplayName = GetChannelDisplayName(c)
                });

            return Ok(channels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notification channels");
            return StatusCode(500, "An error occurred while retrieving notification channels.");
        }
    }

    /// <summary>
    /// Get all available notification statuses
    /// </summary>
    [HttpGet("statuses")]
    [Authorize(Policy = "NotificationReadPolicy")]
    public ActionResult<object> GetNotificationStatuses()
    {
        try
        {
            var statuses = Enum.GetValues<NotificationStatus>()
                .Select(s => new {
                    Value = (int)s,
                    Name = s.ToString(),
                    DisplayName = GetStatusDisplayName(s)
                });

            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notification statuses");
            return StatusCode(500, "An error occurred while retrieving notification statuses.");
        }
    }

    /// <summary>
    /// Get all available notification priorities
    /// </summary>
    [HttpGet("priorities")]
    [Authorize(Policy = "NotificationReadPolicy")]
    public ActionResult<object> GetNotificationPriorities()
    {
        try
        {
            var priorities = Enum.GetValues<NotificationPriority>()
                .Select(p => new {
                    Value = (int)p,
                    Name = p.ToString(),
                    DisplayName = GetPriorityDisplayName(p)
                });

            return Ok(priorities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notification priorities");
            return StatusCode(500, "An error occurred while retrieving notification priorities.");
        }
    }

    private static string GetTypeDisplayName(NotificationType type)
    {
        return type switch
        {
            NotificationType.Alert => "Alert",
            NotificationType.Appointment => "Appointment",
            NotificationType.Medication => "Medication",
            NotificationType.TestResult => "Test Result",
            NotificationType.General => "General",
            NotificationType.Emergency => "Emergency",
            NotificationType.Reminder => "Reminder",
            _ => type.ToString()
        };
    }

    private static string GetChannelDisplayName(NotificationChannel channel)
    {
        return channel switch
        {
            NotificationChannel.Email => "Email",
            NotificationChannel.SMS => "SMS",
            NotificationChannel.Push => "Push Notification",
            NotificationChannel.InApp => "In-App Notification",
            NotificationChannel.Console => "Console (Debug)",
            _ => channel.ToString()
        };
    }

    private static string GetStatusDisplayName(NotificationStatus status)
    {
        return status switch
        {
            NotificationStatus.Pending => "Pending",
            NotificationStatus.Sent => "Sent",
            NotificationStatus.Failed => "Failed",
            NotificationStatus.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }

    private static string GetPriorityDisplayName(NotificationPriority priority)
    {
        return priority switch
        {
            NotificationPriority.Low => "Low",
            NotificationPriority.Normal => "Normal",
            NotificationPriority.High => "High",
            NotificationPriority.Critical => "Critical",
            _ => priority.ToString()
        };
    }
}