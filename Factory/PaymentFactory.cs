using Bogus;
using Cloud.Models;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Factories
{
	public interface IRentPaymentFactory
	{
		/// <summary>
		/// Creates a new RentPaymentModel instance
		/// </summary>
		/// <param name="tenantId">The ID of the tenant making the payment</param>
		/// <param name="amount">The amount to be paid in cents</param>
		/// <param name="currency">The currency of the payment</param>
		/// <param name="paymentIntentId">The Stripe PaymentIntent ID</param>
		/// <param name="status">The initial status of the payment</param>
		/// <returns>A new RentPaymentModel instance</returns>
		RentPaymentModel Create(Guid tenantId, Guid propertyId, int amount, string currency, string paymentIntentId, PaymentStatus status);
		Task SeedRentPaymentsAsync(int count);
	}

	/// <summary>
	/// Factory class for creating and seeding RentPaymentModel instances.
	/// </summary>
	public class RentPaymentFactory : IRentPaymentFactory
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly Faker<RentPaymentModel> _rentPaymentFaker;
		private readonly ILogger<RentPaymentFactory> _logger;

		/// <summary>
		/// Initializes a new instance of the RentPaymentFactory class.
		/// </summary>
		/// <param name="dbContext">The database context for entity operations.</param>
		/// <param name="logger">The logger for logging operations.</param>
		public RentPaymentFactory(ApplicationDbContext dbContext, ILogger<RentPaymentFactory> logger)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			// Initialize Bogus for generating fake rent payment data
			_rentPaymentFaker = new Faker<RentPaymentModel>()
				.RuleFor(r => r.Amount, f => f.Random.Int(10000, 1000000)) // Amount in cents (100.00 to 10,000.00)
				.RuleFor(r => r.Currency, f => f.PickRandom("usd", "eur", "gbp", "myr"))
				.RuleFor(r => r.PaymentIntentId, f => $"pi_{f.Random.AlphaNumeric(24)}")
				.RuleFor(r => r.PaymentMethodId, f => $"pm_{f.Random.AlphaNumeric(24)}")
				.RuleFor(r => r.Status, f => f.PickRandom<PaymentStatus>());
		}

		/// <summary>
		/// Creates a new RentPaymentModel instance.
		/// </summary>
		/// <param name="tenantId">The ID of the tenant making the payment.</param>
		/// <param name="amount">The amount to be paid in cents.</param>
		/// <param name="currency">The currency of the payment.</param>
		/// <param name="paymentIntentId">The Stripe PaymentIntent ID.</param>
		/// <param name="status">The initial status of the payment.</param>
		/// <returns>A new RentPaymentModel instance.</returns>
		public RentPaymentModel Create(Guid tenantId, Guid propertyId, int amount, string currency, string paymentIntentId, PaymentStatus status)
		{
			return new RentPaymentModel
			{
				TenantId = tenantId,
				PropertyId = propertyId,
				Amount = amount,
				Currency = currency,
				PaymentIntentId = paymentIntentId,
				Status = status
			};
		}

		/// <summary>
		/// Seeds the database with a specified number of fake rent payments.
		/// </summary>
		/// <param name="count">The number of rent payments to create.</param>
		public async Task SeedRentPaymentsAsync(int count)
		{
			try
			{
				if (_dbContext.RentPayments == null || _dbContext.Tenants == null)
				{
					throw new InvalidOperationException("RentPayments or Tenants DbSet is not initialized.");
				}

				var rentPayments = new List<RentPaymentModel>();
				var tenants = await _dbContext.Tenants.ToListAsync();
				var properties = await _dbContext.Properties.ToListAsync();

				if (!tenants.Any())
				{
					_logger.LogWarning("No tenants found in the database. Cannot seed rent payments.");
					return;
				}

				if (!properties.Any())
				{
					_logger.LogWarning("No properties found in the database. Cannot seed rent payments.");
					return;
				}

				for (int i = 0; i < count; i++)
				{
					var rentPayment = _rentPaymentFaker.Generate();
					rentPayment.TenantId = tenants[new Random().Next(tenants.Count)].Id;
					rentPayment.PropertyId = properties[new Random().Next(properties.Count)].Id;
					rentPayments.Add(rentPayment);
				}

				await _dbContext.RentPayments.AddRangeAsync(rentPayments);
				await _dbContext.SaveChangesAsync();

				_logger.LogInformation("Successfully seeded {Count} rent payments", count);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while seeding rent payments");
				throw;
			}
		}
	}
}