using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringService.DTOs;
using MonitoringService.Models;
using MonitoringService.Services;

namespace MonitoringService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(IAlertService alertService, ILogger<AlertsController> logger)
    {
        _alertService = alertService;
        _logger = logger;
    }

    /// <summary>
    /// Get alerts with filtering
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "MonitoringReadPolicy")]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetAlerts([FromQuery] AlertFilterDto filter)
    {
        try
        {
            var alerts = await _alertService.GetAlertsAsync(filter);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving alerts");
            return StatusCode(500, "An error occurred while retrieving alerts.");
        }
    }

    /// <summary>
    /// Get alert by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "MonitoringReadPolicy")]
    public async Task<ActionResult<AlertDto>> GetAlertById(Guid id)
    {
        try
        {
            var alert = await _alertService.GetAlertByIdAsync(id);

            if (alert == null)
                return NotFound($"Alert with ID {id} not found.");

            return Ok(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving alert with ID: {AlertId}", id);
            return StatusCode(500, "An error occurred while retrieving the alert.");
        }
    }

    /// <summary>
    /// Get alerts by patient ID
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Policy = "MonitoringReadPolicy")]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetAlertsByPatientId(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters. Page must be >= 1 and PageSize must be between 1 and 100.");

            var alerts = await _alertService.GetAlertsByPatientIdAsync(patientId, page, pageSize);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving alerts for patient: {PatientId}", patientId);
            return StatusCode(500, "An error occurred while retrieving alerts.");
        }
    }

    /// <summary>
    /// Get active alerts across all patients
    /// </summary>
    [HttpGet("active")]
    [Authorize(Policy = "MonitoringReadPolicy")]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetActiveAlerts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters. Page must be >= 1 and PageSize must be between 1 and 100.");

            var alerts = await _alertService.GetActiveAlertsAsync(page, pageSize);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active alerts");
            return StatusCode(500, "An error occurred while retrieving active alerts.");
        }
    }

    /// <summary>
    /// Get critical alerts across all patients
    /// </summary>
    [HttpGet("critical")]
    [Authorize(Policy = "MonitoringReadPolicy")]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetCriticalAlerts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters. Page must be >= 1 and PageSize must be between 1 and 100.");

            var alerts = await _alertService.GetCriticalAlertsAsync(page, pageSize);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving critical alerts");
            return StatusCode(500, "An error occurred while retrieving critical alerts.");
        }
    }

    /// <summary>
    /// Get alerts by date range for a patient
    /// </summary>
    [HttpGet("patient/{patientId:guid}/daterange")]
    [Authorize(Policy = "MonitoringReadPolicy")]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetAlertsByDateRange(
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

            var alerts = await _alertService.GetAlertsByDateRangeAsync(patientId, fromDate, toDate);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving alerts by date range for patient: {PatientId}", patientId);
            return StatusCode(500, "An error occurred while retrieving alerts.");
        }
    }

    /// <summary>
    /// Get active alert count
    /// </summary>
    [HttpGet("count/active")]
    [Authorize(Policy = "MonitoringReadPolicy")]
    public async Task<ActionResult<object>> GetActiveAlertCount([FromQuery] Guid? patientId = null)
    {
        try
        {
            var count = await _alertService.GetActiveAlertCountAsync(patientId);
            return Ok(new { activeAlertCount = count, patientId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active alert count");
            return StatusCode(500, "An error occurred while retrieving active alert count.");
        }
    }

    /// <summary>
    /// Create a new alert manually
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "MonitoringWritePolicy")]
    public async Task<ActionResult<AlertDto>> CreateAlert([FromBody] CreateAlertDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var alert = await _alertService.CreateAlertAsync(createDto);

            return CreatedAtAction(
                nameof(GetAlertById),
                new { id = alert.Id },
                alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating alert");
            return StatusCode(500, "An error occurred while creating the alert.");
        }
    }

    /// <summary>
    /// Update an existing alert
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "MonitoringWritePolicy")]
    public async Task<ActionResult<AlertDto>> UpdateAlert(Guid id, [FromBody] UpdateAlertDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var alert = await _alertService.UpdateAlertAsync(id, updateDto);

            if (alert == null)
                return NotFound($"Alert with ID {id} not found.");

            return Ok(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating alert with ID: {AlertId}", id);
            return StatusCode(500, "An error occurred while updating the alert.");
        }
    }

    /// <summary>
    /// Delete an alert
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "MonitoringWritePolicy")]
    public async Task<ActionResult> DeleteAlert(Guid id)
    {
        try
        {
            var deleted = await _alertService.DeleteAlertAsync(id);

            if (!deleted)
                return NotFound($"Alert with ID {id} not found.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting alert with ID: {AlertId}", id);
            return StatusCode(500, "An error occurred while deleting the alert.");
        }
    }

    /// <summary>
    /// Acknowledge an alert
    /// </summary>
    [HttpPatch("{id:guid}/acknowledge")]
    [Authorize(Policy = "MonitoringWritePolicy")]
    public async Task<ActionResult> AcknowledgeAlert(Guid id, [FromBody] AcknowledgeAlertRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.AcknowledgedBy))
                return BadRequest("AcknowledgedBy is required.");

            var success = await _alertService.AcknowledgeAlertAsync(id, request.AcknowledgedBy);

            if (!success)
                return NotFound($"Alert with ID {id} not found or cannot be acknowledged.");

            return Ok(new { message = "Alert acknowledged successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while acknowledging alert with ID: {AlertId}", id);
            return StatusCode(500, "An error occurred while acknowledging the alert.");
        }
    }

    /// <summary>
    /// Resolve an alert
    /// </summary>
    [HttpPatch("{id:guid}/resolve")]
    [Authorize(Policy = "MonitoringWritePolicy")]
    public async Task<ActionResult> ResolveAlert(Guid id, [FromBody] ResolveAlertRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ResolvedBy))
                return BadRequest("ResolvedBy is required.");

            var success = await _alertService.ResolveAlertAsync(id, request.ResolvedBy, request.ResolutionNotes);

            if (!success)
                return NotFound($"Alert with ID {id} not found or cannot be resolved.");

            return Ok(new { message = "Alert resolved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resolving alert with ID: {AlertId}", id);
            return StatusCode(500, "An error occurred while resolving the alert.");
        }
    }

    /// <summary>
    /// Get all available alert severities
    /// </summary>
    [HttpGet("severities")]
    [Authorize(Policy = "MonitoringReadPolicy")]
    public ActionResult<object> GetAlertSeverities()
    {
        try
        {
            var severities = Enum.GetValues<AlertSeverity>()
                .Select(s => new {
                    Value = (int)s,
                    Name = s.ToString(),
                    DisplayName = GetSeverityDisplayName(s)
                });

            return Ok(severities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving alert severities");
            return StatusCode(500, "An error occurred while retrieving alert severities.");
        }
    }

    /// <summary>
    /// Get all available alert statuses
    /// </summary>
    [HttpGet("statuses")]
    [Authorize(Policy = "MonitoringReadPolicy")]
    public ActionResult<object> GetAlertStatuses()
    {
        try
        {
            var statuses = Enum.GetValues<AlertStatus>()
                .Select(s => new {
                    Value = (int)s,
                    Name = s.ToString(),
                    DisplayName = GetStatusDisplayName(s)
                });

            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving alert statuses");
            return StatusCode(500, "An error occurred while retrieving alert statuses.");
        }
    }

    private static string GetSeverityDisplayName(AlertSeverity severity)
    {
        return severity switch
        {
            AlertSeverity.Information => "Information",
            AlertSeverity.Warning => "Warning",
            AlertSeverity.Critical => "Critical",
            _ => severity.ToString()
        };
    }

    private static string GetStatusDisplayName(AlertStatus status)
    {
        return status switch
        {
            AlertStatus.New => "New",
            AlertStatus.Acknowledged => "Acknowledged",
            AlertStatus.InProgress => "In Progress",
            AlertStatus.Resolved => "Resolved",
            AlertStatus.Closed => "Closed",
            _ => status.ToString()
        };
    }
}

public class AcknowledgeAlertRequest
{
    public string AcknowledgedBy { get; set; } = string.Empty;
}

public class ResolveAlertRequest
{
    public string ResolvedBy { get; set; } = string.Empty;
    public string? ResolutionNotes { get; set; }
}