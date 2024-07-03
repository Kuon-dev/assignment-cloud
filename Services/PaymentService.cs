// PaymentService.cs
using Cloud.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Cloud.Services {
  /// <summary>
  /// Service for handling payment-related operations.
  /// </summary>
  public class PaymentService : IPaymentService {
	private readonly ApplicationDbContext _context;
	private readonly string _stripeWebhookSecret;

	/// <summary>
	/// Initializes a new instance of the PaymentService class.
	/// </summary>
	/// <param name="context">The database context.</param>
	/// <param name="configuration">The configuration to get Stripe settings.</param>
	public PaymentService(ApplicationDbContext context, IConfiguration configuration) {
	  _context = context;
	  _stripeWebhookSecret = configuration["Stripe:WebhookSecret"];
	}

	/// <inheritdoc />
	public async Task<(IEnumerable<RentPaymentModel> Payments, int TotalCount)> GetAllPaymentsAsync(int page, int size) {
	  var totalCount = await _context.RentPayments.CountAsync();
	  var payments = await _context.RentPayments
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  return (payments, totalCount);
	}

	/// <inheritdoc />
	public async Task<RentPaymentModel?> GetPaymentByIdAsync(Guid id) {
	  return await _context.RentPayments.FindAsync(id);
	}

	/// <inheritdoc />
	public async Task<RentPaymentModel> CreatePaymentAsync(RentPaymentModel payment) {
	  payment.Id = Guid.NewGuid();
	  payment.CreatedAt = DateTime.UtcNow;
	  payment.UpdatedAt = DateTime.UtcNow;

	  _context.RentPayments.Add(payment);
	  await _context.SaveChangesAsync();

	  return payment;
	}

	/// <inheritdoc />
	public async Task<RentPaymentModel?> UpdatePaymentAsync(Guid id, RentPaymentModel payment) {
	  var existingPayment = await _context.RentPayments.FindAsync(id);

	  if (existingPayment == null) {
		return null;
	  }

	  existingPayment.Amount = payment.Amount;
	  existingPayment.Currency = payment.Currency;
	  existingPayment.PaymentIntentId = payment.PaymentIntentId;
	  existingPayment.PaymentMethodId = payment.PaymentMethodId;
	  existingPayment.Status = payment.Status;
	  existingPayment.UpdatedAt = DateTime.UtcNow;

	  await _context.SaveChangesAsync();

	  return existingPayment;
	}

	/// <inheritdoc />
	public async Task<bool> DeletePaymentAsync(Guid id) {
	  var payment = await _context.RentPayments.FindAsync(id);

	  if (payment == null) {
		return false;
	  }

	  _context.RentPayments.Remove(payment);
	  await _context.SaveChangesAsync();

	  return true;
	}

	/// <inheritdoc />
	public async Task<(IEnumerable<RentPaymentModel> Payments, int TotalCount)> GetPaymentsByUserIdAsync(string userId, int page, int size) {
	  var query = _context.RentPayments
		  .Where(p => p.Tenant.UserId == userId);

	  var totalCount = await query.CountAsync();
	  var payments = await query
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  return (payments, totalCount);
	}

	/// <inheritdoc />
	public async Task<(IEnumerable<RentPaymentModel> Payments, int TotalCount)> GetPaymentsByPropertyIdAsync(Guid propertyId, int page, int size) {
	  var query = _context.RentPayments
		  .Where(p => p.Tenant.CurrentPropertyId == propertyId);

	  var totalCount = await query.CountAsync();
	  var payments = await query
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  return (payments, totalCount);
	}

	/// <inheritdoc />
	public async Task HandleStripeWebhookAsync(string json, string signature) {
	  try {
		var stripeEvent = EventUtility.ConstructEvent(json, signature, _stripeWebhookSecret);

		switch (stripeEvent.Type) {
		  case Events.PaymentIntentSucceeded:
			var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
			await HandlePaymentIntentSucceededAsync(paymentIntent);
			break;
			// Add more cases for other event types as needed
		}
	  }
	  catch (StripeException e) {
		// Handle exception (e.g., log error, send notification)
		throw;
	  }
	}

	private async Task HandlePaymentIntentSucceededAsync(PaymentIntent paymentIntent) {
	  var payment = await _context.RentPayments
		  .FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntent.Id);

	  if (payment != null) {
		payment.Status = PaymentStatus.Succeeded;
		payment.UpdatedAt = DateTime.UtcNow;
		await _context.SaveChangesAsync();
	  }
	}
  }
}