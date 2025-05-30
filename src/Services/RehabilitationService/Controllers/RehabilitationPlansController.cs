using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RehabilitationService.DTOs;
using RehabilitationService.Models;
using RehabilitationService.Services;

namespace RehabilitationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RehabilitationPlansController : ControllerBase
{
    private readonly IRehabilitationService _rehabilitationService;
    private readonly ILogger<RehabilitationPlansController> _logger;

    public RehabilitationPlansController(IRehabilitationService rehabilitationService, ILogger<RehabilitationPlansController> logger)
    {
        _rehabilitationService = rehabilitationService;
        _logger = logger;
    }

    /// <summary>
    /// Get rehabilitation plans with filtering
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public async Task<ActionResult<IEnumerable<RehabilitationPlanDto>>> GetRehabilitationPlans([FromQuery] RehabilitationFilterDto filter)
    {
        try
        {
            var plans = await _rehabilitationService.GetRehabilitationPlansAsync(filter);
            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving rehabilitation plans");
            return StatusCode(500, "An error occurred while retrieving rehabilitation plans.");
        }
    }

    /// <summary>
    /// Get rehabilitation plan by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public async Task<ActionResult<RehabilitationPlanDto>> GetRehabilitationPlanById(Guid id)
    {
        try
        {
            var plan = await _rehabilitationService.GetRehabilitationPlanByIdAsync(id);

            if (plan == null)
                return NotFound($"Rehabilitation plan with ID {id} not found.");

            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving rehabilitation plan with ID: {PlanId}", id);
            return StatusCode(500, "An error occurred while retrieving the rehabilitation plan.");
        }
    }

    /// <summary>
    /// Get rehabilitation plans by patient ID
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public async Task<ActionResult<IEnumerable<RehabilitationPlanDto>>> GetRehabilitationPlansByPatientId(Guid patientId)
    {
        try
        {
            var plans = await _rehabilitationService.GetRehabilitationPlansByPatientIdAsync(patientId);
            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving rehabilitation plans for patient: {PatientId}", patientId);
            return StatusCode(500, "An error occurred while retrieving rehabilitation plans.");
        }
    }

