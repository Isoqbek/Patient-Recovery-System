using System.ComponentModel.DataAnnotations;

namespace KnowledgeBaseService.Models;

public class KnowledgeArticle
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public ArticleCategory Category { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Keywords { get; set; }

    [MaxLength(200)]
    public string? Source { get; set; }

    [MaxLength(50)]
    public string? Version { get; set; }

    public bool IsVerified { get; set; }

    public bool IsPublished { get; set; }

    [MaxLength(200)]
    public string? AuthorOrEditor { get; set; }

    public ArticlePriority Priority { get; set; }

    [MaxLength(500)]
    public string? Summary { get; set; }

    [MaxLength(2000)]
    public string? References { get; set; }

    public DateTime? LastReviewedDate { get; set; }

    public DateTime? NextReviewDate { get; set; }

    public int ViewCount { get; set; }

    [MaxLength(100)]
    public string? Language { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public enum ArticleCategory
{
    DiagnosisGuideline = 1,
    TreatmentProtocol = 2,
    SymptomManagement = 3,
    DrugInformation = 4,
    RehabilitationExercise = 5,
    SimilarCaseStudy = 6,
    EmergencyProcedure = 7,
    PreventiveCare = 8,
    LabTestReference = 9,
    MedicalDevices = 10,
    PatientEducation = 11,
    Unknown = 99
}

public enum ArticlePriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}