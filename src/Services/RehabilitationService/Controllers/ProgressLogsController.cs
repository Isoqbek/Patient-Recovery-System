using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RehabilitationService.DTOs;
using RehabilitationService.Models;
using RehabilitationService.Services;

namespace RehabilitationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressLogsController : ControllerBase
{
    private readonly IRehabilitationService _rehabilitationService;
    private readonly ILogger<ProgressLogsController> _logger;

    public ProgressLogsController(IRehabilitationService rehabilitationService, ILogger<ProgressLogsController> logger)
    {
        _rehabilitationService = rehabilitationService;
        _logger = logger;
    }

    /// <summary>
    /// Get progress logs with filtering
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public async Task<ActionResult<IEnumerable<ProgressLogDto>>> GetProgressLogs([FromQuery] ProgressFilterDto filter)
    {
        try
        {
            var logs = await _rehabilitationService.GetProgressLogsAsync(filter);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving progress logs");
            return StatusCode(500, "An error occurred while retrieving progress logs.");
        }
    }

    /// <summary>
    /// Get progress log by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public async Task<ActionResult<ProgressLogDto>> GetProgressLogById(Guid id)
    {
        try
        {
            var log = await _rehabilitationService.GetProgressLogByIdAsync(id);

            if (log == null)
                return NotFound($"Progress log with ID {id} not found.");

            return Ok(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving progress log with ID: {LogId}", id);
            return StatusCode(500, "An error occurred while retrieving the progress log.");
        }
    }

    /// <summary>
    /// Get progress logs by rehabilitation plan ID
    /// </summary>
    [HttpGet("plan/{planId:guid}")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public async Task<ActionResult<IEnumerable<ProgressLogDto>>> GetProgressLogsByPlanId(
        Guid planId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters. Page must be >= 1 and PageSize must be between 1 and 100.");

            var logs = await _rehabilitationService.GetProgressLogsByPlanIdAsync(planId, page, pageSize);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving progress logs for plan: {PlanId}", planId);
            return StatusCode(500, "An error occurred while retrieving progress logs.");
        }
    }

    /// <summary>
    /// Create a new progress log
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RehabilitationWritePolicy")]
    public async Task<ActionResult<ProgressLogDto>> CreateProgressLog([FromBody] CreateProgressLogDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var log = await _rehabilitationService.CreateProgressLogAsync(createDto);

            return CreatedAtAction(
                nameof(GetProgressLogById),
                new { id = log.Id },
                log);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating progress log");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating progress log");
            return StatusCode(500, "An error occurred while creating the progress log.");
        }
    }

    /// <summary>
    /// Delete a progress log
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RehabilitationWritePolicy")]
    public async Task<ActionResult> DeleteProgressLog(Guid id)
    {
        try
        {
            var deleted = await _rehabilitationService.DeleteProgressLogAsync(id);

            if (!deleted)
                return NotFound($"Progress log with ID {id} not found.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting progress log with ID: {LogId}", id);
            return StatusCode(500, "An error occurred while deleting the progress log.");
        }
    }

    /// <summary>
    /// Get all available progress types
    /// </summary>
    [HttpGet("types")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public ActionResult<object> GetProgressTypes()
    {
        try
        {
            var types = Enum.GetValues<ProgressType>()
                .Select(t => new {
                    Value = (int)t,
                    Name = t.ToString(),
                    DisplayName = GetProgressTypeDisplayName(t)
                });

            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving progress types");
            return StatusCode(500, "An error occurred while retrieving progress types.");
        }
    }

    /// <summary>
    /// Get all available completion statuses
    /// </summary>
    [HttpGet("completion-statuses")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public ActionResult<object> GetCompletionStatuses()
    {
        try
        {
            var statuses = Enum.GetValues<CompletionStatus>()
                .Select(s => new {
                    Value = (int)s,
                    Name = s.ToString(),
                    DisplayName = GetCompletionStatusDisplayName(s)
                });

            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving completion statuses");
            return StatusCode(500, "An error occurred while retrieving completion statuses.");
        }
    }

    private static string GetProgressTypeDisplayName(ProgressType type)
    {
        return type switch
        {
            ProgressType.Exercise => "Exercise",
            ProgressType.Therapy => "Therapy Session",
            ProgressType.Assessment => "Assessment",
            ProgressType.Milestone => "Milestone",
            ProgressType.SelfReport => "Self Report",
            ProgressType.TherapistEvaluation => "Therapist Evaluation",
            _ => type.ToString()
        };
    }

    private static string GetCompletionStatusDisplayName(CompletionStatus status)
    {
        return status switch
        {
            CompletionStatus.NotStarted => "Not Started",
            CompletionStatus.InProgress => "In Progress",
            CompletionStatus.Completed => "Completed",
            CompletionStatus.PartiallyCompleted => "Partially Completed",
            CompletionStatus.Skipped => "Skipped",
            CompletionStatus.Modified => "Modified",
            _ => status.ToString()
        };
    }
}