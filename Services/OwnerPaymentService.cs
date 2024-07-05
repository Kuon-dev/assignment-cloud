using Cloud.Models;
using Cloud.Factories;
using Cloud.Models.Validator;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Cloud.Services {
  public interface IOwnerPaymentService {
	Task<IEnumerable<OwnerPaymentModel>> GetAllPaymentsAsync(int page, int size);
	Task<OwnerPaymentModel> GetPaymentByIdAsync(Guid id);
	Task<OwnerPaymentModel> CreatePaymentAsync(Guid ownerId, Guid propertyId, decimal amount);
	Task<OwnerPaymentModel> UpdatePaymentAsync(Guid id, OwnerPaymentModel payment);
	Task<bool> DeletePaymentAsync(Guid id);
	Task<IEnumerable<OwnerPaymentModel>> GetPaymentsByOwnerIdAsync(Guid ownerId, int page, int size);
	Task<IEnumerable<OwnerPaymentModel>> GetPaymentsByPropertyIdAsync(Guid propertyId, int page, int size);
	Task<OwnerPaymentModel> ProcessStripePaymentAsync(Guid paymentId);
  }

  /// <summary>
  /// Service for managing owner payments.
  /// </summary>
  public class OwnerPaymentService : IOwnerPaymentService {
	private readonly ApplicationDbContext _context;
	private readonly PaymentIntentService _paymentIntentService;
	private readonly OwnerPaymentFactory _paymentFactory;
	private readonly OwnerPaymentValidator _paymentValidator;

	public OwnerPaymentService(
		ApplicationDbContext context,
		PaymentIntentService paymentIntentService,
		OwnerPaymentFactory paymentFactory,
		OwnerPaymentValidator paymentValidator) {
	  _context = context ?? throw new ArgumentNullException(nameof(context));
	  _paymentIntentService = paymentIntentService ?? throw new ArgumentNullException(nameof(paymentIntentService));
	  _paymentFactory = paymentFactory ?? throw new ArgumentNullException(nameof(paymentFactory));
	  _paymentValidator = paymentValidator ?? throw new ArgumentNullException(nameof(paymentValidator));
	}

	public async Task<IEnumerable<OwnerPaymentModel>> GetAllPaymentsAsync(int page, int size) {
	  try {
		return await _context.OwnerPayments
			.Skip((page - 1) * size)
			.Take(size)
			.ToListAsync();
	  }
	  catch (Exception ex) {
		throw new ServiceException("Failed to retrieve owner payments.", ex);
	  }
	}

	public async Task<OwnerPaymentModel> GetPaymentByIdAsync(Guid id) {
	  try {
		var payment = await _context.OwnerPayments.FindAsync(id);
		if (payment == null) {
		  throw new Exception($"Owner payment with ID {id} not found.");
		}
		return payment;
	  }
	  catch (Exception ex) when (ex is not NotFoundException) {
		throw new ServiceException($"Failed to retrieve owner payment with ID {id}.", ex);
	  }
	}

	public async Task<OwnerPaymentModel> CreatePaymentAsync(Guid ownerId, Guid propertyId, decimal amount) {
	  try {
		var payment = await _paymentFactory.CreatePaymentAsync(ownerId, propertyId, amount, OwnerPaymentStatus.Pending);
		_paymentValidator.ValidatePayment(payment);
		return payment;
	  }
	  catch (Exception ex) {
		throw new ServiceException("Failed to create owner payment.", ex);
	  }
	}

	public async Task<OwnerPaymentModel> UpdatePaymentAsync(Guid id, OwnerPaymentModel payment) {
	  try {
		if (id != payment.Id) {
		  throw new ArgumentException("ID mismatch between route and payment model.");
		}

		_paymentValidator.ValidatePayment(payment);
		_context.Entry(payment).State = EntityState.Modified;
		await _context.SaveChangesAsync();
		return payment;
	  }
	  catch (DbUpdateConcurrencyException) {
		if (!await OwnerPaymentExistsAsync(id)) {
		  throw new Exception($"Owner payment with ID {id} not found.");
		}
		throw;
	  }
	  catch (Exception ex) when (ex is not NotFoundException && ex is not ArgumentException) {
		throw new ServiceException($"Failed to update owner payment with ID {id}.", ex);
	  }
	}

	public async Task<bool> DeletePaymentAsync(Guid id) {
	  try {
		var payment = await _context.OwnerPayments.FindAsync(id);
		if (payment == null) {
		  return false;
		}

		_context.OwnerPayments.Remove(payment);
		await _context.SaveChangesAsync();
		return true;
	  }
	  catch (Exception ex) {
		throw new ServiceException($"Failed to delete owner payment with ID {id}.", ex);
	  }
	}

	public async Task<IEnumerable<OwnerPaymentModel>> GetPaymentsByOwnerIdAsync(Guid ownerId, int page, int size) {
	  try {
		return await _context.OwnerPayments
			.Where(p => p.OwnerId == ownerId)
			.Skip((page - 1) * size)
			.Take(size)
			.ToListAsync();
	  }
	  catch (Exception ex) {
		throw new ServiceException($"Failed to retrieve owner payments for owner ID {ownerId}.", ex);
	  }
	}

	public async Task<IEnumerable<OwnerPaymentModel>> GetPaymentsByPropertyIdAsync(Guid propertyId, int page, int size) {
	  try {
		return await _context.OwnerPayments
			.Where(p => p.PropertyId == propertyId)
			.Skip((page - 1) * size)
			.Take(size)
			.ToListAsync();
	  }
	  catch (Exception ex) {
		throw new ServiceException($"Failed to retrieve owner payments for property ID {propertyId}.", ex);
	  }
	}

	public async Task<OwnerPaymentModel> ProcessStripePaymentAsync(Guid paymentId) {
	  try {
		var payment = await _context.OwnerPayments.FindAsync(paymentId);
		if (payment == null) {
		  throw new NotFoundException($"Owner payment with ID {paymentId} not found.");
		}

		var options = new PaymentIntentCreateOptions {
		  Amount = (long)(payment.Amount * 100),
		  Currency = "usd",
		  PaymentMethodTypes = new List<string> { "card" },
		  Metadata = new Dictionary<string, string>
			{
						{ "PaymentId", payment.Id.ToString() },
						{ "OwnerId", payment.OwnerId.ToString() },
						{ "PropertyId", payment.PropertyId.ToString() }
					}
		};

		var intent = await _paymentIntentService.CreateAsync(options);

		payment.StripePaymentIntentId = intent.Id;
		payment.Status = OwnerPaymentStatus.Pending;

		await _context.SaveChangesAsync();

		return payment;
	  }
	  catch (Exception ex) when (ex is not NotFoundException) {
		throw new ServiceException($"Failed to process Stripe payment for owner payment ID {paymentId}.", ex);
	  }
	}

	private async Task<bool> OwnerPaymentExistsAsync(Guid id) {
	  return await _context.OwnerPayments.AnyAsync(e => e.Id == id);
	}
  }

}