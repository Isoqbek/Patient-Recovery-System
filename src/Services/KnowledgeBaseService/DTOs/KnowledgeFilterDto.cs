using KnowledgeBaseService.Models;

namespace KnowledgeBaseService.DTOs;

public class KnowledgeFilterDto
{
    public ArticleCategory? Category { get; set; }
    public string? Keywords { get; set; }
    public string? SearchTerm { get; set; }
    public string? Source { get; set; }
    public bool? IsVerified { get; set; }
    public bool? IsPublished { get; set; }
    public ArticlePriority? Priority { get; set; }
    public string? AuthorOrEditor { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? LastReviewedFrom { get; set; }
    public DateTime? LastReviewedTo { get; set; }
    public bool? NeedsReview { get; set; }
    public string? Language { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}