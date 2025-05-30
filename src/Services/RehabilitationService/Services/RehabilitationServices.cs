using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RehabilitationService.Data;
using RehabilitationService.DTOs;
using RehabilitationService.Models;

namespace RehabilitationService.Services;

public class RehabilitationServices : IRehabilitationService
{
    private readonly RehabilitationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RehabilitationServices> _logger;

    public RehabilitationServices(RehabilitationDbContext context, IMapper mapper, ILogger<RehabilitationServices> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    #region Rehabilitation Plans

    public async Task<IEnumerable<RehabilitationPlanDto>> GetRehabilitationPlansAsync(RehabilitationFilterDto filter)
    {
        try
        {
            var query = _context.RehabilitationPlans
                .Include(p => p.ProgressLogs)
                .AsQueryable();

            // Apply filters
            if (filter.PatientId.HasValue)
                query = query.Where(p => p.PatientId == filter.PatientId.Value);

            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == filter.Status.Value);

            if (filter.PlanType.HasValue)
                query = query.Where(p => p.PlanType == filter.PlanType.Value);

            if (filter.Difficulty.HasValue)
                query = query.Where(p => p.Difficulty == filter.Difficulty.Value);

            if (!string.IsNullOrEmpty(filter.AssignedTherapist))
                query = query.Where(p => p.AssignedTherapist != null && p.AssignedTherapist.Contains(filter.AssignedTherapist));

            if (filter.StartDateFrom.HasValue)
                query = query.Where(p => p.StartDate >= filter.StartDateFrom.Value);

            if (filter.StartDateTo.HasValue)
                query = query.Where(p => p.StartDate <= filter.StartDateTo.Value);

            if (filter.IsActive.HasValue && filter.IsActive.Value)
                query = query.Where(p => p.Status == PlanStatus.Active);

            // Apply sorting
            if (filter.SortBy?.ToLower() == "planname")
                query = filter.SortDescending ? query.OrderByDescending(p => p.PlanName) : query.OrderBy(p => p.PlanName);
            else if (filter.SortBy?.ToLower() == "status")
                query = filter.SortDescending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status);
            else if (filter.SortBy?.ToLower() == "plantype")
                query = filter.SortDescending ? query.OrderByDescending(p => p.PlanType) : query.OrderBy(p => p.PlanType);
            else // Default: StartDate
                query = filter.SortDescending ? query.OrderByDescending(p => p.StartDate) : query.OrderBy(p => p.StartDate);

            // Apply pagination
            var plans = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} rehabilitation plans with filter", plans.Count);

