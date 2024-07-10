using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cloud.Services;
using Cloud.Models.DTO;

namespace Cloud.Controllers
{
	/// <summary>
	/// Controller for handling payment-related operations
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = "Tenant")]
	public class PaymentController : ControllerBase
	{
		private readonly IPaymentService _paymentService;

		public PaymentController(IPaymentService paymentService)
		{
			_paymentService = paymentService;
		}

		/// <summary>
		/// Creates a new payment intent
		/// </summary>
		/// <param name="createPaymentDto">The DTO containing payment details</param>
		/// <returns>The client secret for the created payment intent</returns>
		[HttpPost("create-intent")]
		public async Task<ActionResult<CreatePaymentIntentResponseDto>> CreatePaymentIntent([FromBody] CreatePaymentIntentDto createPaymentDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value ?? throw new UnauthorizedAccessException());

			try
			{
				var clientSecret = await _paymentService.CreatePaymentIntentAsync(tenantId, createPaymentDto.Amount);
				return Ok(new CreatePaymentIntentResponseDto { ClientSecret = clientSecret });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
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