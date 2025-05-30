using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KnowledgeBaseService.DTOs;
using KnowledgeBaseService.Models;
using KnowledgeBaseService.Services;

namespace KnowledgeBaseService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KnowledgeArticlesController : ControllerBase
{
    private readonly IKnowledgeBaseService _knowledgeBaseService;
    private readonly ILogger<KnowledgeArticlesController> _logger;

    public KnowledgeArticlesController(IKnowledgeBaseService knowledgeBaseService, ILogger<KnowledgeArticlesController> logger)
    {
        _knowledgeBaseService = knowledgeBaseService;
        _logger = logger;
    }

    /// <summary>
    /// Get knowledge articles with filtering
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> GetArticles([FromQuery] KnowledgeFilterDto filter)
    {
        try
        {
            var articles = await _knowledgeBaseService.GetArticlesAsync(filter);
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving knowledge articles");
            return StatusCode(500, "An error occurred while retrieving knowledge articles.");
        }
    }

    /// <summary>
    /// Get knowledge article by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public async Task<ActionResult<KnowledgeArticleDto>> GetArticleById(Guid id)
    {
        try
        {
            var article = await _knowledgeBaseService.GetArticleByIdAsync(id);

            if (article == null)
                return NotFound($"Knowledge article with ID {id} not found.");

            // Increment view count
            await _knowledgeBaseService.IncrementViewCountAsync(id);

            return Ok(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving knowledge article with ID: {ArticleId}", id);
            return StatusCode(500, "An error occurred while retrieving the knowledge article.");
        }
    }

    /// <summary>
    /// Search knowledge articles
    /// </summary>
    [HttpGet("search")]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> SearchArticles(
        [FromQuery] string searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required.");

            if (page < 1 || pageSize < 1 || pageSize > 50)
                return BadRequest("Invalid pagination parameters. Page must be >= 1 and PageSize must be between 1 and 50.");

            var articles = await _knowledgeBaseService.SearchArticlesAsync(searchTerm, page, pageSize);
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching knowledge articles with term: {SearchTerm}", searchTerm);
            return StatusCode(500, "An error occurred while searching knowledge articles.");
        }
    }

    /// <summary>
    /// Get articles by category
    /// </summary>
    [HttpGet("category/{category}")]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> GetArticlesByCategory(
        ArticleCategory category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 50)
                return BadRequest("Invalid pagination parameters. Page must be >= 1 and PageSize must be between 1 and 50.");

