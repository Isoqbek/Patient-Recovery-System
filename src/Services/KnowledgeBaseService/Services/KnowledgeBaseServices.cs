using AutoMapper;
using Microsoft.EntityFrameworkCore;
using KnowledgeBaseService.Data;
using KnowledgeBaseService.DTOs;
using KnowledgeBaseService.Models;

namespace KnowledgeBaseService.Services;

public class KnowledgeBaseServices : IKnowledgeBaseService
{
    private readonly KnowledgeBaseDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<KnowledgeBaseServices> _logger;

    public KnowledgeBaseServices(KnowledgeBaseDbContext context, IMapper mapper, ILogger<KnowledgeBaseServices> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    #region Article Management

    public async Task<IEnumerable<KnowledgeArticleDto>> GetArticlesAsync(KnowledgeFilterDto filter)
    {
        try
        {
            var query = _context.KnowledgeArticles.AsQueryable();

            // Apply filters
            if (filter.Category.HasValue)
                query = query.Where(a => a.Category == filter.Category.Value);

            if (!string.IsNullOrEmpty(filter.Keywords))
            {
                var keywords = filter.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim().ToLower());
                query = query.Where(a => keywords.Any(k => a.Keywords != null && a.Keywords.ToLower().Contains(k)));
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchLower = filter.SearchTerm.ToLower();
                query = query.Where(a =>
                    a.Title.ToLower().Contains(searchLower) ||
                    a.Content.ToLower().Contains(searchLower) ||
                    (a.Summary != null && a.Summary.ToLower().Contains(searchLower)));
            }

            if (!string.IsNullOrEmpty(filter.Source))
                query = query.Where(a => a.Source != null && a.Source.Contains(filter.Source));

            if (filter.IsVerified.HasValue)
                query = query.Where(a => a.IsVerified == filter.IsVerified.Value);

            if (filter.IsPublished.HasValue)
                query = query.Where(a => a.IsPublished == filter.IsPublished.Value);

            if (filter.Priority.HasValue)
                query = query.Where(a => a.Priority == filter.Priority.Value);

            if (!string.IsNullOrEmpty(filter.AuthorOrEditor))
                query = query.Where(a => a.AuthorOrEditor != null && a.AuthorOrEditor.Contains(filter.AuthorOrEditor));

            if (filter.CreatedFrom.HasValue)
                query = query.Where(a => a.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(a => a.CreatedAt <= filter.CreatedTo.Value);

            if (filter.NeedsReview.HasValue && filter.NeedsReview.Value)
                query = query.Where(a => a.NextReviewDate.HasValue && a.NextReviewDate.Value <= DateTime.UtcNow);

            if (!string.IsNullOrEmpty(filter.Language))
                query = query.Where(a => a.Language == filter.Language);

            // Apply sorting
            if (filter.SortBy?.ToLower() == "title")
                query = filter.SortDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title);
            else if (filter.SortBy?.ToLower() == "category")
                query = filter.SortDescending ? query.OrderByDescending(a => a.Category) : query.OrderBy(a => a.Category);
            else if (filter.SortBy?.ToLower() == "priority")
                query = filter.SortDescending ? query.OrderByDescending(a => a.Priority) : query.OrderBy(a => a.Priority);
            else if (filter.SortBy?.ToLower() == "viewcount")
                query = filter.SortDescending ? query.OrderByDescending(a => a.ViewCount) : query.OrderBy(a => a.ViewCount);
            else if (filter.SortBy?.ToLower() == "updatedat")
                query = filter.SortDescending ? query.OrderByDescending(a => a.UpdatedAt) : query.OrderBy(a => a.UpdatedAt);
            else // Default: CreatedAt
                query = filter.SortDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt);

            // Apply pagination
            var articles = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} knowledge articles with filter", articles.Count);

