using NotificationService.Models;

namespace NotificationService.DTOs;

public class NotificationFilterDto
{
    public Guid? PatientId { get; set; }
    public string? RecipientType { get; set; }
    public NotificationType? NotificationType { get; set; }
    public NotificationChannel? Channel { get; set; }
    public NotificationStatus? Status { get; set; }
    public NotificationPriority? Priority { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? RelatedEntityType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}