using System.ComponentModel.DataAnnotations;
using MonitoringService.Models;

namespace MonitoringService.DTOs;

public class UpdateAlertDto
{
    [Required(ErrorMessage = "Status is required")]
    public AlertStatus Status { get; set; }

    [StringLength(200, ErrorMessage = "Acknowledged by cannot exceed 200 characters")]
    public string? AcknowledgedBy { get; set; }

    [StringLength(200, ErrorMessage = "Resolved by cannot exceed 200 characters")]
    public string? ResolvedBy { get; set; }

    [StringLength(1000, ErrorMessage = "Resolution notes cannot exceed 1000 characters")]
    public string? ResolutionNotes { get; set; }
}