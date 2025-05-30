using RehabilitationService.DTOs;

namespace RehabilitationService.Services;

public interface IRehabilitationService
{
    // Rehabilitation Plans
    Task<IEnumerable<RehabilitationPlanDto>> GetRehabilitationPlansAsync(RehabilitationFilterDto filter);
    Task<RehabilitationPlanDto?> GetRehabilitationPlanByIdAsync(Guid id);
    Task<IEnumerable<RehabilitationPlanDto>> GetRehabilitationPlansByPatientIdAsync(Guid patientId);
    Task<RehabilitationPlanDto> CreateRehabilitationPlanAsync(CreateRehabilitationPlanDto createDto);
    Task<RehabilitationPlanDto?> UpdateRehabilitationPlanAsync(Guid id, UpdateRehabilitationPlanDto updateDto);
    Task<bool> DeleteRehabilitationPlanAsync(Guid id);
    Task<bool> RehabilitationPlanExistsAsync(Guid id);

    // Plan Status Management
    Task<bool> ActivatePlanAsync(Guid id);
    Task<bool> CompletePlanAsync(Guid id);
    Task<bool> PutPlanOnHoldAsync(Guid id, string reason);
    Task<bool> CancelPlanAsync(Guid id, string reason);

    // Progress Logs
    Task<IEnumerable<ProgressLogDto>> GetProgressLogsAsync(ProgressFilterDto filter);
    Task<ProgressLogDto?> GetProgressLogByIdAsync(Guid id);
    Task<IEnumerable<ProgressLogDto>> GetProgressLogsByPlanIdAsync(Guid planId, int page = 1, int pageSize = 10);
    Task<ProgressLogDto> CreateProgressLogAsync(CreateProgressLogDto createDto);
    Task<bool> DeleteProgressLogAsync(Guid id);

    // Analytics and Reports
    Task<object> GetPlanStatisticsAsync(Guid planId);
    Task<object> GetPatientProgressSummaryAsync(Guid patientId);
    Task<IEnumerable<RehabilitationPlanDto>> GetActivePlansAsync();
    Task<IEnumerable<RehabilitationPlanDto>> GetPlansNeedingAttentionAsync();
}