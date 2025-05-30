using ClinicalRecordService.Models;

namespace ClinicalRecordService.DTOs;

public class ClinicalEntryFilterDto
{
    public Guid? PatientId { get; set; }
    public EntryType? EntryType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? RecordedBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "EntryDateTime";
    public bool SortDescending { get; set; } = true;
}