            return _mapper.Map<IEnumerable<RehabilitationPlanDto>>(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving rehabilitation plans with filter");
            throw;
        }
    }

    public async Task<RehabilitationPlanDto?> GetRehabilitationPlanByIdAsync(Guid id)
    {
        try
        {
            var plan = await _context.RehabilitationPlans
                .Include(p => p.ProgressLogs.OrderByDescending(log => log.LogDate).Take(5))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null)
                return null;

            return _mapper.Map<RehabilitationPlanDto>(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving rehabilitation plan with ID: {PlanId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<RehabilitationPlanDto>> GetRehabilitationPlansByPatientIdAsync(Guid patientId)
    {
        try
        {
            var plans = await _context.RehabilitationPlans
                .Include(p => p.ProgressLogs)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RehabilitationPlanDto>>(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving rehabilitation plans for patient: {PatientId}", patientId);
            throw;
        }
    }

    public async Task<RehabilitationPlanDto> CreateRehabilitationPlanAsync(CreateRehabilitationPlanDto createDto)
    {
        try
        {
            var plan = _mapper.Map<RehabilitationPlan>(createDto);
            plan.Id = Guid.NewGuid();
            plan.Status = PlanStatus.Pending;

            // Set end date if not provided
            if (!plan.EndDate.HasValue && plan.EstimatedDurationWeeks > 0)
            {
                plan.EndDate = plan.StartDate.AddDays(plan.EstimatedDurationWeeks * 7);
            }

            _context.RehabilitationPlans.Add(plan);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new rehabilitation plan with ID: {PlanId} for patient: {PatientId}", plan.Id, plan.PatientId);

            return _mapper.Map<RehabilitationPlanDto>(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating rehabilitation plan");
            throw;
        }
    }

    public async Task<RehabilitationPlanDto?> UpdateRehabilitationPlanAsync(Guid id, UpdateRehabilitationPlanDto updateDto)
    {
        try
        {
            var plan = await _context.RehabilitationPlans.FindAsync(id);

            if (plan == null)
                return null;

            _mapper.Map(updateDto, plan);

            // Update end date if duration changed
            if (!plan.EndDate.HasValue && plan.EstimatedDurationWeeks > 0)
            {
                plan.EndDate = plan.StartDate.AddDays(plan.EstimatedDurationWeeks * 7);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated rehabilitation plan with ID: {PlanId}", id);

            return _mapper.Map<RehabilitationPlanDto>(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating rehabilitation plan with ID: {PlanId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteRehabilitationPlanAsync(Guid id)
    {
        try
        {
            var plan = await _context.RehabilitationPlans.FindAsync(id);

            if (plan == null)
                return false;

            _context.RehabilitationPlans.Remove(plan);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted rehabilitation plan with ID: {PlanId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting rehabilitation plan with ID: {PlanId}", id);
            throw;
        }
    }

    public async Task<bool> RehabilitationPlanExistsAsync(Guid id)
    {
        try
        {
            return await _context.RehabilitationPlans.AnyAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if rehabilitation plan exists with ID: {PlanId}", id);
            throw;
        }
    }

    #endregion

    #region Plan Status Management

    public async Task<bool> ActivatePlanAsync(Guid id)
    {
        try
        {
            var plan = await _context.RehabilitationPlans.FindAsync(id);

            if (plan == null || plan.Status != PlanStatus.Pending)
                return false;

            plan.Status = PlanStatus.Active;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Activated rehabilitation plan with ID: {PlanId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating rehabilitation plan with ID: {PlanId}", id);
            throw;
        }
    }

    public async Task<bool> CompletePlanAsync(Guid id)
    {
        try
        {
            var plan = await _context.RehabilitationPlans.FindAsync(id);

            if (plan == null || plan.Status != PlanStatus.Active)
                return false;

            plan.Status = PlanStatus.Completed;
            plan.EndDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Completed rehabilitation plan with ID: {PlanId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing rehabilitation plan with ID: {PlanId}", id);
            throw;
        }
    }

    public async Task<bool> PutPlanOnHoldAsync(Guid id, string reason)
    {
        try
        {
            var plan = await _context.RehabilitationPlans.FindAsync(id);

            if (plan == null || plan.Status != PlanStatus.Active)
                return false;

            plan.Status = PlanStatus.OnHold;
            plan.SpecialInstructions = $"On Hold: {reason}. {plan.SpecialInstructions}";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Put rehabilitation plan {PlanId} on hold. Reason: {Reason}", id, reason);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while putting rehabilitation plan {PlanId} on hold", id);
            throw;
        }
    }

    public async Task<bool> CancelPlanAsync(Guid id, string reason)
    {
        try
        {
            var plan = await _context.RehabilitationPlans.FindAsync(id);

            if (plan == null || plan.Status == PlanStatus.Completed || plan.Status == PlanStatus.Cancelled)
                return false;

            plan.Status = PlanStatus.Cancelled;
            plan.SpecialInstructions = $"Cancelled: {reason}. {plan.SpecialInstructions}";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cancelled rehabilitation plan {PlanId}. Reason: {Reason}", id, reason);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling rehabilitation plan {PlanId}", id);
            throw;
        }
    }

    #endregion

    #region Progress Logs

    public async Task<IEnumerable<ProgressLogDto>> GetProgressLogsAsync(ProgressFilterDto filter)
    {
        try
        {
            var query = _context.RehabilitationProgressLogs.AsQueryable();

            // Apply filters
            if (filter.RehabilitationPlanId.HasValue)
                query = query.Where(log => log.RehabilitationPlanId == filter.RehabilitationPlanId.Value);

            if (filter.PatientId.HasValue)
            {
                query = query.Where(log => _context.RehabilitationPlans
                    .Any(plan => plan.Id == log.RehabilitationPlanId && plan.PatientId == filter.PatientId.Value));
            }

            if (filter.ProgressType.HasValue)
                query = query.Where(log => log.ProgressType == filter.ProgressType.Value);

            if (filter.CompletionStatus.HasValue)
                query = query.Where(log => log.CompletionStatus == filter.CompletionStatus.Value);

            if (!string.IsNullOrEmpty(filter.SubmittedBy))
                query = query.Where(log => log.SubmittedBy != null && log.SubmittedBy.Contains(filter.SubmittedBy));

            if (filter.LogDateFrom.HasValue)
                query = query.Where(log => log.LogDate >= filter.LogDateFrom.Value);

            if (filter.LogDateTo.HasValue)
                query = query.Where(log => log.LogDate <= filter.LogDateTo.Value);

            if (filter.MinPainLevel.HasValue)
                query = query.Where(log => log.PainLevel >= filter.MinPainLevel.Value);

            if (filter.MaxPainLevel.HasValue)
                query = query.Where(log => log.PainLevel <= filter.MaxPainLevel.Value);

            // Apply sorting
            if (filter.SortBy?.ToLower() == "progresstype")
                query = filter.SortDescending ? query.OrderByDescending(log => log.ProgressType) : query.OrderBy(log => log.ProgressType);
            else if (filter.SortBy?.ToLower() == "completionstatus")
                query = filter.SortDescending ? query.OrderByDescending(log => log.CompletionStatus) : query.OrderBy(log => log.CompletionStatus);
            else // Default: LogDate
                query = filter.SortDescending ? query.OrderByDescending(log => log.LogDate) : query.OrderBy(log => log.LogDate);

            // Apply pagination
            var logs = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProgressLogDto>>(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving progress logs with filter");
            throw;
        }
    }

    public async Task<ProgressLogDto?> GetProgressLogByIdAsync(Guid id)
    {
        try
        {
            var log = await _context.RehabilitationProgressLogs.FindAsync(id);

            if (log == null)
                return null;

            return _mapper.Map<ProgressLogDto>(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving progress log with ID: {LogId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ProgressLogDto>> GetProgressLogsByPlanIdAsync(Guid planId, int page = 1, int pageSize = 10)
    {
        try
        {
            var logs = await _context.RehabilitationProgressLogs
                .Where(log => log.RehabilitationPlanId == planId)
                .OrderByDescending(log => log.LogDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProgressLogDto>>(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving progress logs for plan: {PlanId}", planId);
            throw;
        }
    }

    public async Task<ProgressLogDto> CreateProgressLogAsync(CreateProgressLogDto createDto)
    {
        try
        {
            // Verify that the rehabilitation plan exists
            var planExists = await _context.RehabilitationPlans.AnyAsync(p => p.Id == createDto.RehabilitationPlanId);
            if (!planExists)
            {
                throw new InvalidOperationException($"Rehabilitation plan with ID {createDto.RehabilitationPlanId} not found.");
            }

            var log = _mapper.Map<RehabilitationProgressLog>(createDto);
            log.Id = Guid.NewGuid();

            _context.RehabilitationProgressLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new progress log with ID: {LogId} for plan: {PlanId}", log.Id, log.RehabilitationPlanId);

            return _mapper.Map<ProgressLogDto>(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating progress log");
            throw;
        }
    }

    public async Task<bool> DeleteProgressLogAsync(Guid id)
    {
        try
        {
            var log = await _context.RehabilitationProgressLogs.FindAsync(id);

            if (log == null)
                return false;

            _context.RehabilitationProgressLogs.Remove(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted progress log with ID: {LogId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting progress log with ID: {LogId}", id);
            throw;
        }
    }

    #endregion

    #region Analytics and Reports

    public async Task<object> GetPlanStatisticsAsync(Guid planId)
    {
        try
        {
            var plan = await _context.RehabilitationPlans
                .Include(p => p.ProgressLogs)
                .FirstOrDefaultAsync(p => p.Id == planId);

            if (plan == null)
                return new { error = "Plan not found" };

            var logs = plan.ProgressLogs;
            var totalLogs = logs.Count;
            var completedSessions = logs.Count(l => l.CompletionStatus == CompletionStatus.Completed);
            var averagePainLevel = logs.Where(l => l.PainLevel.HasValue).Average(l => l.PainLevel);
            var averageEnergyLevel = logs.Where(l => l.EnergyLevel.HasValue).Average(l => l.EnergyLevel);
            var averageMoodLevel = logs.Where(l => l.MoodLevel.HasValue).Average(l => l.MoodLevel);
            var totalDuration = logs.Where(l => l.DurationMinutes.HasValue).Sum(l => l.DurationMinutes);

            return new
            {
                planId,
                totalSessions = totalLogs,
                completedSessions,
                completionRate = totalLogs > 0 ? (double)completedSessions / totalLogs * 100 : 0,
                averagePainLevel = Math.Round(averagePainLevel ?? 0, 1),
                averageEnergyLevel = Math.Round(averageEnergyLevel ?? 0, 1),
                averageMoodLevel = Math.Round(averageMoodLevel ?? 0, 1),
                totalDurationHours = Math.Round((totalDuration ?? 0) / 60.0, 1),
                daysActive = (DateTime.UtcNow - plan.StartDate).Days,
                status = plan.Status.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting plan statistics for plan: {PlanId}", planId);
            throw;
        }
    }

    public async Task<object> GetPatientProgressSummaryAsync(Guid patientId)
    {
        try
        {
            var plans = await _context.RehabilitationPlans
                .Include(p => p.ProgressLogs)
                .Where(p => p.PatientId == patientId)
                .ToListAsync();

            var totalPlans = plans.Count;
            var activePlans = plans.Count(p => p.Status == PlanStatus.Active);
            var completedPlans = plans.Count(p => p.Status == PlanStatus.Completed);
            var totalSessions = plans.SelectMany(p => p.ProgressLogs).Count();
            var recentActivity = plans.SelectMany(p => p.ProgressLogs)
                .Where(l => l.LogDate >= DateTime.UtcNow.AddDays(-7))
                .Count();

            return new
            {
                patientId,
                totalPlans,
                activePlans,
                completedPlans,
                totalSessions,
                recentActivity,
                planTypes = plans.GroupBy(p => p.PlanType)
                    .Select(g => new { type = g.Key.ToString(), count = g.Count() })
                    .ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting patient progress summary for patient: {PatientId}", patientId);
            throw;
        }
    }

    public async Task<IEnumerable<RehabilitationPlanDto>> GetActivePlansAsync()
    {
        try
        {
            var plans = await _context.RehabilitationPlans
                .Include(p => p.ProgressLogs)
                .Where(p => p.Status == PlanStatus.Active)
                .OrderBy(p => p.StartDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RehabilitationPlanDto>>(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active plans");
            throw;
        }
    }

    public async Task<IEnumerable<RehabilitationPlanDto>> GetPlansNeedingAttentionAsync()
    {
        try
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            var plans = await _context.RehabilitationPlans
                .Include(p => p.ProgressLogs)
                .Where(p => p.Status == PlanStatus.Active &&
                           !p.ProgressLogs.Any(log => log.LogDate >= sevenDaysAgo))
                .OrderBy(p => p.StartDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RehabilitationPlanDto>>(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving plans needing attention");
            throw;
        }
    }

    #endregion
}