            return _mapper.Map<IEnumerable<KnowledgeArticleDto>>(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving knowledge articles with filter");
            throw;
        }
    }

    public async Task<KnowledgeArticleDto?> GetArticleByIdAsync(Guid id)
    {
        try
        {
            var article = await _context.KnowledgeArticles.FindAsync(id);

            if (article == null)
                return null;

            return _mapper.Map<KnowledgeArticleDto>(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving knowledge article with ID: {ArticleId}", id);
            throw;
        }
    }

    public async Task<KnowledgeArticleDto> CreateArticleAsync(CreateKnowledgeArticleDto createDto)
    {
        try
        {
            var article = _mapper.Map<KnowledgeArticle>(createDto);
            article.Id = Guid.NewGuid();
            article.ViewCount = 0;

            _context.KnowledgeArticles.Add(article);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new knowledge article with ID: {ArticleId}", article.Id);

            return _mapper.Map<KnowledgeArticleDto>(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating knowledge article");
            throw;
        }
    }

    public async Task<KnowledgeArticleDto?> UpdateArticleAsync(Guid id, UpdateKnowledgeArticleDto updateDto)
    {
        try
        {
            var article = await _context.KnowledgeArticles.FindAsync(id);

            if (article == null)
                return null;

            _mapper.Map(updateDto, article);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated knowledge article with ID: {ArticleId}", id);

            return _mapper.Map<KnowledgeArticleDto>(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating knowledge article with ID: {ArticleId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteArticleAsync(Guid id)
    {
        try
        {
            var article = await _context.KnowledgeArticles.FindAsync(id);

            if (article == null)
                return false;

            _context.KnowledgeArticles.Remove(article);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted knowledge article with ID: {ArticleId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting knowledge article with ID: {ArticleId}", id);
            throw;
        }
    }

    public async Task<bool> ArticleExistsAsync(Guid id)
    {
        try
        {
            return await _context.KnowledgeArticles.AnyAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if knowledge article exists with ID: {ArticleId}", id);
            throw;
        }
    }

    #endregion

    #region Publishing and Verification

    public async Task<bool> PublishArticleAsync(Guid id)
    {
        try
        {
            var article = await _context.KnowledgeArticles.FindAsync(id);

            if (article == null)
                return false;

            article.IsPublished = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Published knowledge article with ID: {ArticleId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while publishing knowledge article with ID: {ArticleId}", id);
            throw;
        }
    }

    public async Task<bool> UnpublishArticleAsync(Guid id)
    {
        try
        {
            var article = await _context.KnowledgeArticles.FindAsync(id);

            if (article == null)
                return false;

            article.IsPublished = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Unpublished knowledge article with ID: {ArticleId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while unpublishing knowledge article with ID: {ArticleId}", id);
            throw;
        }
    }

    public async Task<bool> VerifyArticleAsync(Guid id, string verifiedBy)
    {
        try
        {
            var article = await _context.KnowledgeArticles.FindAsync(id);

            if (article == null)
                return false;

            article.IsVerified = true;
            article.LastReviewedDate = DateTime.UtcNow;
            article.AuthorOrEditor = verifiedBy;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Verified knowledge article {ArticleId} by {VerifiedBy}", id, verifiedBy);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while verifying knowledge article with ID: {ArticleId}", id);
            throw;
        }
    }

    public async Task<bool> UnverifyArticleAsync(Guid id)
    {
        try
        {
            var article = await _context.KnowledgeArticles.FindAsync(id);

            if (article == null)
                return false;

            article.IsVerified = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Unverified knowledge article with ID: {ArticleId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while unverifying knowledge article with ID: {ArticleId}", id);
            throw;
        }
    }

    #endregion

    #region Search and Discovery

    public async Task<IEnumerable<KnowledgeArticleDto>> SearchArticlesAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<KnowledgeArticleDto>();

            var searchLower = searchTerm.ToLower();
            var articles = await _context.KnowledgeArticles
                .Where(a => a.IsPublished && (
                    a.Title.ToLower().Contains(searchLower) ||
                    a.Content.ToLower().Contains(searchLower) ||
                    (a.Keywords != null && a.Keywords.ToLower().Contains(searchLower)) ||
                    (a.Summary != null && a.Summary.ToLower().Contains(searchLower))))
                .OrderByDescending(a => a.ViewCount)
                .ThenByDescending(a => a.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<KnowledgeArticleDto>>(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching knowledge articles with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeArticleDto>> GetArticlesByCategoryAsync(ArticleCategory category, int page = 1, int pageSize = 10)
    {
        try
        {
            var articles = await _context.KnowledgeArticles
                .Where(a => a.Category == category && a.IsPublished)
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<KnowledgeArticleDto>>(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving articles by category: {Category}", category);
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeArticleDto>> GetArticlesByKeywordsAsync(string keywords, int page = 1, int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keywords))
                return new List<KnowledgeArticleDto>();

            var keywordList = keywords.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim().ToLower());

            var articles = await _context.KnowledgeArticles
                .Where(a => a.IsPublished && keywordList.Any(k =>
                    a.Keywords != null && a.Keywords.ToLower().Contains(k)))
                .OrderByDescending(a => a.ViewCount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<KnowledgeArticleDto>>(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving articles by keywords: {Keywords}", keywords);
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeArticleDto>> GetRelatedArticlesAsync(Guid articleId, int count = 5)
    {
        try
        {
            var article = await _context.KnowledgeArticles.FindAsync(articleId);
            if (article == null)
                return new List<KnowledgeArticleDto>();

            var relatedArticles = await _context.KnowledgeArticles
                .Where(a => a.Id != articleId && a.IsPublished &&
                           (a.Category == article.Category ||
                            (article.Keywords != null && a.Keywords != null &&
                             article.Keywords.Split(',').Any(k => a.Keywords.Contains(k.Trim())))))
                .OrderByDescending(a => a.ViewCount)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<IEnumerable<KnowledgeArticleDto>>(relatedArticles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving related articles for: {ArticleId}", articleId);
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeArticleDto>> GetPopularArticlesAsync(int count = 10)
    {
        try
        {
            var articles = await _context.KnowledgeArticles
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.ViewCount)
                .ThenByDescending(a => a.UpdatedAt)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<IEnumerable<KnowledgeArticleDto>>(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving popular articles");
            throw;
        }
    }

    #endregion

    #region Analytics and Reports

    public async Task<object> GetArticleStatisticsAsync()
    {
        try
        {
            var totalArticles = await _context.KnowledgeArticles.CountAsync();
            var publishedArticles = await _context.KnowledgeArticles.CountAsync(a => a.IsPublished);
            var verifiedArticles = await _context.KnowledgeArticles.CountAsync(a => a.IsVerified);
            var articlesNeedingReview = await _context.KnowledgeArticles.CountAsync(a =>
                a.NextReviewDate.HasValue && a.NextReviewDate.Value <= DateTime.UtcNow);

            var categoryStats = await _context.KnowledgeArticles
                .GroupBy(a => a.Category)
                .Select(g => new { Category = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var totalViews = await _context.KnowledgeArticles.SumAsync(a => a.ViewCount);

            return new
            {
                totalArticles,
                publishedArticles,
                verifiedArticles,
                articlesNeedingReview,
                totalViews,
                categoryStats
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting article statistics");
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeArticleDto>> GetRecentArticlesAsync(int count = 10)
    {
        try
        {
            var articles = await _context.KnowledgeArticles
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<IEnumerable<KnowledgeArticleDto>>(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving recent articles");
            throw;
        }
    }

    public async Task<IEnumerable<KnowledgeArticleDto>> GetArticlesNeedingReviewAsync()
    {
        try
        {
            var articles = await _context.KnowledgeArticles
                .Where(a => a.NextReviewDate.HasValue && a.NextReviewDate.Value <= DateTime.UtcNow)
                .OrderBy(a => a.NextReviewDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<KnowledgeArticleDto>>(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving articles needing review");
            throw;
        }
    }

    public async Task<bool> IncrementViewCountAsync(Guid id)
    {
        try
        {
            var article = await _context.KnowledgeArticles.FindAsync(id);

            if (article == null)
                return false;

            article.ViewCount++;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while incrementing view count for article: {ArticleId}", id);
            throw;
        }
    }

    #endregion

    #region Content Management

    public async Task<bool> MarkForReviewAsync(Guid id, DateTime nextReviewDate)
    {
        try
        {
            var article = await _context.KnowledgeArticles.FindAsync(id);

            if (article == null)
                return false;

            article.NextReviewDate = nextReviewDate;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Marked article {ArticleId} for review on {ReviewDate}", id, nextReviewDate);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while marking article for review: {ArticleId}", id);
            throw;
        }
    }

    public async Task<bool> CompleteReviewAsync(Guid id, string reviewedBy)
    {
        try
        {
            var article = await _context.KnowledgeArticles.FindAsync(id);

            if (article == null)
                return false;

            article.LastReviewedDate = DateTime.UtcNow;
            article.NextReviewDate = DateTime.UtcNow.AddDays(365); // Next review in 1 year
            article.AuthorOrEditor = reviewedBy;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Completed review for article {ArticleId} by {ReviewedBy}", id, reviewedBy);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing review for article: {ArticleId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetUniqueSourcesAsync()
    {
        try
        {
            var sources = await _context.KnowledgeArticles
                .Where(a => !string.IsNullOrEmpty(a.Source))
                .Select(a => a.Source!)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            return sources;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving unique sources");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetUniqueAuthorsAsync()
    {
        try
        {
            var authors = await _context.KnowledgeArticles
                .Where(a => !string.IsNullOrEmpty(a.AuthorOrEditor))
                .Select(a => a.AuthorOrEditor!)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            return authors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving unique authors");
            throw;
        }
    }

    #endregion
}