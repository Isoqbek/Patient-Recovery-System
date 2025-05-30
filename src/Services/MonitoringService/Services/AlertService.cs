using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MonitoringService.Data;
using MonitoringService.DTOs;
using MonitoringService.Models;
using MonitoringService.Events;
using Microsoft.Extensions.Logging;

namespace MonitoringService.Services;

public class AlertService : IAlertService
{
    private readonly MonitoringDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AlertService> _logger;
    private readonly IEventBusService _eventBusService;

    public AlertService(
        MonitoringDbContext context,
        IMapper mapper,
        ILogger<AlertService> logger,
        IEventBusService eventBusService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _eventBusService = eventBusService;
    }

    public async Task<IEnumerable<AlertDto>> GetAlertsAsync(AlertFilterDto filter)
    {
        try
        {
            var query = _context.Alerts.AsQueryable();

            // Apply filters
            if (filter.PatientId.HasValue)
                query = query.Where(a => a.PatientId == filter.PatientId.Value);

            if (filter.Severity.HasValue)
                query = query.Where(a => a.Severity == filter.Severity.Value);

            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == filter.Status.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(a => a.AlertDateTime >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(a => a.AlertDateTime <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.AcknowledgedBy))
                query = query.Where(a => a.AcknowledgedBy != null && a.AcknowledgedBy.Contains(filter.AcknowledgedBy));

            if (filter.IsActive.HasValue && filter.IsActive.Value)
                query = query.Where(a => a.Status == AlertStatus.New || a.Status == AlertStatus.Acknowledged || a.Status == AlertStatus.InProgress);

            // Apply sorting
            if (filter.SortBy?.ToLower() == "severity")
                query = filter.SortDescending ? query.OrderByDescending(a => a.Severity) : query.OrderBy(a => a.Severity);
            else if (filter.SortBy?.ToLower() == "status")
                query = filter.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status);
            else if (filter.SortBy?.ToLower() == "title")
                query = filter.SortDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title);
            else // Default: AlertDateTime
                query = filter.SortDescending ? query.OrderByDescending(a => a.AlertDateTime) : query.OrderBy(a => a.AlertDateTime);

