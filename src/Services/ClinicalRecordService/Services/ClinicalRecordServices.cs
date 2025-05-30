using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ClinicalRecordService.Data;
using ClinicalRecordService.DTOs;
using ClinicalRecordService.Models;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace ClinicalRecordService.Services;

public class ClinicalRecordServices : IClinicalRecordService
{
    private readonly ClinicalRecordDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ClinicalRecordServices> _logger;

    public ClinicalRecordServices(ClinicalRecordDbContext context, IMapper mapper, ILogger<ClinicalRecordServices> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ClinicalEntryDto>> GetClinicalEntriesAsync(ClinicalEntryFilterDto filter)
    {
        try
        {
            var query = _context.ClinicalEntries.AsQueryable();

            // Apply filters
            if (filter.PatientId.HasValue)
                query = query.Where(e => e.PatientId == filter.PatientId.Value);

            if (filter.EntryType.HasValue)
                query = query.Where(e => e.EntryType == filter.EntryType.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(e => e.EntryDateTime >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(e => e.EntryDateTime <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.RecordedBy))
                query = query.Where(e => e.RecordedBy != null && e.RecordedBy.Contains(filter.RecordedBy));

            // Apply sorting
            if (filter.SortBy?.ToLower() == "entrytype")
                query = filter.SortDescending ? query.OrderByDescending(e => e.EntryType) : query.OrderBy(e => e.EntryType);
            else if (filter.SortBy?.ToLower() == "recordedby")
                query = filter.SortDescending ? query.OrderByDescending(e => e.RecordedBy) : query.OrderBy(e => e.RecordedBy);
            else // Default: EntryDateTime
                query = filter.SortDescending ? query.OrderByDescending(e => e.EntryDateTime) : query.OrderBy(e => e.EntryDateTime);

            // Apply pagination
            var totalCount = await query.CountAsync();
            var entries = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} clinical entries with filter", entries.Count);

            return _mapper.Map<IEnumerable<ClinicalEntryDto>>(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving clinical entries with filter");
            throw;
        }
    }

    public async Task<ClinicalEntryDto?> GetClinicalEntryByIdAsync(Guid id)
    {
        try
        {
            var entry = await _context.ClinicalEntries.FindAsync(id);

            if (entry == null)
                return null;

            return _mapper.Map<ClinicalEntryDto>(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving clinical entry with ID: {EntryId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ClinicalEntryDto>> GetClinicalEntriesByPatientIdAsync(Guid patientId, int page = 1, int pageSize = 10)
    {
        try
        {
            var entries = await _context.ClinicalEntries
                .Where(e => e.PatientId == patientId)
                .OrderByDescending(e => e.EntryDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClinicalEntryDto>>(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving clinical entries for patient: {PatientId}", patientId);
            throw;
        }
    }

    public async Task<IEnumerable<ClinicalEntryDto>> GetClinicalEntriesByTypeAsync(Guid patientId, EntryType entryType, int page = 1, int pageSize = 10)
    {
        try
        {
            var entries = await _context.ClinicalEntries
                .Where(e => e.PatientId == patientId && e.EntryType == entryType)
                .OrderByDescending(e => e.EntryDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClinicalEntryDto>>(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving clinical entries for patient {PatientId} with type {EntryType}", patientId, entryType);
            throw;
        }
    }

    public async Task<ClinicalEntryDto> CreateClinicalEntryAsync(CreateClinicalEntryDto createDto)
    {
        try
        {
            var entry = _mapper.Map<ClinicalEntry>(createDto);
            entry.Id = Guid.NewGuid();

            // Serialize Data object to JSON string
            if (createDto.Data != null)
            {
                entry.Data = JsonConvert.SerializeObject(createDto.Data);
            }

            _context.ClinicalEntries.Add(entry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new clinical entry with ID: {EntryId} for patient: {PatientId}", entry.Id, entry.PatientId);

            return _mapper.Map<ClinicalEntryDto>(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating clinical entry");
            throw;
        }
    }

    public async Task<ClinicalEntryDto?> UpdateClinicalEntryAsync(Guid id, UpdateClinicalEntryDto updateDto)
    {
        try
        {
            var entry = await _context.ClinicalEntries.FindAsync(id);

            if (entry == null)
                return null;

            _mapper.Map(updateDto, entry);

            // Serialize Data object to JSON string
            if (updateDto.Data != null)
            {
                entry.Data = JsonConvert.SerializeObject(updateDto.Data);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated clinical entry with ID: {EntryId}", id);

            return _mapper.Map<ClinicalEntryDto>(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating clinical entry with ID: {EntryId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteClinicalEntryAsync(Guid id)
    {
        try
        {
            var entry = await _context.ClinicalEntries.FindAsync(id);

            if (entry == null)
                return false;

            _context.ClinicalEntries.Remove(entry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted clinical entry with ID: {EntryId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting clinical entry with ID: {EntryId}", id);
            throw;
        }
    }

    public async Task<bool> ClinicalEntryExistsAsync(Guid id)
    {
        try
        {
            return await _context.ClinicalEntries.AnyAsync(e => e.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if clinical entry exists with ID: {EntryId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ClinicalEntryDto>> GetRecentVitalSignsAsync(Guid patientId, int count = 5)
    {
        try
        {
            var entries = await _context.ClinicalEntries
                .Where(e => e.PatientId == patientId && e.EntryType == EntryType.VitalSign)
                .OrderByDescending(e => e.EntryDateTime)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClinicalEntryDto>>(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving recent vital signs for patient: {PatientId}", patientId);
            throw;
        }
    }

    public async Task<IEnumerable<ClinicalEntryDto>> GetClinicalEntriesByDateRangeAsync(Guid patientId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var entries = await _context.ClinicalEntries
                .Where(e => e.PatientId == patientId && e.EntryDateTime >= fromDate && e.EntryDateTime <= toDate)
                .OrderByDescending(e => e.EntryDateTime)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClinicalEntryDto>>(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving clinical entries by date range for patient: {PatientId}", patientId);
            throw;
        }
    }
}