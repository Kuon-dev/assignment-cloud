// OwnerPaymentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cloud.Models;
using Cloud.Services;

namespace Cloud.Controllers {
  [Authorize]
  [ApiController]
  [Route("api/[controller]")]
  public class OwnerPaymentsController : ControllerBase {
	private readonly IOwnerPaymentService _ownerPaymentService;

	public OwnerPaymentsController(IOwnerPaymentService ownerPaymentService) {
	  _ownerPaymentService = ownerPaymentService;
	}

	/// <summary>
	/// Get all owner payments with pagination
	/// </summary>
	/// <param name="page">Page number</param>
	/// <param name="size">Page size</param>
	/// <returns>A list of owner payments</returns>
	[HttpGet]
	public async Task<ActionResult<IEnumerable<OwnerPaymentModel>>> GetOwnerPayments([FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var payments = await _ownerPaymentService.GetAllPaymentsAsync(page, size);
	  return Ok(payments);
	}

	/// <summary>
	/// Get a specific owner payment by ID
	/// </summary>
	/// <param name="id">Payment ID</param>
	/// <returns>The owner payment</returns>
	[HttpGet("{id}")]
	public async Task<ActionResult<OwnerPaymentModel>> GetOwnerPayment(Guid id) {
	  var payment = await _ownerPaymentService.GetPaymentByIdAsync(id);

	  if (payment == null) {
		return NotFound();
	  }

	  return Ok(payment);
	}

	/// <summary>
	/// Create a new owner payment
	/// </summary>
	/// <param name="payment">The owner payment to create</param>
	/// <returns>The created owner payment</returns>
	[HttpPost]
	public async Task<ActionResult<OwnerPaymentModel>> CreateOwnerPayment(OwnerPaymentModel payment) {
	  var createdPayment = await _ownerPaymentService.CreatePaymentAsync(payment);
	  return CreatedAtAction(nameof(GetOwnerPayment), new { id = createdPayment.Id }, createdPayment);
	}

	/// <summary>
	/// Update an existing owner payment
	/// </summary>
	/// <param name="id">Payment ID</param>
	/// <param name="payment">The updated owner payment</param>
	/// <returns>No content</returns>
	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateOwnerPayment(Guid id, OwnerPaymentModel payment) {
	  var updatedPayment = await _ownerPaymentService.UpdatePaymentAsync(id, payment);

	  if (updatedPayment == null) {
		return NotFound();
	  }

	  return NoContent();
	}

	/// <summary>
	/// Delete an owner payment
	/// </summary>
	/// <param name="id">Payment ID</param>
	/// <returns>No content</returns>
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteOwnerPayment(Guid id) {
	  var result = await _ownerPaymentService.DeletePaymentAsync(id);

	  if (!result) {
		return NotFound();
	  }

	  return NoContent();
	}

	/// <summary>
	/// Get all payments for a specific owner with pagination
	/// </summary>
	/// <param name="ownerId">Owner ID</param>
	/// <param name="page">Page number</param>
	/// <param name="size">Page size</param>
	/// <returns>A list of owner payments</returns>
	[HttpGet("owner/{ownerId}")]
	public async Task<ActionResult<IEnumerable<OwnerPaymentModel>>> GetPaymentsByOwnerId(Guid ownerId, [FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var payments = await _ownerPaymentService.GetPaymentsByOwnerIdAsync(ownerId, page, size);
	  return Ok(payments);
	}

	/// <summary>
	/// Get all payments for a specific property with pagination
	/// </summary>
	/// <param name="propertyId">Property ID</param>
	/// <param name="page">Page number</param>
	/// <param name="size">Page size</param>
	/// <returns>A list of owner payments</returns>
	[HttpGet("property/{propertyId}")]
	public async Task<ActionResult<IEnumerable<OwnerPaymentModel>>> GetPaymentsByPropertyId(Guid propertyId, [FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var payments = await _ownerPaymentService.GetPaymentsByPropertyIdAsync(propertyId, page, size);
	  return Ok(payments);
	}

	/// <summary>
	/// Process a Stripe payment for an owner payment
	/// </summary>
	/// <param name="id">Payment ID</param>
	/// <returns>The updated owner payment</returns>
	[HttpPost("{id}/process-payment")]
	public async Task<ActionResult<OwnerPaymentModel>> ProcessStripePayment(Guid id) {
	  var payment = await _ownerPaymentService.ProcessStripePaymentAsync(id);

	  if (payment == null) {
		return NotFound();
	  }

	  return Ok(payment);
	}
  }
}