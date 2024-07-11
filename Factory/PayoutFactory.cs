using Bogus;
using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cloud.Factories
{
	/// <summary>
	/// Seeder class for creating payout models with validations.
	/// </summary>
	public class PayoutFactory
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly Faker<PayoutPeriod> _payoutPeriodFaker;
		private readonly Faker<OwnerPayout> _ownerPayoutFaker;
		private readonly ILogger<PayoutFactory> _logger;

		/// <summary>
		/// Initializes a new instance of the PayoutFactory class.
		/// </summary>
		/// <param name="dbContext">The database context for entity operations.</param>
		/// <param name="logger">The logger for logging operations.</param>
		public PayoutFactory(ApplicationDbContext dbContext, ILogger<PayoutFactory> logger)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			// Initialize Bogus for generating fake payout period data
			_payoutPeriodFaker = new Faker<PayoutPeriod>()
				.RuleFor(p => p.StartDate, f => f.Date.Past(1).Date.ToUniversalTime())
				.RuleFor(p => p.EndDate, (f, p) => p.StartDate.AddDays(f.Random.Int(15, 31)).Date.ToUniversalTime())
				.RuleFor(p => p.Status, f => f.PickRandom<PayoutPeriodStatus>());

			// Initialize Bogus for generating fake owner payout data
			_ownerPayoutFaker = new Faker<OwnerPayout>()
				.RuleFor(p => p.Amount, f => f.Random.Decimal(100, 10000))
				.RuleFor(p => p.Currency, f => f.Finance.Currency().Code)
				.RuleFor(p => p.Status, f => f.PickRandom<PayoutStatus>())
				.RuleFor(p => p.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
				.RuleFor(p => p.ProcessedAt, (f, p) => p.Status == PayoutStatus.Completed ? f.Date.Between(p.CreatedAt, DateTime.UtcNow) : null)
				.RuleFor(p => p.TransactionReference, f => f.Random.AlphaNumeric(20))
				.RuleFor(p => p.Notes, f => f.Lorem.Sentence());
		}

		/// <summary>
		/// Seeds the database with a specified number of fake payout periods and owner payouts.
		/// </summary>
		/// <param name="payoutPeriodCount">The number of payout periods to create.</param>
		/// <param name="ownerPayoutCount">The number of owner payouts to create per period.</param>
		public async Task SeedPayoutsAsync(int payoutPeriodCount, int ownerPayoutCount)
		{
			try
			{
				if (_dbContext.PayoutPeriods == null || _dbContext.OwnerPayouts == null)
				{
					throw new InvalidOperationException("PayoutPeriods or OwnerPayouts DbSet is not initialized.");
				}

				var payoutPeriods = new List<PayoutPeriod>();
				var ownerPayouts = new List<OwnerPayout>();

				var owners = await _dbContext.Owners.ToListAsync();
				if (!owners.Any())
				{
					_logger.LogWarning("No owners found in the database. Cannot seed payouts.");
					return;
				}

				for (int i = 0; i < payoutPeriodCount; i++)
				{
					var payoutPeriod = _payoutPeriodFaker.Generate();
					payoutPeriods.Add(payoutPeriod);

					for (int j = 0; j < ownerPayoutCount; j++)
					{
						var ownerPayout = _ownerPayoutFaker.Generate();
						ownerPayout.PayoutPeriodId = payoutPeriod.Id;
						ownerPayout.OwnerId = owners[new Random().Next(owners.Count)].Id;
						ownerPayouts.Add(ownerPayout);
					}
				}

				await _dbContext.PayoutPeriods.AddRangeAsync(payoutPeriods);
				await _dbContext.OwnerPayouts.AddRangeAsync(ownerPayouts);
				await _dbContext.SaveChangesAsync();

				_logger.LogInformation("Successfully seeded {PayoutPeriodCount} payout periods and {OwnerPayoutCount} owner payouts", payoutPeriodCount, ownerPayoutCount * payoutPeriodCount);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while seeding payouts");
				throw;
			}
		}
	}
}