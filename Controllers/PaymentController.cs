using Cloud.Models;
using Cloud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Cloud.Models.DTO;

namespace Cloud.Controllers {
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public class PaymentsController : ControllerBase {
	private readonly IPaymentService _paymentService;

	public PaymentsController(IPaymentService paymentService) {
	  _paymentService = paymentService;
	}

	[HttpGet]
	public async Task<IActionResult> GetAllPayments([FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var (payments, totalCount) = await _paymentService.GetAllPaymentsAsync(page, size);
	  return Ok(new { Payments = payments, TotalCount = totalCount });
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetPaymentById(Guid id) {
	  var payment = await _paymentService.GetPaymentByIdAsync(id);
	  if (payment == null) {
		return NotFound();
	  }
	  return Ok(payment);
	}

	[HttpPost]
	public async Task<IActionResult> CreatePayment([FromBody] CreateRentPaymentDto paymentDto) {
	  if (!ModelState.IsValid) {
		return BadRequest(ModelState);
	  }

	  var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
	  if (string.IsNullOrEmpty(userId)) {
		return Unauthorized();
	  }

	  var createdPayment = await _paymentService.CreatePaymentAsync(paymentDto, userId);
	  return CreatedAtAction(nameof(GetPaymentById), new { id = createdPayment.Id }, createdPayment);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] RentPaymentModel payment) {
	  if (!ModelState.IsValid) {
		return BadRequest(ModelState);
	  }

	  var updatedPayment = await _paymentService.UpdatePaymentAsync(id, payment);
	  if (updatedPayment == null) {
		return NotFound();
	  }
	  return Ok(updatedPayment);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeletePayment(Guid id) {
	  var result = await _paymentService.DeletePaymentAsync(id);
	  if (!result) {
		return NotFound();
	  }
	  return NoContent();
	}

	[HttpGet("user/{userId}")]
	public async Task<IActionResult> GetPaymentsByUserId(string userId, [FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var (payments, totalCount) = await _paymentService.GetPaymentsByUserIdAsync(userId, page, size);
	  return Ok(new { Payments = payments, TotalCount = totalCount });
	}

	[HttpGet("property/{propertyId}")]
	public async Task<IActionResult> GetPaymentsByPropertyId(Guid propertyId, [FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var (payments, totalCount) = await _paymentService.GetPaymentsByPropertyIdAsync(propertyId, page, size);
	  return Ok(new { Payments = payments, TotalCount = totalCount });
	}
  }
}