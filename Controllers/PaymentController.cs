using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cloud.Models;
using Cloud.Models.DTO;
using Cloud.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Cloud.Controllers
{
	/// <summary>
	/// Controller for handling payment-related operations
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class PaymentController : ControllerBase
	{
		private readonly IPaymentService _paymentService;
		private readonly ApplicationDbContext _context;
		private readonly ILogger<PaymentController> _logger;

		public PaymentController(IPaymentService paymentService, ApplicationDbContext context, ILogger<PaymentController> logger)
		{
			_paymentService = paymentService;
			_context = context;
			_logger = logger;
		}

		/// <summary>
		/// Creates a new payment intent
		/// </summary>
		/// <param name="createPaymentDto">The DTO containing payment details</param>
		/// <returns>The client secret for the created payment intent</returns>
		[HttpPost("create-intent")]
		[Authorize(Roles = "Tenant")]
		public async Task<ActionResult<CreatePaymentIntentResponseDto>> CreatePaymentIntent([FromBody] CreatePaymentIntentDto createPaymentDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = (User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
			var tenantId = _context.Tenants.FirstOrDefault(t => t.User != null && t.User.Id == userId)?.Id ?? throw new NotFoundException("Tenant not found");
			var propertyId = Guid.Parse(createPaymentDto.PropertyId);

			if (!_context.Properties.Any(p => p.Id == propertyId))
			{
				return NotFound("Property not found");
			}

			try
			{
				var clientSecret = await _paymentService.CreatePaymentIntentAsync(tenantId, propertyId, createPaymentDto.Amount);
				return Ok(new CreatePaymentIntentResponseDto { ClientSecret = clientSecret });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				// Log the exception
				_logger.LogError(ex, "An error occurred while creating a payment intent");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		/// <summary>
		/// Processes a successful payment
		/// </summary>
		/// <param name="paymentIntentId">The ID of the successful PaymentIntent</param>
		/// <returns>A success message if the payment was processed successfully</returns>
		[HttpPost("process-payment")]
		public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto processPaymentDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var success = await _paymentService.ProcessSuccessfulPaymentAsync(processPaymentDto.PaymentIntentId);
			if (success)
			{
				return Ok(new { Message = "Payment processed successfully" });
			}
			return BadRequest(new { Message = "Failed to process payment" });
		}

		/// <summary>
		/// Cancels a payment
		/// </summary>
		/// <param name="paymentIntentId">The ID of the PaymentIntent to cancel</param>
		/// <returns>A success message if the payment was cancelled successfully</returns>
		[HttpPost("cancel-payment")]
		public async Task<IActionResult> CancelPayment([FromBody] CancelPaymentDto cancelPaymentDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var success = await _paymentService.CancelPaymentAsync(cancelPaymentDto.PaymentIntentId);
			if (success)
			{
				return Ok(new { Message = "Payment cancelled successfully" });
			}
			return BadRequest(new { Message = "Failed to cancel payment" });
		}

		/// <summary>
		/// Gets a rent payment by its ID
		/// </summary>
		/// <param name="paymentId">The ID of the rent payment</param>
		/// <returns>The rent payment if found</returns>
		[HttpGet("{paymentId}")]
		public async Task<ActionResult<RentPaymentDto>> GetRentPayment(Guid paymentId)
		{
			var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? throw new UnauthorizedAccessException());
			var rentPayment = await _paymentService.GetRentPaymentByIdAsync(paymentId, tenantId);

			if (rentPayment == null)
			{
				return NotFound();
			}

			return Ok(new RentPaymentDto
			{
				Id = rentPayment.Id,
				Amount = rentPayment.Amount,
				Currency = rentPayment.Currency,
				Status = rentPayment.Status.ToString(),
				CreatedAt = rentPayment.CreatedAt
			});
		}

		/// <summary>
		/// Gets all rent payments for the current tenant
		/// </summary>
		/// <returns>A list of rent payments for the tenant</returns>
		[HttpGet]
		public async Task<ActionResult<List<RentPaymentDto>>> GetRentPayments()
		{
			var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? throw new UnauthorizedAccessException());
			var rentPayments = await _paymentService.GetRentPaymentsForTenantAsync(tenantId);

			var rentPaymentDtos = rentPayments.Select(rp => new RentPaymentDto
			{
				Id = rp.Id,
				Amount = rp.Amount,
				Currency = rp.Currency,
				Status = rp.Status.ToString(),
				CreatedAt = rp.CreatedAt
			}).ToList();

			return Ok(rentPaymentDtos);
		}
	}
}