using RehabilitationService.Models;

namespace RehabilitationService.DTOs;

public class ProgressFilterDto
{
    public Guid? RehabilitationPlanId { get; set; }
    public Guid? PatientId { get; set; }
    public ProgressType? ProgressType { get; set; }
    public CompletionStatus? CompletionStatus { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? LogDateFrom { get; set; }
    public DateTime? LogDateTo { get; set; }
    public int? MinPainLevel { get; set; }
    public int? MaxPainLevel { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "LogDate";
    public bool SortDescending { get; set; } = true;
}