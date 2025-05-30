using System.ComponentModel.DataAnnotations;

namespace ClinicalRecordService.Models;

public class ClinicalEntry
{
    public Guid Id { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    [Required]
    public EntryType EntryType { get; set; }

    [Required]
    public DateTime EntryDateTime { get; set; }

    [MaxLength(200)]
    public string? RecordedBy { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public string? Data { get; set; } // JSON string for type-specific data

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public enum EntryType
{
    VitalSign = 1,      // Hayotiy ko'rsatkichlar (harorat, qon bosimi, puls)
    Symptom = 2,        // Simptomlar
    Observation = 3,    // Kuzatuvlar
    Medication = 4,     // Dori vositalari
    Procedure = 5,      // Muolajalar
    TestResult = 6,     // Test natijalari
    Note = 7,           // Umumiy qaydlar
    Diagnosis = 8,      // Tashxis
    Treatment = 9,      // Davolash
    Allergy = 10        // Allergiyalar
}