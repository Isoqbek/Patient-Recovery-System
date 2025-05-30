using System.ComponentModel.DataAnnotations;
using MonitoringService.Models;

namespace MonitoringService.DTOs;

public class CreateAlertDto
{
    [Required(ErrorMessage = "Patient ID is required")]
    public Guid PatientId { get; set; }

    [Required(ErrorMessage = "Alert date and time is required")]
    public DateTime AlertDateTime { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Severity is required")]
    public AlertSeverity Severity { get; set; }

    public Guid? TriggeringClinicalEntryId { get; set; }
}