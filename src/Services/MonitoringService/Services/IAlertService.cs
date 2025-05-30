using MonitoringService.DTOs;
using MonitoringService.Models;

namespace MonitoringService.Services;

public interface IAlertService
{
    Task<IEnumerable<AlertDto>> GetAlertsAsync(AlertFilterDto filter);
    Task<AlertDto?> GetAlertByIdAsync(Guid id);
    Task<IEnumerable<AlertDto>> GetAlertsByPatientIdAsync(Guid patientId, int page = 1, int pageSize = 10);
    Task<IEnumerable<AlertDto>> GetActiveAlertsAsync(int page = 1, int pageSize = 10);
    Task<IEnumerable<AlertDto>> GetCriticalAlertsAsync(int page = 1, int pageSize = 10);
    Task<AlertDto> CreateAlertAsync(CreateAlertDto createDto);
    Task<AlertDto?> UpdateAlertAsync(Guid id, UpdateAlertDto updateDto);
    Task<bool> DeleteAlertAsync(Guid id);
    Task<bool> AlertExistsAsync(Guid id);
    Task<bool> AcknowledgeAlertAsync(Guid id, string acknowledgedBy);
    Task<bool> ResolveAlertAsync(Guid id, string resolvedBy, string? resolutionNotes = null);
    Task<int> GetActiveAlertCountAsync(Guid? patientId = null);
    Task<IEnumerable<AlertDto>> GetAlertsByDateRangeAsync(Guid patientId, DateTime fromDate, DateTime toDate);
}