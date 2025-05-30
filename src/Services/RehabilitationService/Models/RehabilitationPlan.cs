using System.ComponentModel.DataAnnotations;

namespace RehabilitationService.Models;

public class RehabilitationPlan
{
    public Guid Id { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    [Required]
    [MaxLength(200)]
    public string PlanName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Goals { get; set; } = string.Empty;

    [Required]
    public PlanStatus Status { get; set; }

    [MaxLength(200)]
    public string? AssignedTherapist { get; set; }

    [MaxLength(200)]
    public string? CreatedBy { get; set; }

    public PlanType PlanType { get; set; }

    public PlanDifficulty Difficulty { get; set; }

    public int EstimatedDurationWeeks { get; set; }

    [MaxLength(500)]
    public string? SpecialInstructions { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public ICollection<RehabilitationProgressLog> ProgressLogs { get; set; } = new List<RehabilitationProgressLog>();
}

public enum PlanStatus
{
    Pending = 1,
    Active = 2,
    Completed = 3,
    OnHold = 4,
    Cancelled = 5,
    Discontinued = 6
}

public enum PlanType
{
    Physical = 1,
    Occupational = 2,
    Speech = 3,
    Cardiac = 4,
    Neurological = 5,
    Orthopedic = 6,
    General = 7
}

public enum PlanDifficulty
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
    Expert = 4
}