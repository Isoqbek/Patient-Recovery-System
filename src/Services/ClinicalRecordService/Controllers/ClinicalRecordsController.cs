using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicalRecordService.DTOs;
using ClinicalRecordService.Models;
using ClinicalRecordService.Services;

namespace ClinicalRecordService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClinicalRecordsController : ControllerBase
{
    private readonly IClinicalRecordService _clinicalRecordService;
    private readonly ILogger<ClinicalRecordsController> _logger;

    public ClinicalRecordsController(IClinicalRecordService clinicalRecordService, ILogger<ClinicalRecordsController> logger)
    {
        _clinicalRecordService = clinicalRecordService;
        _logger = logger;
    }

    /// <summary>
    /// Get clinical entries with filtering
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ClinicalRecordReadPolicy")]
    public async Task<ActionResult<IEnumerable<ClinicalEntryDto>>> GetClinicalEntries([FromQuery] ClinicalEntryFilterDto filter)
    {
        try
        {
            var entries = await _clinicalRecordService.GetClinicalEntriesAsync(filter);
            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving clinical entries");
            return StatusCode(500, "An error occurred while retrieving clinical entries.");
        }
    }

    /// <summary>
    /// Get clinical entry by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ClinicalRecordReadPolicy")]
    public async Task<ActionResult<ClinicalEntryDto>> GetClinicalEntryById(Guid id)
    {
        try
        {
            var entry = await _clinicalRecordService.GetClinicalEntryByIdAsync(id);

            if (entry == null)
                return NotFound($"Clinical entry with ID {id} not found.");

            return Ok(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving clinical entry with ID: {EntryId}", id);
            return StatusCode(500, "An error occurred while retrieving the clinical entry.");
        }
    }

    /// <summary>
    /// Get clinical entries by patient ID
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Policy = "ClinicalRecordReadPolicy")]
    public async Task<ActionResult<IEnumerable<ClinicalEntryDto>>> GetClinicalEntriesByPatientId(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters. Page must be >= 1 and PageSize must be between 1 and 100.");

            var entries = await _clinicalRecordService.GetClinicalEntriesByPatientIdAsync(patientId, page, pageSize);
            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving clinical entries for patient: {PatientId}", patientId);
            return StatusCode(500, "An error occurred while retrieving clinical entries.");
        }
    }

    /// <summary>
    /// Get clinical entries by patient ID and entry type
    /// </summary>
    [HttpGet("patient/{patientId:guid}/type/{entryType}")]
    [Authorize(Policy = "ClinicalRecordReadPolicy")]
    public async Task<ActionResult<IEnumerable<ClinicalEntryDto>>> GetClinicalEntriesByType(
        Guid patientId,
        EntryType entryType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters. Page must be >= 1 and PageSize must be between 1 and 100.");

            var entries = await _clinicalRecordService.GetClinicalEntriesByTypeAsync(patientId, entryType, page, pageSize);
            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving clinical entries for patient {PatientId} with type {EntryType}", patientId, entryType);
            return StatusCode(500, "An error occurred while retrieving clinical entries.");
        }
    }

    /// <summary>
    /// Get recent vital signs for a patient
    /// </summary>
    [HttpGet("patient/{patientId:guid}/vitalsigns/recent")]
    [Authorize(Policy = "ClinicalRecordReadPolicy")]
    public async Task<ActionResult<IEnumerable<ClinicalEntryDto>>> GetRecentVitalSigns(
        Guid patientId,
        [FromQuery] int count = 5)
    {
        try
        {
            if (count < 1 || count > 50)
                return BadRequest("Count must be between 1 and 50.");

            var entries = await _clinicalRecordService.GetRecentVitalSignsAsync(patientId, count);
            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving recent vital signs for patient: {PatientId}", patientId);
            return StatusCode(500, "An error occurred while retrieving recent vital signs.");
        }
    }

    /// <summary>
    /// Get clinical entries by date range
    /// </summary>
    [HttpGet("patient/{patientId:guid}/daterange")]
    [Authorize(Policy = "ClinicalRecordReadPolicy")]
    public async Task<ActionResult<IEnumerable<ClinicalEntryDto>>> GetClinicalEntriesByDateRange(
        Guid patientId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            if (fromDate > toDate)
                return BadRequest("FromDate cannot be greater than ToDate.");

            if (toDate.Subtract(fromDate).TotalDays > 365)
                return BadRequest("Date range cannot exceed 365 days.");

            var entries = await _clinicalRecordService.GetClinicalEntriesByDateRangeAsync(patientId, fromDate, toDate);
            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving clinical entries by date range for patient: {PatientId}", patientId);
            return StatusCode(500, "An error occurred while retrieving clinical entries.");
        }
    }

    /// <summary>
    /// Create a new clinical entry
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ClinicalRecordWritePolicy")]
    public async Task<ActionResult<ClinicalEntryDto>> CreateClinicalEntry([FromBody] CreateClinicalEntryDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entry = await _clinicalRecordService.CreateClinicalEntryAsync(createDto);

            return CreatedAtAction(
                nameof(GetClinicalEntryById),
                new { id = entry.Id },
                entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating clinical entry");
            return StatusCode(500, "An error occurred while creating the clinical entry.");
        }
    }

    /// <summary>
    /// Update an existing clinical entry
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ClinicalRecordWritePolicy")]
    public async Task<ActionResult<ClinicalEntryDto>> UpdateClinicalEntry(Guid id, [FromBody] UpdateClinicalEntryDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entry = await _clinicalRecordService.UpdateClinicalEntryAsync(id, updateDto);

            if (entry == null)
                return NotFound($"Clinical entry with ID {id} not found.");

            return Ok(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating clinical entry with ID: {EntryId}", id);
            return StatusCode(500, "An error occurred while updating the clinical entry.");
        }
    }

    /// <summary>
    /// Delete a clinical entry
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ClinicalRecordWritePolicy")]
    public async Task<ActionResult> DeleteClinicalEntry(Guid id)
    {
        try
        {
            var deleted = await _clinicalRecordService.DeleteClinicalEntryAsync(id);

            if (!deleted)
                return NotFound($"Clinical entry with ID {id} not found.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting clinical entry with ID: {EntryId}", id);
            return StatusCode(500, "An error occurred while deleting the clinical entry.");
        }
    }

    /// <summary>
    /// Check if clinical entry exists
    /// </summary>
    [HttpHead("{id:guid}")]
    [Authorize(Policy = "ClinicalRecordReadPolicy")]
    public async Task<ActionResult> ClinicalEntryExists(Guid id)
    {
        try
        {
            var exists = await _clinicalRecordService.ClinicalEntryExistsAsync(id);

            if (!exists)
                return NotFound();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if clinical entry exists with ID: {EntryId}", id);
            return StatusCode(500, "An error occurred while checking clinical entry existence.");
        }
    }

    /// <summary>
    /// Get all available entry types
    /// </summary>
    [HttpGet("entry-types")]
    [Authorize(Policy = "ClinicalRecordReadPolicy")]
    public ActionResult<object> GetEntryTypes()
    {
        try
        {
            var entryTypes = Enum.GetValues<EntryType>()
                .Select(et => new {
                    Value = (int)et,
                    Name = et.ToString(),
                    DisplayName = GetEntryTypeDisplayName(et)
                });

            return Ok(entryTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving entry types");
            return StatusCode(500, "An error occurred while retrieving entry types.");
        }
    }

    private static string GetEntryTypeDisplayName(EntryType entryType)
    {
        return entryType switch
        {
            EntryType.VitalSign => "Vital Signs",
            EntryType.Symptom => "Symptoms",
            EntryType.Observation => "Observations",
            EntryType.Medication => "Medications",
            EntryType.Procedure => "Procedures",
            EntryType.TestResult => "Test Results",
            EntryType.Note => "Notes",
            EntryType.Diagnosis => "Diagnosis",
            EntryType.Treatment => "Treatment",
            EntryType.Allergy => "Allergies",
            _ => entryType.ToString()
        };
    }
}