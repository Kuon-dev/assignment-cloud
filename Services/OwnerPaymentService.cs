using Cloud.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Cloud.Services {
  public interface IOwnerPaymentService {
	Task<IEnumerable<OwnerPaymentModel>> GetAllPaymentsAsync(int page, int size);
	Task<OwnerPaymentModel> GetPaymentByIdAsync(Guid id);
	Task<OwnerPaymentModel> CreatePaymentAsync(OwnerPaymentModel payment);
	Task<OwnerPaymentModel> UpdatePaymentAsync(Guid id, OwnerPaymentModel payment);
	Task<bool> DeletePaymentAsync(Guid id);
	Task<IEnumerable<OwnerPaymentModel>> GetPaymentsByOwnerIdAsync(Guid ownerId, int page, int size);
	Task<IEnumerable<OwnerPaymentModel>> GetPaymentsByPropertyIdAsync(Guid propertyId, int page, int size);
	Task<OwnerPaymentModel> ProcessStripePaymentAsync(Guid paymentId);
  }

  public class OwnerPaymentService : IOwnerPaymentService {
	private readonly ApplicationDbContext _context;
	private readonly PaymentIntentService _paymentIntentService;

	public OwnerPaymentService(ApplicationDbContext context, PaymentIntentService paymentIntentService) {
	  _context = context;
	  _paymentIntentService = paymentIntentService;
	}

	public async Task<IEnumerable<OwnerPaymentModel>> GetAllPaymentsAsync(int page, int size) {
	  return await _context.OwnerPayments
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();
	}

	public async Task<OwnerPaymentModel> GetPaymentByIdAsync(Guid id) {
	  if (!OwnerPaymentExists(id)) {
		throw new KeyNotFoundException();
	  }

	  if (_context.OwnerPayments == null)
		throw new InvalidOperationException();

	  return await _context.OwnerPayments.FindAsync(id) ?? throw new KeyNotFoundException();
	}

	public async Task<OwnerPaymentModel> CreatePaymentAsync(OwnerPaymentModel payment) {
	  _context.OwnerPayments.Add(payment);
	  await _context.SaveChangesAsync();
	  return payment;
	}

	public async Task<OwnerPaymentModel> UpdatePaymentAsync(Guid id, OwnerPaymentModel payment) {
	  if (id != payment.Id) {
		throw new ArgumentException();
	  }

	  _context.Entry(payment).State = EntityState.Modified;

	  try {
		await _context.SaveChangesAsync();
	  }
	  catch (DbUpdateConcurrencyException) {
		if (!OwnerPaymentExists(id)) {
		  throw new KeyNotFoundException();
		}
		else {
		  throw;
		}
	  }

	  return payment;
	}

	public async Task<bool> DeletePaymentAsync(Guid id) {
	  var payment = await _context.OwnerPayments.FindAsync(id);
	  if (payment == null) {
		return false;
	  }

	  _context.OwnerPayments.Remove(payment);
	  await _context.SaveChangesAsync();

	  return true;
	}

	public async Task<IEnumerable<OwnerPaymentModel>> GetPaymentsByOwnerIdAsync(Guid ownerId, int page, int size) {
	  return await _context.OwnerPayments
		  .Where(p => p.OwnerId == ownerId)
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();
	}

	public async Task<IEnumerable<OwnerPaymentModel>> GetPaymentsByPropertyIdAsync(Guid propertyId, int page, int size) {
	  return await _context.OwnerPayments
		  .Where(p => p.PropertyId == propertyId)
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();
	}

	public async Task<OwnerPaymentModel> ProcessStripePaymentAsync(Guid paymentId) {
	  var payment = await _context.OwnerPayments.FindAsync(paymentId);
	  if (payment == null) {
		throw new KeyNotFoundException();
	  }

	  var options = new PaymentIntentCreateOptions {
		Amount = (long)(payment.Amount * 100), // Stripe uses cents
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

	private bool OwnerPaymentExists(Guid id) {
	  return _context.OwnerPayments.Any(e => e.Id == id);
	}
  }
}