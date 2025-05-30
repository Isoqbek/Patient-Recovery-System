using MonitoringService.Models;

namespace MonitoringService.DTOs;

public class AlertFilterDto
{
    public Guid? PatientId { get; set; }
    public AlertSeverity? Severity { get; set; }
    public AlertStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? AcknowledgedBy { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "AlertDateTime";
    public bool SortDescending { get; set; } = true;
}