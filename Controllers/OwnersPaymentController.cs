using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cloud.Services;
using Cloud.Models.DTO;
/*using Cloud.Filters;*/

namespace Cloud.Controllers
{
	/// <summary>
	/// Controller for managing owner payments
	/// </summary>
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class OwnerPaymentsController : ControllerBase
	{
		private readonly IOwnerPaymentService _ownerPaymentService;

		public OwnerPaymentsController(IOwnerPaymentService ownerPaymentService)
		{
			_ownerPaymentService = ownerPaymentService;
		}

		/// <summary>
		/// Get all owner payments with pagination
		/// </summary>
		/// <param name="page">Page number</param>
		/// <param name="size">Page size</param>
		/// <returns>A list of owner payments</returns>
		[HttpGet]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<ActionResult<IEnumerable<OwnerPaymentDto>>> GetOwnerPayments([FromQuery] int page = 1, [FromQuery] int size = 10)
		{
			var payments = await _ownerPaymentService.GetAllPaymentsAsync(page, size);
			var paymentDtos = payments.Select(p => new OwnerPaymentDto(p));
			return Ok(paymentDtos);
		}

		/// <summary>
		/// Get a specific owner payment by ID
		/// </summary>
		/// <param name="id">Payment ID</param>
		/// <returns>The owner payment</returns>
		[HttpGet("{id}")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<ActionResult<OwnerPaymentDto>> GetOwnerPayment(Guid id)
		{
			try
			{
				var payment = await _ownerPaymentService.GetPaymentByIdAsync(id);
				return Ok(new OwnerPaymentDto(payment));
			}
			catch (NotFoundException)
			{
				return NotFound();
			}
		}

		/// <summary>
		/// Create a new owner payment
		/// </summary>
		/// <param name="paymentDto">The owner payment to create</param>
		/// <returns>The created owner payment</returns>
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<OwnerPaymentDto>> CreateOwnerPayment([FromBody] CreateOwnerPaymentDto paymentDto)
		{
			try
			{
				var createdPayment = await _ownerPaymentService.CreatePaymentAsync(paymentDto.OwnerId, paymentDto.PropertyId, paymentDto.Amount);
				return CreatedAtAction(nameof(GetOwnerPayment), new { id = createdPayment.Id }, new OwnerPaymentDto(createdPayment));
			}
			catch (ValidationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Update an existing owner payment
		/// </summary>
		/// <param name="id">Payment ID</param>
		/// <param name="paymentDto">The updated owner payment</param>
		/// <returns>No content</returns>
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateOwnerPayment(Guid id, [FromBody] UpdateOwnerPaymentDto paymentDto)
		{
			try
			{
				var payment = await _ownerPaymentService.GetPaymentByIdAsync(id);
				payment.Amount = paymentDto.Amount;
				payment.Status = paymentDto.Status;

				await _ownerPaymentService.UpdatePaymentAsync(id, payment);
				return NoContent();
			}
			catch (NotFoundException)
			{
				return NotFound();
			}
			catch (ValidationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Delete an owner payment
		/// </summary>
		/// <param name="id">Payment ID</param>
		/// <returns>No content</returns>
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteOwnerPayment(Guid id)
		{
			try
			{
				await _ownerPaymentService.DeletePaymentAsync(id);
				return NoContent();
			}
			catch (NotFoundException)
			{
				return NotFound();
			}
		}

		/// <summary>
		/// Get all payments for a specific owner with pagination
		/// </summary>
		/// <param name="ownerId">Owner ID</param>
		/// <param name="page">Page number</param>
		/// <param name="size">Page size</param>
		/// <returns>A list of owner payments</returns>
		[HttpGet("owner/{ownerId}")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<ActionResult<IEnumerable<OwnerPaymentDto>>> GetPaymentsByOwnerId(Guid ownerId, [FromQuery] int page = 1, [FromQuery] int size = 10)
		{
			var payments = await _ownerPaymentService.GetPaymentsByOwnerIdAsync(ownerId, page, size);
			var paymentDtos = payments.Select(p => new OwnerPaymentDto(p));
			return Ok(paymentDtos);
		}

		/// <summary>
		/// Get all payments for a specific property with pagination
		/// </summary>
		/// <param name="propertyId">Property ID</param>
		/// <param name="page">Page number</param>
		/// <param name="size">Page size</param>
		/// <returns>A list of owner payments</returns>
		[HttpGet("property/{propertyId}")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<ActionResult<IEnumerable<OwnerPaymentDto>>> GetPaymentsByPropertyId(Guid propertyId, [FromQuery] int page = 1, [FromQuery] int size = 10)
		{
			var payments = await _ownerPaymentService.GetPaymentsByPropertyIdAsync(propertyId, page, size);
			var paymentDtos = payments.Select(p => new OwnerPaymentDto(p));
			return Ok(paymentDtos);
		}

		/// <summary>
		/// Process a Stripe payment for an owner payment
		/// </summary>
		/// <param name="id">Payment ID</param>
		/// <returns>The updated owner payment</returns>
		[HttpPost("{id}/process-payment")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<OwnerPaymentDto>> ProcessStripePayment(Guid id)
		{
			try
			{
				var payment = await _ownerPaymentService.ProcessStripePaymentAsync(id);
				return Ok(new OwnerPaymentDto(payment));
			}
			catch (NotFoundException)
			{
				return NotFound();
			}
			catch (ServiceException ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}