using RehabilitationService.Models;

namespace RehabilitationService.DTOs;

public class RehabilitationFilterDto
{
    public Guid? PatientId { get; set; }
    public PlanStatus? Status { get; set; }
    public PlanType? PlanType { get; set; }
    public PlanDifficulty? Difficulty { get; set; }
    public string? AssignedTherapist { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "StartDate";
    public bool SortDescending { get; set; } = true;
}