    /// <summary>
    /// Get active rehabilitation plans
    /// </summary>
    [HttpGet("active")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public async Task<ActionResult<IEnumerable<RehabilitationPlanDto>>> GetActivePlans()
    {
        try
        {
            var plans = await _rehabilitationService.GetActivePlansAsync();
            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active rehabilitation plans");
            return StatusCode(500, "An error occurred while retrieving active rehabilitation plans.");
        }
    }

    /// <summary>
    /// Get plans needing attention (no recent progress)
    /// </summary>
    [HttpGet("needing-attention")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public async Task<ActionResult<IEnumerable<RehabilitationPlanDto>>> GetPlansNeedingAttention()
    {
        try
        {
            var plans = await _rehabilitationService.GetPlansNeedingAttentionAsync();
            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving plans needing attention");
            return StatusCode(500, "An error occurred while retrieving plans needing attention.");
        }
    }

    /// <summary>
    /// Create a new rehabilitation plan
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RehabilitationWritePolicy")]
    public async Task<ActionResult<RehabilitationPlanDto>> CreateRehabilitationPlan([FromBody] CreateRehabilitationPlanDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var plan = await _rehabilitationService.CreateRehabilitationPlanAsync(createDto);

            return CreatedAtAction(
                nameof(GetRehabilitationPlanById),
                new { id = plan.Id },
                plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating rehabilitation plan");
            return StatusCode(500, "An error occurred while creating the rehabilitation plan.");
        }
    }

    /// <summary>
    /// Update an existing rehabilitation plan
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RehabilitationWritePolicy")]
    public async Task<ActionResult<RehabilitationPlanDto>> UpdateRehabilitationPlan(Guid id, [FromBody] UpdateRehabilitationPlanDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var plan = await _rehabilitationService.UpdateRehabilitationPlanAsync(id, updateDto);

            if (plan == null)
                return NotFound($"Rehabilitation plan with ID {id} not found.");

            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating rehabilitation plan with ID: {PlanId}", id);
            return StatusCode(500, "An error occurred while updating the rehabilitation plan.");
        }
    }

    /// <summary>
    /// Delete a rehabilitation plan
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RehabilitationWritePolicy")]
    public async Task<ActionResult> DeleteRehabilitationPlan(Guid id)
    {
        try
        {
            var deleted = await _rehabilitationService.DeleteRehabilitationPlanAsync(id);

            if (!deleted)
                return NotFound($"Rehabilitation plan with ID {id} not found.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting rehabilitation plan with ID: {PlanId}", id);
            return StatusCode(500, "An error occurred while deleting the rehabilitation plan.");
        }
    }

    /// <summary>
    /// Activate a rehabilitation plan
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    [Authorize(Policy = "RehabilitationWritePolicy")]
    public async Task<ActionResult> ActivatePlan(Guid id)
    {
        try
        {
            var success = await _rehabilitationService.ActivatePlanAsync(id);

            if (!success)
                return NotFound($"Rehabilitation plan with ID {id} not found or cannot be activated.");

            return Ok(new { message = "Rehabilitation plan activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating rehabilitation plan with ID: {PlanId}", id);
            return StatusCode(500, "An error occurred while activating the rehabilitation plan.");
        }
    }

    /// <summary>
    /// Complete a rehabilitation plan
    /// </summary>
    [HttpPatch("{id:guid}/complete")]
    [Authorize(Policy = "RehabilitationWritePolicy")]
    public async Task<ActionResult> CompletePlan(Guid id)
    {
        try
        {
            var success = await _rehabilitationService.CompletePlanAsync(id);

            if (!success)
                return NotFound($"Rehabilitation plan with ID {id} not found or cannot be completed.");

            return Ok(new { message = "Rehabilitation plan completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing rehabilitation plan with ID: {PlanId}", id);
            return StatusCode(500, "An error occurred while completing the rehabilitation plan.");
        }
    }

    /// <summary>
    /// Put a rehabilitation plan on hold
    /// </summary>
    [HttpPatch("{id:guid}/hold")]
    [Authorize(Policy = "RehabilitationWritePolicy")]
    public async Task<ActionResult> PutPlanOnHold(Guid id, [FromBody] HoldPlanRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest("Reason is required to put plan on hold.");

            var success = await _rehabilitationService.PutPlanOnHoldAsync(id, request.Reason);

            if (!success)
                return NotFound($"Rehabilitation plan with ID {id} not found or cannot be put on hold.");

            return Ok(new { message = "Rehabilitation plan put on hold successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while putting rehabilitation plan on hold with ID: {PlanId}", id);
            return StatusCode(500, "An error occurred while putting the rehabilitation plan on hold.");
        }
    }

    /// <summary>
    /// Cancel a rehabilitation plan
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Policy = "RehabilitationWritePolicy")]
    public async Task<ActionResult> CancelPlan(Guid id, [FromBody] CancelPlanRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest("Reason is required to cancel plan.");

            var success = await _rehabilitationService.CancelPlanAsync(id, request.Reason);

            if (!success)
                return NotFound($"Rehabilitation plan with ID {id} not found or cannot be cancelled.");

            return Ok(new { message = "Rehabilitation plan cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling rehabilitation plan with ID: {PlanId}", id);
            return StatusCode(500, "An error occurred while cancelling the rehabilitation plan.");
        }
    }

    /// <summary>
    /// Get plan statistics
    /// </summary>
    [HttpGet("{id:guid}/statistics")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public async Task<ActionResult<object>> GetPlanStatistics(Guid id)
    {
        try
        {
            var statistics = await _rehabilitationService.GetPlanStatisticsAsync(id);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving plan statistics for ID: {PlanId}", id);
            return StatusCode(500, "An error occurred while retrieving plan statistics.");
        }
    }

    /// <summary>
    /// Get patient progress summary
    /// </summary>
    [HttpGet("patient/{patientId:guid}/summary")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public async Task<ActionResult<object>> GetPatientProgressSummary(Guid patientId)
    {
        try
        {
            var summary = await _rehabilitationService.GetPatientProgressSummaryAsync(patientId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving patient progress summary for: {PatientId}", patientId);
            return StatusCode(500, "An error occurred while retrieving patient progress summary.");
        }
    }

    /// <summary>
    /// Get all available plan types
    /// </summary>
    [HttpGet("types")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public ActionResult<object> GetPlanTypes()
    {
        try
        {
            var types = Enum.GetValues<PlanType>()
                .Select(t => new {
                    Value = (int)t,
                    Name = t.ToString(),
                    DisplayName = GetPlanTypeDisplayName(t)
                });

            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving plan types");
            return StatusCode(500, "An error occurred while retrieving plan types.");
        }
    }

    /// <summary>
    /// Get all available plan statuses
    /// </summary>
    [HttpGet("statuses")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public ActionResult<object> GetPlanStatuses()
    {
        try
        {
            var statuses = Enum.GetValues<PlanStatus>()
                .Select(s => new {
                    Value = (int)s,
                    Name = s.ToString(),
                    DisplayName = GetPlanStatusDisplayName(s)
                });

            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving plan statuses");
            return StatusCode(500, "An error occurred while retrieving plan statuses.");
        }
    }

    /// <summary>
    /// Get all available difficulty levels
    /// </summary>
    [HttpGet("difficulties")]
    [Authorize(Policy = "RehabilitationReadPolicy")]
    public ActionResult<object> GetDifficultyLevels()
    {
        try
        {
            var difficulties = Enum.GetValues<PlanDifficulty>()
                .Select(d => new {
                    Value = (int)d,
                    Name = d.ToString(),
                    DisplayName = GetDifficultyDisplayName(d)
                });

            return Ok(difficulties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving difficulty levels");
            return StatusCode(500, "An error occurred while retrieving difficulty levels.");
        }
    }

    private static string GetPlanTypeDisplayName(PlanType type)
    {
        return type switch
        {
            PlanType.Physical => "Physical Therapy",
            PlanType.Occupational => "Occupational Therapy",
            PlanType.Speech => "Speech Therapy",
            PlanType.Cardiac => "Cardiac Rehabilitation",
            PlanType.Neurological => "Neurological Rehabilitation",
            PlanType.Orthopedic => "Orthopedic Rehabilitation",
            PlanType.General => "General Rehabilitation",
            _ => type.ToString()
        };
    }

    private static string GetPlanStatusDisplayName(PlanStatus status)
    {
        return status switch
        {
            PlanStatus.Pending => "Pending",
            PlanStatus.Active => "Active",
            PlanStatus.Completed => "Completed",
            PlanStatus.OnHold => "On Hold",
            PlanStatus.Cancelled => "Cancelled",
            PlanStatus.Discontinued => "Discontinued",
            _ => status.ToString()
        };
    }

    private static string GetDifficultyDisplayName(PlanDifficulty difficulty)
    {
        return difficulty switch
        {
            PlanDifficulty.Beginner => "Beginner",
            PlanDifficulty.Intermediate => "Intermediate",
            PlanDifficulty.Advanced => "Advanced",
            PlanDifficulty.Expert => "Expert",
            _ => difficulty.ToString()
        };
    }
}

public class HoldPlanRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class CancelPlanRequest
{
    public string Reason { get; set; } = string.Empty;
}