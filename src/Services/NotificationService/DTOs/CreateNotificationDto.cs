using System.ComponentModel.DataAnnotations;
using NotificationService.Models;

namespace NotificationService.DTOs;

public class CreateNotificationDto
{
    [Required(ErrorMessage = "Patient ID is required")]
    public Guid PatientId { get; set; }

    [Required(ErrorMessage = "Recipient type is required")]
    [StringLength(200, ErrorMessage = "Recipient type cannot exceed 200 characters")]
    public string RecipientType { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Recipient ID cannot exceed 200 characters")]
    public string? RecipientId { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(300, ErrorMessage = "Email cannot exceed 300 characters")]
    public string? RecipientEmail { get; set; }

    [Phone(ErrorMessage = "Invalid phone format")]
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? RecipientPhone { get; set; }

    [Required(ErrorMessage = "Notification type is required")]
    public NotificationType NotificationType { get; set; }

    [Required(ErrorMessage = "Channel is required")]
    public NotificationChannel Channel { get; set; }

    [Required(ErrorMessage = "Subject is required")]
    [StringLength(300, ErrorMessage = "Subject cannot exceed 300 characters")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
    public string Message { get; set; } = string.Empty;

    public Guid? RelatedEntityId { get; set; }

    [StringLength(100, ErrorMessage = "Related entity type cannot exceed 100 characters")]
    public string? RelatedEntityType { get; set; }

    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
}