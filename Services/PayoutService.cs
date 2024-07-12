using Microsoft.EntityFrameworkCore;
/*using Microsoft.Extensions.Logging;*/
using Cloud.Models;
using Cloud.Models.DTO;

namespace Cloud.Services
{
	public interface IPayoutService
	{
		Task<PayoutPeriodDto> CreatePayoutPeriodAsync(CreatePayoutPeriodDto dto);
		Task<IEnumerable<PayoutPeriodDto>> GetPayoutPeriodsAsync();
		Task<PayoutPeriodDto?> GetPayoutPeriodAsync(Guid id);
		Task<OwnerPayoutDto> CreateOwnerPayoutAsync(CreateOwnerPayoutDto dto);
		Task<IEnumerable<OwnerPayoutDto>> GetOwnerPayoutsAsync(Guid ownerId);
		Task<OwnerPayoutDto?> GetOwnerPayoutAsync(Guid id);
		Task<OwnerPayoutDto> ProcessOwnerPayoutAsync(Guid payoutId);
		Task<PayoutSettingsDto> GetPayoutSettingsAsync();
		Task<PayoutSettingsDto> UpdatePayoutSettingsAsync(UpdatePayoutSettingsDto dto);
		Task<IEnumerable<OwnerPayoutStatusDto>> GetOwnersPayoutStatusAsync(Guid payoutPeriodId);
		Task<IEnumerable<PaymentDto>> GetOwnerPaymentsAsync(Guid payoutPeriodId, Guid ownerId);
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

		public async Task<IEnumerable<OwnerPayoutStatusDto>> GetOwnersPayoutStatusAsync(Guid payoutPeriodId)
		{
			var payoutPeriod = await _context.PayoutPeriods.FindAsync(payoutPeriodId);
			if (payoutPeriod == null)
				throw new ArgumentException("Payout period not found", nameof(payoutPeriodId));

			var owners = await _context.Owners.ToListAsync();
			var payouts = await _context.OwnerPayouts
				.Where(op => op.PayoutPeriodId == payoutPeriodId)
				.ToListAsync();

			return owners.Select(owner => new OwnerPayoutStatusDto
			{
				OwnerId = owner.Id,
				OwnerName = $"{owner.User?.FirstName} {owner.User?.LastName}",
				HasReceivedPayout = payouts.Any(p => p.OwnerId == owner.Id)
			});
		}

		public async Task<IEnumerable<PaymentDto>> GetOwnerPaymentsAsync(Guid payoutPeriodId, Guid ownerId)
		{
			var payoutPeriod = await _context.PayoutPeriods.FindAsync(payoutPeriodId);
			if (payoutPeriod == null)
				throw new ArgumentException("Payout period not found", nameof(payoutPeriodId));

			var owner = await _context.Owners.FindAsync(ownerId);
			if (owner == null)
				throw new ArgumentException("Owner not found", nameof(ownerId));

			var payments = await _context.RentPayments
				.Include(rp => rp.Tenant)
				.ThenInclude(t => t!.CurrentProperty)
				.Where(rp => rp.Tenant!.CurrentProperty!.OwnerId == ownerId &&
							 rp.CreatedAt >= payoutPeriod.StartDate &&
							 rp.CreatedAt <= payoutPeriod.EndDate &&
							 rp.Status == PaymentStatus.Succeeded)
				.ToListAsync();

			return payments.Select(p => new PaymentDto
			{
				Id = p.Id,
				Amount = p.Amount,
				Currency = p.Currency,
				Status = p.Status.ToString(),
				CreatedAt = p.CreatedAt,
				PropertyId = p.Tenant!.CurrentPropertyId ?? Guid.Empty,
				TenantName = $"{p.Tenant!.User?.FirstName} {p.Tenant.User?.LastName}"
			});
		}

		public async Task<PayoutPeriodDto> CreatePayoutPeriodAsync(CreatePayoutPeriodDto dto)
		{
			var payoutPeriod = new PayoutPeriod
			{
				StartDate = dto.StartDate,
				EndDate = dto.EndDate,
				Status = PayoutPeriodStatus.Pending
			};

			_context.PayoutPeriods.Add(payoutPeriod);
			await _context.SaveChangesAsync();
			return MapToPayoutPeriodDto(payoutPeriod);
		}

		public async Task<IEnumerable<PayoutPeriodDto>> GetPayoutPeriodsAsync()
		{
			var payoutPeriods = await _context.PayoutPeriods.ToListAsync();
			return payoutPeriods.Select(MapToPayoutPeriodDto);
		}

