// PaymentService.cs
using Cloud.Factories;
using Cloud.Models;
using Cloud.Models.DTO;
/*using Cloud.Models.Validator;*/
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Cloud.Services {
  /// <summary>
  /// Interface for payment-related operations.
  /// </summary>
  public interface IPaymentService {
	/// <summary>
	/// Get all payments with pagination.
	/// </summary>
	/// <param name="page">The page number.</param>
	/// <param name="size">The number of items per page.</param>
	/// <returns>A paginated list of payments.</returns>
	Task<(IEnumerable<RentPaymentModel> Payments, int TotalCount)> GetAllPaymentsAsync(int page, int size);

	/// <summary>
	/// Get a specific payment by ID.
	/// </summary>
	/// <param name="id">The ID of the payment.</param>
	/// <returns>The payment if found, null otherwise.</returns>
	Task<RentPaymentModel> GetPaymentByIdAsync(Guid id);

	/// <summary>
	/// Create a new payment.
	/// </summary>
	/// <param name="payment">The payment to create.</param>
	/// <returns>The created payment.</returns>
	Task<RentPaymentModel> CreatePaymentAsync(CreateRentPaymentDto paymentDto, String userId);

	/// <summary>
	/// Update an existing payment.
	/// </summary>
	/// <param name="id">The ID of the payment to update.</param>
	/// <param name="payment">The updated payment information.</param>
	/// <returns>The updated payment if found, null otherwise.</returns>
	Task<RentPaymentModel> UpdatePaymentAsync(Guid id, RentPaymentModel payment);

	/// <summary>
	/// Delete a payment.
	/// </summary>
	/// <param name="id">The ID of the payment to delete.</param>
	/// <returns>True if the payment was deleted, false otherwise.</returns>
	Task<bool> DeletePaymentAsync(Guid id);

	/// <summary>
	/// Get all payments for a specific user with pagination.
	/// </summary>
	/// <param name="userId">The ID of the user.</param>
	/// <param name="page">The page number.</param>
	/// <param name="size">The number of items per page.</param>
	/// <returns>A paginated list of payments for the specified user.</returns>
	Task<(IEnumerable<RentPaymentModel> Payments, int TotalCount)> GetPaymentsByUserIdAsync(string userId, int page, int size);

	/// <summary>
	/// Get all payments for a specific property with pagination.
	/// </summary>
	/// <param name="propertyId">The ID of the property.</param>
	/// <param name="page">The page number.</param>
	/// <param name="size">The number of items per page.</param>
	/// <returns>A paginated list of payments for the specified property.</returns>
	Task<(IEnumerable<RentPaymentModel> Payments, int TotalCount)> GetPaymentsByPropertyIdAsync(Guid propertyId, int page, int size);

	/// <summary>
	/// Handle Stripe webhook events.
	/// </summary>
	/// <param name="json">The JSON payload from Stripe.</param>
	/// <param name="signature">The Stripe signature header.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/*Task HandleStripeWebhookAsync(string json, string signature);*/
  }
  /// <summary>
  /// Service for handling payment-related operations.
  /// </summary>
  public class PaymentService : IPaymentService {
	private readonly ApplicationDbContext _context;
	private readonly RentPaymentFactory? _rentPaymentFactory;

	/// <summary>
	/// Initializes a new instance of the PaymentService class.
	/// </summary>
	/// <param name="context">The database context.</param>
	/// <param name="configuration">The configuration to get Stripe settings.</param>
	public PaymentService(ApplicationDbContext context, IConfiguration configuration) {
	  _context = context ?? throw new ArgumentNullException(nameof(context));
	  _rentPaymentFactory = null;
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
	public async Task<RentPaymentModel> GetPaymentByIdAsync(Guid id) {
	  var payments = await _context.RentPayments.FindAsync(id);
	  if (payments == null) throw new InvalidOperationException("Payment not found");
	  return payments;
	}

	/// <inheritdoc />
	public async Task<RentPaymentModel> CreatePaymentAsync(CreateRentPaymentDto paymentDto, string createdBy) {
	  if (paymentDto == null || _rentPaymentFactory == null) {
		if (_rentPaymentFactory == null) {
		  throw new ArgumentNullException(nameof(_rentPaymentFactory));
		}
		throw new ArgumentNullException(nameof(paymentDto));
	  }

	  var tenant = await _context.Tenants.FindAsync(paymentDto.TenantId);
	  if (tenant == null || tenant.Id.ToString() != createdBy) {
		throw new InvalidOperationException("Invalid tenant");
	  }

	  var payment = await _rentPaymentFactory.CreatePaymentAsync(paymentDto);
	  payment.UpdateCreationProperties(DateTime.UtcNow);

	  await _context.SaveChangesAsync();
	  return payment;
	}

	/// <inheritdoc />
	public async Task<RentPaymentModel> UpdatePaymentAsync(Guid id, RentPaymentModel payment) {
	  var existingPayment = await _context.RentPayments.FindAsync(id);

	  if (existingPayment == null) throw new InvalidOperationException("Payment not found");

	  existingPayment.Amount = payment.Amount;
	  existingPayment.Currency = payment.Currency;
	  existingPayment.PaymentIntentId = payment.PaymentIntentId;
	  existingPayment.PaymentMethodId = payment.PaymentMethodId;
	  existingPayment.Status = payment.Status;
	  existingPayment.UpdateModifiedProperties(DateTime.UtcNow);

	  await _context.SaveChangesAsync();

	  return existingPayment;
	}

	/// <inheritdoc />
	public async Task<bool> DeletePaymentAsync(Guid id) {
	  var payment = await _context.RentPayments.FindAsync(id);

	  if (payment == null) {
		return false;
	  }

	  payment.UpdateIsDeleted(DateTime.UtcNow, true);
	  await _context.SaveChangesAsync();

	  return true;
	}

	/// <inheritdoc />
	public async Task<(IEnumerable<RentPaymentModel> Payments, int TotalCount)> GetPaymentsByUserIdAsync(string userId, int page, int size) {
	  var query = _context.RentPayments
		.Where(p => p.Tenant != null && p.Tenant.UserId != null && p.Tenant.UserId == userId);

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
		  .Where(p => p.Tenant != null && p.Tenant.CurrentPropertyId.HasValue && p.Tenant.CurrentPropertyId == propertyId);

	  var totalCount = await query.CountAsync();
	  var payments = await query
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  return (payments, totalCount);
	}

	private async Task HandlePaymentIntentSucceededAsync(PaymentIntent paymentIntent) {
	  var payment = await _context.RentPayments
		  .FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntent.Id);

	  if (payment != null) {
		payment.Status = PaymentStatus.Succeeded;
		payment.UpdateModifiedProperties(DateTime.UtcNow);
		await _context.SaveChangesAsync();
	  }
	}
  }
}