            // Apply pagination
            var alerts = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} alerts with filter", alerts.Count);

            return _mapper.Map<IEnumerable<AlertDto>>(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving alerts with filter");
            throw;
        }
    }

    public async Task<AlertDto?> GetAlertByIdAsync(Guid id)
    {
        try
        {
            var alert = await _context.Alerts.FindAsync(id);

            if (alert == null)
                return null;

            return _mapper.Map<AlertDto>(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving alert with ID: {AlertId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<AlertDto>> GetAlertsByPatientIdAsync(Guid patientId, int page = 1, int pageSize = 10)
    {
        try
        {
            var alerts = await _context.Alerts
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AlertDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AlertDto>>(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving alerts for patient: {PatientId}", patientId);
            throw;
        }
    }

    public async Task<IEnumerable<AlertDto>> GetActiveAlertsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var alerts = await _context.Alerts
                .Where(a => a.Status == AlertStatus.New || a.Status == AlertStatus.Acknowledged || a.Status == AlertStatus.InProgress)
                .OrderByDescending(a => a.Severity)
                .ThenByDescending(a => a.AlertDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AlertDto>>(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active alerts");
            throw;
        }
    }

    public async Task<IEnumerable<AlertDto>> GetCriticalAlertsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var alerts = await _context.Alerts
                .Where(a => a.Severity == AlertSeverity.Critical &&
                           (a.Status == AlertStatus.New || a.Status == AlertStatus.Acknowledged || a.Status == AlertStatus.InProgress))
                .OrderByDescending(a => a.AlertDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AlertDto>>(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving critical alerts");
            throw;
        }
    }

    public async Task<AlertDto> CreateAlertAsync(CreateAlertDto createDto)
    {
        try
        {
            var alert = _mapper.Map<Alert>(createDto);
            alert.Id = Guid.NewGuid();
            alert.Status = AlertStatus.New;

            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new alert with ID: {AlertId} for patient: {PatientId}", alert.Id, alert.PatientId);

            // Publish AlertCreatedEvent
            var alertCreatedEvent = new AlertCreatedEvent
            {
                AlertId = alert.Id,
                PatientId = alert.PatientId,
                Title = alert.Title,
                Description = alert.Description,
                Severity = alert.Severity.ToString(),
                AlertDateTime = alert.AlertDateTime,
                CreatedAt = alert.CreatedAt,
                RecipientRoles = GetRecipientRoles(alert.Severity)
            };

            await _eventBusService.PublishAsync(alertCreatedEvent, "AlertCreated");

            return _mapper.Map<AlertDto>(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating alert");
            throw;
        }
    }

    public async Task<AlertDto?> UpdateAlertAsync(Guid id, UpdateAlertDto updateDto)
    {
        try
        {
            var alert = await _context.Alerts.FindAsync(id);

            if (alert == null)
                return null;

            var oldStatus = alert.Status;
            _mapper.Map(updateDto, alert);

            // Handle status-specific updates
            if (updateDto.Status == AlertStatus.Acknowledged && oldStatus != AlertStatus.Acknowledged)
            {
                alert.AcknowledgedAt = DateTime.UtcNow;
            }
            else if (updateDto.Status == AlertStatus.Resolved && oldStatus != AlertStatus.Resolved)
            {
                alert.ResolvedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated alert with ID: {AlertId}", id);

            return _mapper.Map<AlertDto>(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating alert with ID: {AlertId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAlertAsync(Guid id)
    {
        try
        {
            var alert = await _context.Alerts.FindAsync(id);

            if (alert == null)
                return false;

            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted alert with ID: {AlertId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting alert with ID: {AlertId}", id);
            throw;
        }
    }

    public async Task<bool> AlertExistsAsync(Guid id)
    {
        try
        {
            return await _context.Alerts.AnyAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if alert exists with ID: {AlertId}", id);
            throw;
        }
    }

    public async Task<bool> AcknowledgeAlertAsync(Guid id, string acknowledgedBy)
    {
        try
        {
            var alert = await _context.Alerts.FindAsync(id);

            if (alert == null || alert.Status != AlertStatus.New)
                return false;

            alert.Status = AlertStatus.Acknowledged;
            alert.AcknowledgedBy = acknowledgedBy;
            alert.AcknowledgedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Alert {AlertId} acknowledged by {AcknowledgedBy}", id, acknowledgedBy);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while acknowledging alert with ID: {AlertId}", id);
            throw;
        }
    }

    public async Task<bool> ResolveAlertAsync(Guid id, string resolvedBy, string? resolutionNotes = null)
    {
        try
        {
            var alert = await _context.Alerts.FindAsync(id);

            if (alert == null || alert.Status == AlertStatus.Resolved || alert.Status == AlertStatus.Closed)
                return false;

            alert.Status = AlertStatus.Resolved;
            alert.ResolvedBy = resolvedBy;
            alert.ResolvedAt = DateTime.UtcNow;
            alert.ResolutionNotes = resolutionNotes;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Alert {AlertId} resolved by {ResolvedBy}", id, resolvedBy);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resolving alert with ID: {AlertId}", id);
            throw;
        }
    }

    public async Task<int> GetActiveAlertCountAsync(Guid? patientId = null)
    {
        try
        {
            var query = _context.Alerts
                .Where(a => a.Status == AlertStatus.New || a.Status == AlertStatus.Acknowledged || a.Status == AlertStatus.InProgress);

            if (patientId.HasValue)
                query = query.Where(a => a.PatientId == patientId.Value);

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting active alert count");
            throw;
        }
    }

    public async Task<IEnumerable<AlertDto>> GetAlertsByDateRangeAsync(Guid patientId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var alerts = await _context.Alerts
                .Where(a => a.PatientId == patientId && a.AlertDateTime >= fromDate && a.AlertDateTime <= toDate)
                .OrderByDescending(a => a.AlertDateTime)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AlertDto>>(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving alerts by date range for patient: {PatientId}", patientId);
            throw;
        }
    }

    private static string GetRecipientRoles(AlertSeverity severity)
    {
        return severity switch
        {
            AlertSeverity.Critical => "[\"Physician\", \"Nurse\", \"Administrator\"]",
            AlertSeverity.Warning => "[\"Physician\", \"Nurse\"]",
            AlertSeverity.Information => "[\"Nurse\"]",
            _ => "[\"Nurse\"]"
        };
    }
}