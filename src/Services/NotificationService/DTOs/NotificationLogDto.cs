namespace NotificationService.DTOs;

public class NotificationLogDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string RecipientType { get; set; } = string.Empty;
    public string? RecipientId { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string FormattedCreatedAt { get; set; } = string.Empty;
    public string FormattedSentAt { get; set; } = string.Empty;
}