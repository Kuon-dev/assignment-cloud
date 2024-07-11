using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
/*using Microsoft.Extensions.Logging;*/
using Cloud.Services;
using Cloud.Models.DTO;

namespace Cloud.Controllers
{
	/// <summary>
	/// Controller for managing payout-related operations.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = "Admin")]
	public class PayoutController : ControllerBase
	{
		private readonly IPayoutService _payoutService;
		private readonly ILogger<PayoutController> _logger;

		public PayoutController(IPayoutService payoutService, ILogger<PayoutController> logger)
		{
			_payoutService = payoutService ?? throw new ArgumentNullException(nameof(payoutService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Creates a new payout period.
		/// </summary>
		/// <remarks>
		/// This endpoint is used to define a new period for which payouts will be calculated and processed.
		/// It's typically used to set up monthly or custom-length payout cycles.
		/// </remarks>
		/// <param name="dto">The data transfer object containing the start and end dates for the payout period.</param>
		/// <returns>The created payout period details.</returns>
		[HttpPost("periods")]
		public async Task<ActionResult<PayoutPeriodDto>> CreatePayoutPeriod([FromBody] CreatePayoutPeriodDto dto)
		{
			try
			{
				var payoutPeriod = await _payoutService.CreatePayoutPeriodAsync(dto);
				return CreatedAtAction(nameof(GetPayoutPeriod), new { id = payoutPeriod.Id }, payoutPeriod);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating payout period");
				return StatusCode(500, "An error occurred while creating the payout period");
			}
		}

		/// <summary>
		/// Retrieves all payout periods.
		/// </summary>
		/// <remarks>
		/// This endpoint returns a list of all payout periods that have been created.
		/// It can be used to view the history of payout cycles or to select a specific period for further operations.
		/// </remarks>
		/// <returns>A list of all payout periods.</returns>
		[HttpGet("periods")]
		public async Task<ActionResult<IEnumerable<PayoutPeriodDto>>> GetPayoutPeriods()
		{
			try
			{
				var periods = await _payoutService.GetPayoutPeriodsAsync();
				return Ok(periods);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving payout periods");
				return StatusCode(500, "An error occurred while retrieving payout periods");
			}
		}

		/// <summary>
		/// Retrieves a specific payout period by its ID.
		/// </summary>
		/// <remarks>
		/// This endpoint is used to get detailed information about a specific payout period.
		/// It can be used to view the status of a particular payout cycle or to gather information for reporting purposes.
		/// </remarks>
		/// <param name="id">The unique identifier of the payout period.</param>
		/// <returns>The details of the specified payout period.</returns>
		[HttpGet("periods/{id}")]
		public async Task<ActionResult<PayoutPeriodDto>> GetPayoutPeriod(Guid id)
		{
			try
			{
				var period = await _payoutService.GetPayoutPeriodAsync(id);
				if (period == null)
					return NotFound();
				return Ok(period);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving payout period");
				return StatusCode(500, "An error occurred while retrieving the payout period");
			}
		}

		/// <summary>
		/// Creates a new payout for an owner.
		/// </summary>
		/// <remarks>
		/// This endpoint is used to create a new payout record for a specific owner within a given payout period.
		/// It's typically used when calculating the amount due to an owner for a particular period.
		/// </remarks>
		/// <param name="dto">The data transfer object containing the owner ID, payout period ID, and amount.</param>
		/// <returns>The created owner payout details.</returns>
		[HttpPost("owner-payouts")]
		public async Task<ActionResult<OwnerPayoutDto>> CreateOwnerPayout([FromBody] CreateOwnerPayoutDto dto)
		{
			try
			{
				var payout = await _payoutService.CreateOwnerPayoutAsync(dto);
				return CreatedAtAction(nameof(GetOwnerPayout), new { id = payout.Id }, payout);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating owner payout");
				return StatusCode(500, "An error occurred while creating the owner payout");
			}
		}

		/// <summary>
		/// Retrieves all payouts for a specific owner.
		/// </summary>
		/// <remarks>
		/// This endpoint returns a list of all payouts associated with a given owner.
		/// It can be used to view an owner's payout history or to gather information for reporting and auditing purposes.
		/// </remarks>
		/// <param name="ownerId">The unique identifier of the owner.</param>
		/// <returns>A list of all payouts for the specified owner.</returns>
		[HttpGet("owner-payouts")]
		public async Task<ActionResult<IEnumerable<OwnerPayoutDto>>> GetOwnerPayouts([FromQuery] Guid ownerId)
		{
			try
			{
				var payouts = await _payoutService.GetOwnerPayoutsAsync(ownerId);
				return Ok(payouts);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving owner payouts");
				return StatusCode(500, "An error occurred while retrieving owner payouts");
			}
		}

		/// <summary>
		/// Retrieves a specific owner payout by its ID.
		/// </summary>
		/// <remarks>
		/// This endpoint is used to get detailed information about a specific owner payout.
		/// It can be used to view the status of a particular payout or to gather information for dispute resolution.
		/// </remarks>
		/// <param name="id">The unique identifier of the owner payout.</param>
		/// <returns>The details of the specified owner payout.</returns>
		[HttpGet("owner-payouts/{id}")]
		public async Task<ActionResult<OwnerPayoutDto>> GetOwnerPayout(Guid id)
		{
			try
			{
				var payout = await _payoutService.GetOwnerPayoutAsync(id);
				if (payout == null)
					return NotFound();
				return Ok(payout);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving owner payout");
				return StatusCode(500, "An error occurred while retrieving the owner payout");
			}
		}

		/// <summary>
		/// Processes a specific owner payout.
		/// </summary>
		/// <remarks>
		/// This endpoint is used to initiate the processing of a specific owner payout.
		/// It typically involves marking the payout as processed and potentially triggering the actual transfer of funds.
		/// </remarks>
		/// <param name="id">The unique identifier of the owner payout to process.</param>
		/// <returns>The updated details of the processed owner payout.</returns>
		[HttpPost("owner-payouts/{id}/process")]
		public async Task<ActionResult<OwnerPayoutDto>> ProcessOwnerPayout(Guid id)
		{
			try
			{
				var payout = await _payoutService.ProcessOwnerPayoutAsync(id);
				return Ok(payout);
			}
			catch (ArgumentException ex)
			{
				return NotFound(ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing owner payout");
				return StatusCode(500, "An error occurred while processing the owner payout");
			}
		}

		/// <summary>
		/// Retrieves the current payout settings.
		/// </summary>
		/// <remarks>
		/// This endpoint returns the current configuration for payout processing.
		/// It includes settings such as payout cutoff day, processing day, default currency, and minimum payout amount.
		/// </remarks>
		/// <returns>The current payout settings.</returns>
		[HttpGet("settings")]
		public async Task<ActionResult<PayoutSettingsDto>> GetPayoutSettings()
		{
			try
			{
				var settings = await _payoutService.GetPayoutSettingsAsync();
				return Ok(settings);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving payout settings");
				return StatusCode(500, "An error occurred while retrieving payout settings");
			}
		}

		/// <summary>
		/// Updates the payout settings.
		/// </summary>
		/// <remarks>
		/// This endpoint is used to modify the configuration for payout processing.
		/// It allows updating settings such as payout cutoff day, processing day, default currency, and minimum payout amount.
		/// </remarks>
		/// <param name="dto">The data transfer object containing the updated payout settings.</param>
		/// <returns>The updated payout settings.</returns>
		[HttpPut("settings")]
		public async Task<ActionResult<PayoutSettingsDto>> UpdatePayoutSettings([FromBody] UpdatePayoutSettingsDto dto)
		{
			try
			{
				var updatedSettings = await _payoutService.UpdatePayoutSettingsAsync(dto);
				return Ok(updatedSettings);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating payout settings");
				return StatusCode(500, "An error occurred while updating payout settings");
			}
		}
	}
}