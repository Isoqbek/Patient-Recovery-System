namespace MonitoringService.DTOs;

public class AlertDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public DateTime AlertDateTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? TriggeringClinicalEntryId { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string FormattedAlertDateTime { get; set; } = string.Empty;
    public string TimeSinceCreated { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}