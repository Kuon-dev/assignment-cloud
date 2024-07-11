using Microsoft.EntityFrameworkCore;
/*using Microsoft.Extensions.Logging;*/
using Cloud.Models;

namespace Cloud.Services
{
	public interface IPayoutService
	{
		Task<PayoutPeriod> CreatePayoutPeriodAsync(DateTime startDate, DateTime endDate);
		Task<IEnumerable<PayoutPeriod>> GetPayoutPeriodsAsync();
		Task<PayoutPeriod?> GetPayoutPeriodAsync(Guid id);
		Task<OwnerPayout> CreateOwnerPayoutAsync(Guid ownerId, Guid payoutPeriodId, decimal amount);
		Task<IEnumerable<OwnerPayout>> GetOwnerPayoutsAsync(Guid ownerId);
		Task<OwnerPayout?> GetOwnerPayoutAsync(Guid id);
		Task<OwnerPayout> ProcessOwnerPayoutAsync(Guid payoutId);
		Task<PayoutSettings> GetPayoutSettingsAsync();
		Task<PayoutSettings> UpdatePayoutSettingsAsync(PayoutSettings settings);
	}

	public class PayoutService : IPayoutService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<PayoutService> _logger;

		public PayoutService(ApplicationDbContext context, ILogger<PayoutService> logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<PayoutPeriod> CreatePayoutPeriodAsync(DateTime startDate, DateTime endDate)
		{
			var payoutPeriod = new PayoutPeriod
			{
				StartDate = startDate,
				EndDate = endDate,
				Status = PayoutPeriodStatus.Pending
			};

			_context.PayoutPeriods.Add(payoutPeriod);
			await _context.SaveChangesAsync();
			return payoutPeriod;
		}

		public async Task<IEnumerable<PayoutPeriod>> GetPayoutPeriodsAsync()
		{
			return await _context.PayoutPeriods.ToListAsync();
		}

		public async Task<PayoutPeriod?> GetPayoutPeriodAsync(Guid id)
		{
			return await _context.PayoutPeriods.FindAsync(id);
		}

		public async Task<OwnerPayout> CreateOwnerPayoutAsync(Guid ownerId, Guid payoutPeriodId, decimal amount)
		{
			var owner = await _context.Owners.FindAsync(ownerId);
			if (owner == null)
				throw new ArgumentException("Owner not found", nameof(ownerId));

			var payoutPeriod = await _context.PayoutPeriods.FindAsync(payoutPeriodId);
			if (payoutPeriod == null)
				throw new ArgumentException("Payout period not found", nameof(payoutPeriodId));

			var settings = await GetPayoutSettingsAsync();

			var ownerPayout = new OwnerPayout
			{
				OwnerId = ownerId,
				PayoutPeriodId = payoutPeriodId,
				Amount = amount,
				Currency = settings.DefaultCurrency,
				Status = PayoutStatus.Pending,
				CreatedAt = DateTime.UtcNow
			};

			_context.OwnerPayouts.Add(ownerPayout);
			await _context.SaveChangesAsync();
			return ownerPayout;
		}

		public async Task<IEnumerable<OwnerPayout>> GetOwnerPayoutsAsync(Guid ownerId)
		{
			return await _context.OwnerPayouts
				.Where(op => op.OwnerId == ownerId)
				.ToListAsync();
		}

		public async Task<OwnerPayout?> GetOwnerPayoutAsync(Guid id)
		{
			return await _context.OwnerPayouts.FindAsync(id);
		}

		public async Task<OwnerPayout> ProcessOwnerPayoutAsync(Guid payoutId)
		{
			var payout = await _context.OwnerPayouts
				.Include(op => op.Owner)
				.FirstOrDefaultAsync(op => op.Id == payoutId);

			if (payout == null)
				throw new ArgumentException("Payout not found", nameof(payoutId));

			if (payout.Status != PayoutStatus.Pending)
				throw new InvalidOperationException("Payout is not in pending status");

			try
			{
				// Here you would implement the actual bank transfer logic
				// For now, we'll just simulate the process
				payout.Status = PayoutStatus.Processing;
				payout.ProcessedAt = DateTime.UtcNow;
				payout.TransactionReference = Guid.NewGuid().ToString();

				// Simulating a successful transfer
				payout.Status = PayoutStatus.Completed;
				payout.Notes = "Transfer completed successfully";

				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing payout {PayoutId}", payoutId);
				payout.Status = PayoutStatus.Failed;
				payout.Notes = $"Transfer failed: {ex.Message}";
				await _context.SaveChangesAsync();
				throw;
			}

			return payout;
		}

		public async Task<PayoutSettings> GetPayoutSettingsAsync()
		{
			var settings = await _context.PayoutSettings.FirstOrDefaultAsync();
			if (settings == null)
			{
				settings = new PayoutSettings
				{
					PayoutCutoffDay = 1,
					ProcessingDay = 5,
					DefaultCurrency = "USD",
					MinimumPayoutAmount = 100
				};
				_context.PayoutSettings.Add(settings);
				await _context.SaveChangesAsync();
			}
			return settings;
		}

		public async Task<PayoutSettings> UpdatePayoutSettingsAsync(PayoutSettings settings)
		{
			_context.PayoutSettings.Update(settings);
			await _context.SaveChangesAsync();
			return settings;
		}
	}
}