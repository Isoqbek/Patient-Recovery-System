namespace KnowledgeBaseService.DTOs;

public class KnowledgeArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Keywords { get; set; }
    public string? Source { get; set; }
    public string? Version { get; set; }
    public bool IsVerified { get; set; }
    public bool IsPublished { get; set; }
    public string? AuthorOrEditor { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? References { get; set; }
    public DateTime? LastReviewedDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public int ViewCount { get; set; }
    public string? Language { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string FormattedCreatedAt { get; set; } = string.Empty;
    public string FormattedLastReviewed { get; set; } = string.Empty;
    public string FormattedNextReview { get; set; } = string.Empty;
    public bool NeedsReview { get; set; }
    public string ContentPreview { get; set; } = string.Empty;
    public List<string> KeywordList { get; set; } = new();
}