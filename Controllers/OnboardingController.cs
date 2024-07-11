using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Cloud.Models;
using Cloud.Services;
using Cloud.Models.DTO;

namespace Cloud.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class OnboardingController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IStripeService _stripeService;
		private readonly IEmailService _emailService;
		private readonly ILogger<OnboardingController> _logger;

		public OnboardingController(
			ApplicationDbContext context,
			IStripeService stripeService,
			IEmailService emailService,
			ILogger<OnboardingController> logger)
		{
			_context = context;
			_stripeService = stripeService;
			_emailService = emailService;
			_logger = logger;
		}

		[HttpPost("initiate")]
		public async Task<IActionResult> InitiateOnboarding()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return NotFound("User not found.");
			}

			if (user.Role != UserRole.Owner)
			{
				return BadRequest("Only owners can initiate onboarding.");
			}

			try
			{
				var stripeAccount = await _stripeService.CreateConnectedAccountAsync(user.Id);
				var onboardingLink = await _stripeService.CreateOnboardingLinkAsync(stripeAccount.Id.ToString());

				user.StripeCustomer.StripeCustomerId = stripeAccount.Id.ToString();
				await _context.SaveChangesAsync();

				return Ok(new { onboardingUrl = onboardingLink });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during onboarding initiation for user {UserId}", userId);
				return StatusCode(500, "An error occurred during onboarding initiation.");
			}
		}

		[HttpGet("status")]
		public async Task<IActionResult> GetOnboardingStatus()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var user = await _context.Users.FindAsync(userId);
			var stripeid = user?.StripeCustomer?.StripeCustomerId.ToString();
			if (user == null)
			{
				return NotFound("User not found.");
			}

			if (string.IsNullOrEmpty(stripeid))
			{
				return BadRequest("Onboarding has not been initiated.");
			}

			try
			{
				var accountStatus = await _stripeService.GetAccountStatusAsync(stripeid);
				return Ok(new OnboardingStatusDto
				{
					IsVerified = user.IsVerified,
					StripeAccountStatus = accountStatus
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching onboarding status for user {UserId}", userId);
				return StatusCode(500, "An error occurred while fetching onboarding status.");
			}
		}

		[HttpPost("complete")]
		public async Task<IActionResult> CompleteOnboarding()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return NotFound("User not found.");
			}

			var stripeid = user.StripeCustomer?.StripeCustomerId.ToString();

			if (string.IsNullOrEmpty(stripeid))
			{
				return BadRequest("Onboarding has not been initiated.");
			}

			try
			{
				var accountStatus = await _stripeService.GetAccountStatusAsync(stripeid);
				if (accountStatus.DetailsSubmitted && accountStatus.PayoutsEnabled)
				{
					user.IsVerified = true;
					await _context.SaveChangesAsync();

					/*await _emailService.SendEmailAsync(user.Email, "Onboarding Completed", "Your account has been verified and you can now receive payments.");*/

					return Ok(new { message = "Onboarding completed successfully." });
				}
				else
				{
					return BadRequest("Onboarding is not yet complete. Please finish all required steps.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error completing onboarding for user {UserId}", userId);
				return StatusCode(500, "An error occurred while completing onboarding.");
			}
		}
	}
}