using System.ComponentModel.DataAnnotations;

namespace RehabilitationService.Models;

public class RehabilitationProgressLog
{
    public Guid Id { get; set; }

    [Required]
    public Guid RehabilitationPlanId { get; set; }

    [Required]
    public DateTime LogDate { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? ActivityDetails { get; set; }

    public int? PainLevel { get; set; } // 1-10 scale

    public int? EnergyLevel { get; set; } // 1-10 scale

    public int? MoodLevel { get; set; } // 1-10 scale

    [MaxLength(200)]
    public string? SubmittedBy { get; set; }

    public ProgressType ProgressType { get; set; }

    public CompletionStatus CompletionStatus { get; set; }

    public int? DurationMinutes { get; set; }

    [MaxLength(500)]
    public string? Challenges { get; set; }

    [MaxLength(500)]
    public string? Achievements { get; set; }

    [MaxLength(500)]
    public string? TherapistNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public RehabilitationPlan? RehabilitationPlan { get; set; }
}

public enum ProgressType
{
    Exercise = 1,
    Therapy = 2,
    Assessment = 3,
    Milestone = 4,
    SelfReport = 5,
    TherapistEvaluation = 6
}

public enum CompletionStatus
{
    NotStarted = 1,
    InProgress = 2,
    Completed = 3,
    PartiallyCompleted = 4,
    Skipped = 5,
    Modified = 6
}