using System.ComponentModel.DataAnnotations;
using ClinicalRecordService.Models;

namespace ClinicalRecordService.DTOs;

public class UpdateClinicalEntryDto
{
    [Required(ErrorMessage = "Entry type is required")]
    public EntryType EntryType { get; set; }

    [Required(ErrorMessage = "Entry date and time is required")]
    public DateTime EntryDateTime { get; set; }

    [StringLength(200, ErrorMessage = "Recorded by cannot exceed 200 characters")]
    public string? RecordedBy { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    public object? Data { get; set; } // Will be serialized to JSON
}