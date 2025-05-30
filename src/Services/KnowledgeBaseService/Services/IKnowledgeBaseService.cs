using KnowledgeBaseService.DTOs;

namespace KnowledgeBaseService.Services;

public interface IKnowledgeBaseService
{
    // Article Management
    Task<IEnumerable<KnowledgeArticleDto>> GetArticlesAsync(KnowledgeFilterDto filter);
    Task<KnowledgeArticleDto?> GetArticleByIdAsync(Guid id);
    Task<KnowledgeArticleDto> CreateArticleAsync(CreateKnowledgeArticleDto createDto);
    Task<KnowledgeArticleDto?> UpdateArticleAsync(Guid id, UpdateKnowledgeArticleDto updateDto);
    Task<bool> DeleteArticleAsync(Guid id);
    Task<bool> ArticleExistsAsync(Guid id);

    // Publishing and Verification
    Task<bool> PublishArticleAsync(Guid id);
    Task<bool> UnpublishArticleAsync(Guid id);
    Task<bool> VerifyArticleAsync(Guid id, string verifiedBy);
    Task<bool> UnverifyArticleAsync(Guid id);

    // Search and Discovery
    Task<IEnumerable<KnowledgeArticleDto>> SearchArticlesAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<IEnumerable<KnowledgeArticleDto>> GetArticlesByCategoryAsync(Models.ArticleCategory category, int page = 1, int pageSize = 10);
    Task<IEnumerable<KnowledgeArticleDto>> GetArticlesByKeywordsAsync(string keywords, int page = 1, int pageSize = 10);
    Task<IEnumerable<KnowledgeArticleDto>> GetRelatedArticlesAsync(Guid articleId, int count = 5);
    Task<IEnumerable<KnowledgeArticleDto>> GetPopularArticlesAsync(int count = 10);

    // Analytics and Reports
    Task<object> GetArticleStatisticsAsync();
    Task<IEnumerable<KnowledgeArticleDto>> GetRecentArticlesAsync(int count = 10);
    Task<IEnumerable<KnowledgeArticleDto>> GetArticlesNeedingReviewAsync();
    Task<bool> IncrementViewCountAsync(Guid id);

    // Content Management
    Task<bool> MarkForReviewAsync(Guid id, DateTime nextReviewDate);
    Task<bool> CompleteReviewAsync(Guid id, string reviewedBy);
    Task<IEnumerable<string>> GetUniqueSourcesAsync();
    Task<IEnumerable<string>> GetUniqueAuthorsAsync();
}