            var articles = await _knowledgeBaseService.GetArticlesByCategoryAsync(category, page, pageSize);
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving articles by category: {Category}", category);
            return StatusCode(500, "An error occurred while retrieving articles by category.");
        }
    }

    /// <summary>
    /// Get articles by keywords
    /// </summary>
    [HttpGet("keywords")]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> GetArticlesByKeywords(
        [FromQuery] string keywords,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keywords))
                return BadRequest("Keywords are required.");

            if (page < 1 || pageSize < 1 || pageSize > 50)
                return BadRequest("Invalid pagination parameters. Page must be >= 1 and PageSize must be between 1 and 50.");

            var articles = await _knowledgeBaseService.GetArticlesByKeywordsAsync(keywords, page, pageSize);
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving articles by keywords: {Keywords}", keywords);
            return StatusCode(500, "An error occurred while retrieving articles by keywords.");
        }
    }

    /// <summary>
    /// Get related articles
    /// </summary>
    [HttpGet("{id:guid}/related")]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> GetRelatedArticles(
        Guid id,
        [FromQuery] int count = 5)
    {
        try
        {
            if (count < 1 || count > 20)
                return BadRequest("Count must be between 1 and 20.");

            var articles = await _knowledgeBaseService.GetRelatedArticlesAsync(id, count);
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving related articles for: {ArticleId}", id);
            return StatusCode(500, "An error occurred while retrieving related articles.");
        }
    }

    /// <summary>
    /// Get popular articles
    /// </summary>
    [HttpGet("popular")]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> GetPopularArticles([FromQuery] int count = 10)
    {
        try
        {
            if (count < 1 || count > 50)
                return BadRequest("Count must be between 1 and 50.");

            var articles = await _knowledgeBaseService.GetPopularArticlesAsync(count);
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving popular articles");
            return StatusCode(500, "An error occurred while retrieving popular articles.");
        }
    }

    /// <summary>
    /// Get recent articles
    /// </summary>
    [HttpGet("recent")]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> GetRecentArticles([FromQuery] int count = 10)
    {
        try
        {
            if (count < 1 || count > 50)
                return BadRequest("Count must be between 1 and 50.");

            var articles = await _knowledgeBaseService.GetRecentArticlesAsync(count);
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving recent articles");
            return StatusCode(500, "An error occurred while retrieving recent articles.");
        }
    }

    /// <summary>
    /// Get articles needing review
    /// </summary>
    [HttpGet("needing-review")]
    [Authorize(Policy = "KnowledgeWritePolicy")]
    public async Task<ActionResult<IEnumerable<KnowledgeArticleDto>>> GetArticlesNeedingReview()
    {
        try
        {
            var articles = await _knowledgeBaseService.GetArticlesNeedingReviewAsync();
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving articles needing review");
            return StatusCode(500, "An error occurred while retrieving articles needing review.");
        }
    }

    /// <summary>
    /// Create a new knowledge article
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "KnowledgeWritePolicy")]
    public async Task<ActionResult<KnowledgeArticleDto>> CreateArticle([FromBody] CreateKnowledgeArticleDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var article = await _knowledgeBaseService.CreateArticleAsync(createDto);

            return CreatedAtAction(
                nameof(GetArticleById),
                new { id = article.Id },
                article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating knowledge article");
            return StatusCode(500, "An error occurred while creating the knowledge article.");
        }
    }

    /// <summary>
    /// Update an existing knowledge article
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "KnowledgeWritePolicy")]
    public async Task<ActionResult<KnowledgeArticleDto>> UpdateArticle(Guid id, [FromBody] UpdateKnowledgeArticleDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var article = await _knowledgeBaseService.UpdateArticleAsync(id, updateDto);

            if (article == null)
                return NotFound($"Knowledge article with ID {id} not found.");

            return Ok(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating knowledge article with ID: {ArticleId}", id);
            return StatusCode(500, "An error occurred while updating the knowledge article.");
        }
    }

    /// <summary>
    /// Delete a knowledge article
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "KnowledgeWritePolicy")]
    public async Task<ActionResult> DeleteArticle(Guid id)
    {
        try
        {
            var deleted = await _knowledgeBaseService.DeleteArticleAsync(id);

            if (!deleted)
                return NotFound($"Knowledge article with ID {id} not found.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting knowledge article with ID: {ArticleId}", id);
            return StatusCode(500, "An error occurred while deleting the knowledge article.");
        }
    }

    /// <summary>
    /// Publish an article
    /// </summary>
    [HttpPatch("{id:guid}/publish")]
    [Authorize(Policy = "KnowledgeWritePolicy")]
    public async Task<ActionResult> PublishArticle(Guid id)
    {
        try
        {
            var success = await _knowledgeBaseService.PublishArticleAsync(id);

            if (!success)
                return NotFound($"Knowledge article with ID {id} not found.");

            return Ok(new { message = "Article published successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while publishing article with ID: {ArticleId}", id);
            return StatusCode(500, "An error occurred while publishing the article.");
        }
    }

    /// <summary>
    /// Unpublish an article
    /// </summary>
    [HttpPatch("{id:guid}/unpublish")]
    [Authorize(Policy = "KnowledgeWritePolicy")]
    public async Task<ActionResult> UnpublishArticle(Guid id)
    {
        try
        {
            var success = await _knowledgeBaseService.UnpublishArticleAsync(id);

            if (!success)
                return NotFound($"Knowledge article with ID {id} not found.");

            return Ok(new { message = "Article unpublished successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while unpublishing article with ID: {ArticleId}", id);
            return StatusCode(500, "An error occurred while unpublishing the article.");
        }
    }

    /// <summary>
    /// Verify an article
    /// </summary>
    [HttpPatch("{id:guid}/verify")]
    [Authorize(Policy = "KnowledgeWritePolicy")]
    public async Task<ActionResult> VerifyArticle(Guid id, [FromBody] VerifyArticleRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.VerifiedBy))
                return BadRequest("VerifiedBy is required.");

            var success = await _knowledgeBaseService.VerifyArticleAsync(id, request.VerifiedBy);

            if (!success)
                return NotFound($"Knowledge article with ID {id} not found.");

            return Ok(new { message = "Article verified successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while verifying article with ID: {ArticleId}", id);
            return StatusCode(500, "An error occurred while verifying the article.");
        }
    }

    /// <summary>
    /// Complete review for an article
    /// </summary>
    [HttpPatch("{id:guid}/complete-review")]
    [Authorize(Policy = "KnowledgeWritePolicy")]
    public async Task<ActionResult> CompleteReview(Guid id, [FromBody] CompleteReviewRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ReviewedBy))
                return BadRequest("ReviewedBy is required.");

            var success = await _knowledgeBaseService.CompleteReviewAsync(id, request.ReviewedBy);

            if (!success)
                return NotFound($"Knowledge article with ID {id} not found.");

            return Ok(new { message = "Article review completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing review for article with ID: {ArticleId}", id);
            return StatusCode(500, "An error occurred while completing article review.");
        }
    }

    /// <summary>
    /// Mark article for review
    /// </summary>
    [HttpPatch("{id:guid}/mark-for-review")]
    [Authorize(Policy = "KnowledgeWritePolicy")]
    public async Task<ActionResult> MarkForReview(Guid id, [FromBody] MarkForReviewRequest request)
    {
        try
        {
            if (request.NextReviewDate <= DateTime.UtcNow)
                return BadRequest("Next review date must be in the future.");

            var success = await _knowledgeBaseService.MarkForReviewAsync(id, request.NextReviewDate);

            if (!success)
                return NotFound($"Knowledge article with ID {id} not found.");

            return Ok(new { message = "Article marked for review successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking article for review with ID: {ArticleId}", id);
            return StatusCode(500, "An error occurred while marking article for review.");
        }
    }

    /// <summary>
    /// Get article statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public async Task<ActionResult<object>> GetArticleStatistics()
    {
        try
        {
            var statistics = await _knowledgeBaseService.GetArticleStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving article statistics");
            return StatusCode(500, "An error occurred while retrieving article statistics.");
        }
    }

    /// <summary>
    /// Get unique sources
    /// </summary>
    [HttpGet("sources")]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public async Task<ActionResult<IEnumerable<string>>> GetUniqueSources()
    {
        try
        {
            var sources = await _knowledgeBaseService.GetUniqueSourcesAsync();
            return Ok(sources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving unique sources");
            return StatusCode(500, "An error occurred while retrieving unique sources.");
        }
    }

    /// <summary>
    /// Get unique authors
    /// </summary>
    [HttpGet("authors")]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public async Task<ActionResult<IEnumerable<string>>> GetUniqueAuthors()
    {
        try
        {
            var authors = await _knowledgeBaseService.GetUniqueAuthorsAsync();
            return Ok(authors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving unique authors");
            return StatusCode(500, "An error occurred while retrieving unique authors.");
        }
    }

    /// <summary>
    /// Get all available article categories
    /// </summary>
    [HttpGet("categories")]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public ActionResult<object> GetArticleCategories()
    {
        try
        {
            var categories = Enum.GetValues<ArticleCategory>()
                .Select(c => new {
                    Value = (int)c,
                    Name = c.ToString(),
                    DisplayName = GetCategoryDisplayName(c)
                });

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving article categories");
            return StatusCode(500, "An error occurred while retrieving article categories.");
        }
    }

    /// <summary>
    /// Get all available article priorities
    /// </summary>
    [HttpGet("priorities")]
    [Authorize(Policy = "KnowledgeReadPolicy")]
    public ActionResult<object> GetArticlePriorities()
    {
        try
        {
            var priorities = Enum.GetValues<ArticlePriority>()
                .Select(p => new {
                    Value = (int)p,
                    Name = p.ToString(),
                    DisplayName = GetPriorityDisplayName(p)
                });

            return Ok(priorities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving article priorities");
            return StatusCode(500, "An error occurred while retrieving article priorities.");
        }
    }

    private static string GetCategoryDisplayName(ArticleCategory category)
    {
        return category switch
        {
            ArticleCategory.DiagnosisGuideline => "Diagnosis Guideline",
            ArticleCategory.TreatmentProtocol => "Treatment Protocol",
            ArticleCategory.SymptomManagement => "Symptom Management",
            ArticleCategory.DrugInformation => "Drug Information",
            ArticleCategory.RehabilitationExercise => "Rehabilitation Exercise",
            ArticleCategory.SimilarCaseStudy => "Similar Case Study",
            ArticleCategory.EmergencyProcedure => "Emergency Procedure",
            ArticleCategory.PreventiveCare => "Preventive Care",
            ArticleCategory.LabTestReference => "Lab Test Reference",
            ArticleCategory.MedicalDevices => "Medical Devices",
            ArticleCategory.PatientEducation => "Patient Education",
            ArticleCategory.Unknown => "Unknown",
            _ => category.ToString()
        };
    }

    private static string GetPriorityDisplayName(ArticlePriority priority)
    {
        return priority switch
        {
            ArticlePriority.Low => "Low",
            ArticlePriority.Normal => "Normal",
            ArticlePriority.High => "High",
            ArticlePriority.Critical => "Critical",
            _ => priority.ToString()
        };
    }
}

public class VerifyArticleRequest
{
    public string VerifiedBy { get; set; } = string.Empty;
}

public class CompleteReviewRequest
{
    public string ReviewedBy { get; set; } = string.Empty;
}

public class MarkForReviewRequest
{
    public DateTime NextReviewDate { get; set; }
}