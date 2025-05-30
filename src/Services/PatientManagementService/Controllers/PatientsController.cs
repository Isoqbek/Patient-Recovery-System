using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientManagementService.DTOs;
using PatientManagementService.Services;

namespace PatientManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    /// <summary>
    /// Get all patients
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "PatientReadPolicy")]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAllPatients()
    {
        try
        {
            var patients = await _patientService.GetAllPatientsAsync();
            return Ok(patients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all patients");
            return StatusCode(500, "An error occurred while retrieving patients.");
        }
    }

    /// <summary>
    /// Get patient by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "PatientReadPolicy")]
    public async Task<ActionResult<PatientDto>> GetPatientById(Guid id)
    {
        try
        {
            var patient = await _patientService.GetPatientByIdAsync(id);

            if (patient == null)
                return NotFound($"Patient with ID {id} not found.");

            return Ok(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving patient with ID: {PatientId}", id);
            return StatusCode(500, "An error occurred while retrieving the patient.");
        }
    }

    /// <summary>
    /// Create a new patient
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "PatientWritePolicy")]
    public async Task<ActionResult<PatientDto>> CreatePatient([FromBody] CreatePatientDto createPatientDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = await _patientService.CreatePatientAsync(createPatientDto);

            return CreatedAtAction(
                nameof(GetPatientById),
                new { id = patient.Id },
                patient);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating patient");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating patient");
            return StatusCode(500, "An error occurred while creating the patient.");
        }
    }

    /// <summary>
    /// Update an existing patient
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "PatientWritePolicy")]
    public async Task<ActionResult<PatientDto>> UpdatePatient(Guid id, [FromBody] UpdatePatientDto updatePatientDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = await _patientService.UpdatePatientAsync(id, updatePatientDto);

            if (patient == null)
                return NotFound($"Patient with ID {id} not found.");

            return Ok(patient);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating patient with ID: {PatientId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating patient with ID: {PatientId}", id);
            return StatusCode(500, "An error occurred while updating the patient.");
        }
    }

    /// <summary>
    /// Delete a patient
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "PatientWritePolicy")]
    public async Task<ActionResult> DeletePatient(Guid id)
    {
        try
        {
            var deleted = await _patientService.DeletePatientAsync(id);

            if (!deleted)
                return NotFound($"Patient with ID {id} not found.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting patient with ID: {PatientId}", id);
            return StatusCode(500, "An error occurred while deleting the patient.");
        }
    }

    /// <summary>
    /// Search patients by name, email, or phone
    /// </summary>
    [HttpGet("search")]
    [Authorize(Policy = "PatientReadPolicy")]
    public async Task<ActionResult<IEnumerable<PatientDto>>> SearchPatients([FromQuery] string searchTerm)
    {
        try
        {
            var patients = await _patientService.SearchPatientsAsync(searchTerm);
            return Ok(patients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching patients with term: {SearchTerm}", searchTerm);
            return StatusCode(500, "An error occurred while searching patients.");
        }
    }

    /// <summary>
    /// Check if patient exists
    /// </summary>
    [HttpHead("{id:guid}")]
    [Authorize(Policy = "PatientReadPolicy")]
    public async Task<ActionResult> PatientExists(Guid id)
    {
        try
        {
            var exists = await _patientService.PatientExistsAsync(id);

            if (!exists)
                return NotFound();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if patient exists with ID: {PatientId}", id);
            return StatusCode(500, "An error occurred while checking patient existence.");
        }
    }
}