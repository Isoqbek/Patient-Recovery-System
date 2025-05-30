namespace NotificationService.Events;

public class AlertCreatedEvent
{
    public Guid AlertId { get; set; }
    public Guid PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Severity { get; set; } = string.Empty;
    public DateTime AlertDateTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RecipientRoles { get; set; } // JSON array of roles
}