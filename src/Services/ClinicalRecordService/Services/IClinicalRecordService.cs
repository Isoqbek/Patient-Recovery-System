using ClinicalRecordService.DTOs;
using ClinicalRecordService.Models;

namespace ClinicalRecordService.Services;

public interface IClinicalRecordService
{
    Task<IEnumerable<ClinicalEntryDto>> GetClinicalEntriesAsync(ClinicalEntryFilterDto filter);
    Task<ClinicalEntryDto?> GetClinicalEntryByIdAsync(Guid id);
    Task<IEnumerable<ClinicalEntryDto>> GetClinicalEntriesByPatientIdAsync(Guid patientId, int page = 1, int pageSize = 10);
    Task<IEnumerable<ClinicalEntryDto>> GetClinicalEntriesByTypeAsync(Guid patientId, EntryType entryType, int page = 1, int pageSize = 10);
    Task<ClinicalEntryDto> CreateClinicalEntryAsync(CreateClinicalEntryDto createDto);
    Task<ClinicalEntryDto?> UpdateClinicalEntryAsync(Guid id, UpdateClinicalEntryDto updateDto);
    Task<bool> DeleteClinicalEntryAsync(Guid id);
    Task<bool> ClinicalEntryExistsAsync(Guid id);
    Task<IEnumerable<ClinicalEntryDto>> GetRecentVitalSignsAsync(Guid patientId, int count = 5);
    Task<IEnumerable<ClinicalEntryDto>> GetClinicalEntriesByDateRangeAsync(Guid patientId, DateTime fromDate, DateTime toDate);
}