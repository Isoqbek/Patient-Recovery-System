using System.ComponentModel.DataAnnotations;
using RehabilitationService.Models;

namespace RehabilitationService.DTOs;

public class CreateRehabilitationPlanDto
{
    [Required(ErrorMessage = "Patient ID is required")]
    public Guid PatientId { get; set; }

    [Required(ErrorMessage = "Plan name is required")]
    [StringLength(200, ErrorMessage = "Plan name cannot exceed 200 characters")]
    public string PlanName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required(ErrorMessage = "Goals are required")]
    [StringLength(2000, ErrorMessage = "Goals cannot exceed 2000 characters")]
    public string Goals { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Assigned therapist cannot exceed 200 characters")]
    public string? AssignedTherapist { get; set; }

    [StringLength(200, ErrorMessage = "Created by cannot exceed 200 characters")]
    public string? CreatedBy { get; set; }

    [Required(ErrorMessage = "Plan type is required")]
    public PlanType PlanType { get; set; }

    public PlanDifficulty Difficulty { get; set; } = PlanDifficulty.Beginner;

    [Range(1, 104, ErrorMessage = "Estimated duration must be between 1 and 104 weeks")]
    public int EstimatedDurationWeeks { get; set; } = 4;

    [StringLength(500, ErrorMessage = "Special instructions cannot exceed 500 characters")]
    public string? SpecialInstructions { get; set; }
}