using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Cloud.Services;
using Cloud.Models;
using System.Security.Claims;

namespace Cloud.Controllers
{
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

		[HttpPost("periods")]
		public async Task<ActionResult<PayoutPeriod>> CreatePayoutPeriod([FromBody] CreatePayoutPeriodDto dto)
		{
			try
			{
				var payoutPeriod = await _payoutService.CreatePayoutPeriodAsync(dto.StartDate, dto.EndDate);
				return CreatedAtAction(nameof(GetPayoutPeriod), new { id = payoutPeriod.Id }, payoutPeriod);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating payout period");
				return StatusCode(500, "An error occurred while creating the payout period");
			}
		}

		[HttpGet("periods")]
		public async Task<ActionResult<IEnumerable<PayoutPeriod>>> GetPayoutPeriods()
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

		[HttpGet("periods/{id}")]
		public async Task<ActionResult<PayoutPeriod>> GetPayoutPeriod(Guid id)
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

		[HttpPost("owner-payouts")]
		public async Task<ActionResult<OwnerPayout>> CreateOwnerPayout([FromBody] CreateOwnerPayoutDto dto)
		{
			try
			{
				var payout = await _payoutService.CreateOwnerPayoutAsync(dto.OwnerId, dto.PayoutPeriodId, dto.Amount);
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

		[HttpGet("owner-payouts")]
		public async Task<ActionResult<IEnumerable<OwnerPayout>>> GetOwnerPayouts([FromQuery] Guid ownerId)
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

		[HttpGet("owner-payouts/{id}")]
		public async Task<ActionResult<OwnerPayout>> GetOwnerPayout(Guid id)
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

		[HttpPost("owner-payouts/{id}/process")]
		public async Task<ActionResult<OwnerPayout>> ProcessOwnerPayout(Guid id)
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

		[HttpGet("settings")]
		public async Task<ActionResult<PayoutSettings>> GetPayoutSettings()
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

		[HttpPut("settings")]
		public async Task<ActionResult<PayoutSettings>> UpdatePayoutSettings([FromBody] PayoutSettings settings)
		{
			try
			{
				var updatedSettings = await _payoutService.UpdatePayoutSettingsAsync(settings);
				return Ok(updatedSettings);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating payout settings");
				return StatusCode(500, "An error occurred while updating payout settings");
			}
		}
	}

	public class CreatePayoutPeriodDto
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}

	public class CreateOwnerPayoutDto
	{
		public Guid OwnerId { get; set; }
		public Guid PayoutPeriodId { get; set; }
		public decimal Amount { get; set; }
	}
}