using Microsoft.EntityFrameworkCore;
using Stripe;
using Cloud.Models;
using Cloud.Factories;

namespace Cloud.Services
{
	/// <summary>
	/// Service for handling rent payments using Stripe
	/// </summary>
	public interface IPaymentService
	{
		/// <summary>
		/// Creates a payment intent for rent payment
		/// </summary>
		/// <param name="tenantId">The ID of the tenant making the payment</param>
		/// <param name="amount">The amount to be paid in cents</param>
		/// <returns>The created PaymentIntent client secret</returns>
		Task<string> CreatePaymentIntentAsync(Guid tenantId, int amount);

		/// <summary>
		/// Processes a successful payment
		/// </summary>
		/// <param name="paymentIntentId">The ID of the successful PaymentIntent</param>
		/// <returns>True if the payment was processed successfully, false otherwise</returns>
		Task<bool> ProcessSuccessfulPaymentAsync(string paymentIntentId);

		/// <summary>
		/// Cancels a payment intent
		/// </summary>
		/// <param name="paymentIntentId">The ID of the PaymentIntent to cancel</param>
		/// <returns>True if the payment was cancelled successfully, false otherwise</returns>
		Task<bool> CancelPaymentAsync(string paymentIntentId);

		/// <summary>
		/// Gets a rent payment by its ID
		/// </summary>
		/// <param name="paymentId">The ID of the rent payment</param>
		/// <param name="tenantId">The ID of the tenant</param>
		/// <returns>The rent payment if found, null otherwise</returns>
		Task<RentPaymentModel?> GetRentPaymentByIdAsync(Guid paymentId, Guid tenantId);

		/// <summary>
		/// Gets all rent payments for a tenant
		/// </summary>
		/// <param name="tenantId">The ID of the tenant</param>
		/// <returns>A list of rent payments for the tenant</returns>
		Task<List<RentPaymentModel>> GetRentPaymentsForTenantAsync(Guid tenantId);
	}

	public class PaymentService : IPaymentService
	{
		private readonly ApplicationDbContext _context;
		private readonly IRentPaymentFactory _rentPaymentFactory;

		public PaymentService(ApplicationDbContext context, IRentPaymentFactory rentPaymentFactory)
		{
			_context = context;
			_rentPaymentFactory = rentPaymentFactory;
		}

		public async Task<string> CreatePaymentIntentAsync(Guid tenantId, int amount)
		{
			var tenant = await _context.Tenants
				.Include(t => t.User)
				.ThenInclude(u => u.StripeCustomer)
				.FirstOrDefaultAsync(t => t.Id == tenantId);

			if (tenant == null)
			{
				throw new ArgumentException("Tenant not found");
			}

			var options = new PaymentIntentCreateOptions
			{
				Amount = amount,
				Currency = "myr",
				Customer = tenant.User?.StripeCustomer?.StripeCustomerId,
				SetupFutureUsage = "off_session",
				Metadata = new Dictionary<string, string>
				{
					{ "TenantId", tenantId.ToString() }
				}
			};

			var service = new PaymentIntentService();
			var paymentIntent = await service.CreateAsync(options);

			var rentPayment = _rentPaymentFactory.Create(tenantId, amount, "myr", paymentIntent.Id, MapStripeStatusToPaymentStatus(paymentIntent.Status));
			_context.RentPayments?.Add(rentPayment);
			await _context.SaveChangesAsync();

			return paymentIntent.ClientSecret;
		}

		public async Task<bool> ProcessSuccessfulPaymentAsync(string paymentIntentId)
		{
			if (_context.RentPayments == null)
			{
				throw new InvalidOperationException("RentPayments DbSet is null");
			}

			var rentPayment = await _context.RentPayments
				.FirstOrDefaultAsync(rp => rp.PaymentIntentId == paymentIntentId);

			if (rentPayment == null)
			{
				return false;
			}

			var service = new PaymentIntentService();
			var paymentIntent = await service.GetAsync(paymentIntentId);

			rentPayment.Status = MapStripeStatusToPaymentStatus(paymentIntent.Status);
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> CancelPaymentAsync(string paymentIntentId)
		{
			if (_context.RentPayments == null)
			{
				throw new InvalidOperationException("RentPayments DbSet is null");
			}

			var rentPayment = await _context.RentPayments
				.FirstOrDefaultAsync(rp => rp.PaymentIntentId == paymentIntentId);

			if (rentPayment == null)
			{
				return false;
			}

			var service = new PaymentIntentService();
			await service.CancelAsync(paymentIntentId);

			rentPayment.Status = PaymentStatus.Cancelled;
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<RentPaymentModel?> GetRentPaymentByIdAsync(Guid paymentId, Guid tenantId)
		{
			if (_context.RentPayments == null)
			{
				throw new InvalidOperationException("RentPayments DbSet is null");
			}

			return await _context.RentPayments
				.FirstOrDefaultAsync(rp => rp.Id == paymentId && rp.TenantId == tenantId);
		}

		public async Task<List<RentPaymentModel>> GetRentPaymentsForTenantAsync(Guid tenantId)
		{
			if (_context.RentPayments == null)
			{
				throw new InvalidOperationException("RentPayments DbSet is null");
			}

			return await _context.RentPayments
				.Where(rp => rp.TenantId == tenantId)
				.ToListAsync();
		}

		private PaymentStatus MapStripeStatusToPaymentStatus(string stripeStatus)
		{
			return stripeStatus switch
			{
				"requires_payment_method" => PaymentStatus.RequiresPaymentMethod,
				"requires_confirmation" => PaymentStatus.RequiresConfirmation,
				"requires_action" => PaymentStatus.RequiresAction,
				"processing" => PaymentStatus.Processing,
				"requires_capture" => PaymentStatus.RequiresCapture,
				"canceled" => PaymentStatus.Cancelled,
				"succeeded" => PaymentStatus.Succeeded,
				_ => throw new ArgumentException($"Unknown Stripe status: {stripeStatus}")
			};
		}
	}
}