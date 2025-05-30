using System.ComponentModel.DataAnnotations;

namespace MonitoringService.Models;

public class Alert
{
    public Guid Id { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    [Required]
    public DateTime AlertDateTime { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public AlertSeverity Severity { get; set; }

    [Required]
    public AlertStatus Status { get; set; }

    public Guid? TriggeringClinicalEntryId { get; set; }

    [MaxLength(200)]
    public string? AcknowledgedBy { get; set; }

    public DateTime? AcknowledgedAt { get; set; }

    [MaxLength(200)]
    public string? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    [MaxLength(1000)]
    public string? ResolutionNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public enum AlertSeverity
{
    Information = 1,
    Warning = 2,
    Critical = 3
}

public enum AlertStatus
{
    New = 1,
    Acknowledged = 2,
    InProgress = 3,
    Resolved = 4,
    Closed = 5
}