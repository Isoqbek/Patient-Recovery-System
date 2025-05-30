namespace RehabilitationService.DTOs;

public class RehabilitationPlanDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Goals { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AssignedTherapist { get; set; }
    public string? CreatedBy { get; set; }
    public string PlanType { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int EstimatedDurationWeeks { get; set; }
    public string? SpecialInstructions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string FormattedStartDate { get; set; } = string.Empty;
    public string FormattedEndDate { get; set; } = string.Empty;
    public int DaysActive { get; set; }
    public int ProgressLogCount { get; set; }
    public bool IsActive { get; set; }
    public double CompletionPercentage { get; set; }
    public List<ProgressLogDto> RecentProgressLogs { get; set; } = new();
}