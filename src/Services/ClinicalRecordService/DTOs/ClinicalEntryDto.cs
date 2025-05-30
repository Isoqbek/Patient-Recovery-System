namespace ClinicalRecordService.DTOs;

public class ClinicalEntryDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public DateTime EntryDateTime { get; set; }
    public string? RecordedBy { get; set; }
    public string? Notes { get; set; }
    public object? Data { get; set; } // Parsed JSON object
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string FormattedDateTime { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}