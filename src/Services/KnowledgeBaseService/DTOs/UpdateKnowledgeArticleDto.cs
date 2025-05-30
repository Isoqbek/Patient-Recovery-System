using System.ComponentModel.DataAnnotations;
using KnowledgeBaseService.Models;

namespace KnowledgeBaseService.DTOs;

public class UpdateKnowledgeArticleDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(300, ErrorMessage = "Title cannot exceed 300 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public ArticleCategory Category { get; set; }

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Keywords cannot exceed 1000 characters")]
    public string? Keywords { get; set; }

    [StringLength(200, ErrorMessage = "Source cannot exceed 200 characters")]
    public string? Source { get; set; }

    [StringLength(50, ErrorMessage = "Version cannot exceed 50 characters")]
    public string? Version { get; set; }

    public bool IsVerified { get; set; }

    public bool IsPublished { get; set; }

    [StringLength(200, ErrorMessage = "Author/Editor cannot exceed 200 characters")]
    public string? AuthorOrEditor { get; set; }

    public ArticlePriority Priority { get; set; }

    [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters")]
    public string? Summary { get; set; }

    [StringLength(2000, ErrorMessage = "References cannot exceed 2000 characters")]
    public string? References { get; set; }

    public DateTime? LastReviewedDate { get; set; }

    public DateTime? NextReviewDate { get; set; }

    [StringLength(100, ErrorMessage = "Language cannot exceed 100 characters")]
    public string? Language { get; set; }
}