		public async Task<PayoutPeriodDto?> GetPayoutPeriodAsync(Guid id)
		{
			var payoutPeriod = await _context.PayoutPeriods.FindAsync(id);
			return payoutPeriod != null ? MapToPayoutPeriodDto(payoutPeriod) : null;
		}

		public async Task<OwnerPayoutDto> CreateOwnerPayoutAsync(CreateOwnerPayoutDto dto)
		{
			var owner = await _context.Owners.FindAsync(dto.OwnerId);
			if (owner == null)
				throw new ArgumentException("Owner not found", nameof(dto.OwnerId));

			var payoutPeriod = await _context.PayoutPeriods.FindAsync(dto.PayoutPeriodId);
			if (payoutPeriod == null)
				throw new ArgumentException("Payout period not found", nameof(dto.PayoutPeriodId));

			var settings = await GetPayoutSettingsAsync();

			var ownerPayout = new OwnerPayout
			{
				OwnerId = dto.OwnerId,
				PayoutPeriodId = dto.PayoutPeriodId,
				Amount = dto.Amount,
				Currency = settings.DefaultCurrency,
				Status = PayoutStatus.Pending,
				CreatedAt = DateTime.UtcNow
			};

			_context.OwnerPayouts.Add(ownerPayout);
			await _context.SaveChangesAsync();
			return MapToOwnerPayoutDto(ownerPayout);
		}

		public async Task<IEnumerable<OwnerPayoutDto>> GetOwnerPayoutsAsync(Guid ownerId)
		{
			var ownerPayouts = await _context.OwnerPayouts
				.Where(op => op.OwnerId == ownerId)
				.ToListAsync();
			return ownerPayouts.Select(MapToOwnerPayoutDto);
		}

		public async Task<OwnerPayoutDto?> GetOwnerPayoutAsync(Guid id)
		{
			var ownerPayout = await _context.OwnerPayouts.FindAsync(id);
			return ownerPayout != null ? MapToOwnerPayoutDto(ownerPayout) : null;
		}

		public async Task<OwnerPayoutDto> ProcessOwnerPayoutAsync(Guid payoutId)
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

			return MapToOwnerPayoutDto(payout);
		}

		public async Task<PayoutSettingsDto> GetPayoutSettingsAsync()
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
			return MapToPayoutSettingsDto(settings);
		}

		public async Task<PayoutSettingsDto> UpdatePayoutSettingsAsync(UpdatePayoutSettingsDto dto)
		{
			var settings = await _context.PayoutSettings.FirstOrDefaultAsync();
			if (settings == null)
			{
				settings = new PayoutSettings();
				_context.PayoutSettings.Add(settings);
			}

			settings.PayoutCutoffDay = dto.PayoutCutoffDay;
			settings.ProcessingDay = dto.ProcessingDay;
			settings.DefaultCurrency = dto.DefaultCurrency;
			settings.MinimumPayoutAmount = dto.MinimumPayoutAmount;

			await _context.SaveChangesAsync();
			return MapToPayoutSettingsDto(settings);
		}



		private static PayoutPeriodDto MapToPayoutPeriodDto(PayoutPeriod period)
		{
			return new PayoutPeriodDto
			{
				Id = period.Id,
				StartDate = period.StartDate,
				EndDate = period.EndDate,
				Status = period.Status.ToString()
			};
		}

		private static OwnerPayoutDto MapToOwnerPayoutDto(OwnerPayout payout)
		{
			return new OwnerPayoutDto
			{
				Id = payout.Id,
				OwnerId = payout.OwnerId,
				PayoutPeriodId = payout.PayoutPeriodId,
				Amount = payout.Amount,
				Currency = payout.Currency,
				Status = payout.Status.ToString(),
				CreatedAt = payout.CreatedAt,
				ProcessedAt = payout.ProcessedAt,
				TransactionReference = payout.TransactionReference,
				Notes = payout.Notes
			};
		}

		private static PayoutSettingsDto MapToPayoutSettingsDto(PayoutSettings settings)
		{
			return new PayoutSettingsDto
			{
				Id = settings.Id,
				PayoutCutoffDay = settings.PayoutCutoffDay,
				ProcessingDay = settings.ProcessingDay,
				DefaultCurrency = settings.DefaultCurrency,
				MinimumPayoutAmount = settings.MinimumPayoutAmount
			};
		}

	}
}