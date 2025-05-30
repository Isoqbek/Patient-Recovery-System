namespace RehabilitationService.DTOs;

public class ProgressLogDto
{
    public Guid Id { get; set; }
    public Guid RehabilitationPlanId { get; set; }
    public DateTime LogDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? ActivityDetails { get; set; }
    public int? PainLevel { get; set; }
    public int? EnergyLevel { get; set; }
    public int? MoodLevel { get; set; }
    public string? SubmittedBy { get; set; }
    public string ProgressType { get; set; } = string.Empty;
    public string CompletionStatus { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public string? Challenges { get; set; }
    public string? Achievements { get; set; }
    public string? TherapistNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string FormattedLogDate { get; set; } = string.Empty;
    public string FormattedDuration { get; set; } = string.Empty;
    public string PainLevelDescription { get; set; } = string.Empty;
    public string EnergyLevelDescription { get; set; } = string.Empty;
    public string MoodLevelDescription { get; set; } = string.Empty;
}