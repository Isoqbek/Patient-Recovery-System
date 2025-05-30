using System.ComponentModel.DataAnnotations;
using RehabilitationService.Models;

namespace RehabilitationService.DTOs;

public class CreateProgressLogDto
{
    [Required(ErrorMessage = "Rehabilitation plan ID is required")]
    public Guid RehabilitationPlanId { get; set; }

    [Required(ErrorMessage = "Log date is required")]
    public DateTime LogDate { get; set; }

    [Required(ErrorMessage = "Notes are required")]
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string Notes { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Activity details cannot exceed 2000 characters")]
    public string? ActivityDetails { get; set; }

    [Range(1, 10, ErrorMessage = "Pain level must be between 1 and 10")]
    public int? PainLevel { get; set; }

    [Range(1, 10, ErrorMessage = "Energy level must be between 1 and 10")]
    public int? EnergyLevel { get; set; }

    [Range(1, 10, ErrorMessage = "Mood level must be between 1 and 10")]
    public int? MoodLevel { get; set; }

    [StringLength(200, ErrorMessage = "Submitted by cannot exceed 200 characters")]
    public string? SubmittedBy { get; set; }

    [Required(ErrorMessage = "Progress type is required")]
    public ProgressType ProgressType { get; set; }

    [Required(ErrorMessage = "Completion status is required")]
    public CompletionStatus CompletionStatus { get; set; }

    [Range(1, 480, ErrorMessage = "Duration must be between 1 and 480 minutes")]
    public int? DurationMinutes { get; set; }

    [StringLength(500, ErrorMessage = "Challenges cannot exceed 500 characters")]
    public string? Challenges { get; set; }

    [StringLength(500, ErrorMessage = "Achievements cannot exceed 500 characters")]
    public string? Achievements { get; set; }

    [StringLength(500, ErrorMessage = "Therapist notes cannot exceed 500 characters")]
    public string? TherapistNotes { get; set; }
}