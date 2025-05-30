using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models;

public class NotificationLog
{
    public Guid Id { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    [Required]
    [MaxLength(200)]
    public string RecipientType { get; set; } = string.Empty; // "Physician", "Nurse", "Patient", etc.

    [MaxLength(200)]
    public string? RecipientId { get; set; } // Employee ID, Patient ID, etc.

    [MaxLength(300)]
    public string? RecipientEmail { get; set; }

    [MaxLength(20)]
    public string? RecipientPhone { get; set; }

    [Required]
    public NotificationType NotificationType { get; set; }

    [Required]
    public NotificationChannel Channel { get; set; }

    [Required]
    [MaxLength(300)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public NotificationStatus Status { get; set; }

    public DateTime? SentAt { get; set; }

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; } = 0;

    public Guid? RelatedEntityId { get; set; } // AlertId, AppointmentId, etc.

    [MaxLength(100)]
    public string? RelatedEntityType { get; set; } // "Alert", "Appointment", etc.

    public NotificationPriority Priority { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public enum NotificationType
{
    Alert = 1,
    Appointment = 2,
    Medication = 3,
    TestResult = 4,
    General = 5,
    Emergency = 6,
    Reminder = 7
}

public enum NotificationChannel
{
    Email = 1,
    SMS = 2,
    Push = 3,
    InApp = 4,
    Console = 5 // For testing/development
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Cancelled = 4
}

public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}