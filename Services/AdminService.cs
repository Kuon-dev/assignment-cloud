using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Services
{
	public interface IAdminService
	{
		Task<CustomPaginatedResult<UserModel>> GetUsersAsync(PaginationParams paginationParams);
		Task<PerformanceAnalytics> GetPerformanceAnalyticsAsync();
		Task<IEnumerable<ListingAnalytics>> GetListingAnalyticsAsync();
		Task<object> GetFinancialReconciliationDataAsync();
		Task<CustomPaginatedResult<MaintenanceRequestModel>> GetMaintenanceRequestAsync(PaginationParams paginationParams);
		Task<CustomPaginatedResult<PropertyModel>> GetPropertiesAsync(PaginationParams paginationParams);
		Task<bool> UpdateMaintenanceRequestStatusAsync(Guid id, string action);
		// Task<bool> UpdatePropertyStatusAsync(Guid id, string status);
		Task<CustomPaginatedResult<ActivityLogModel>> GetActivityLogsAsync(PaginationParams paginationParams);
	}
}

namespace Cloud.Services
{

	public class AdminService : IAdminService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<AdminService> _logger;

		public AdminService(ApplicationDbContext context, ILogger<AdminService> logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<CustomPaginatedResult<UserModel>> GetUsersAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.Users.AsNoTracking().Where(o => !o.IsDeleted);
			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.ToListAsync();

			return new CustomPaginatedResult<UserModel>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}

		public async Task<PerformanceAnalytics> GetPerformanceAnalyticsAsync()
		{
			var totalListings = await _context.Listings.CountAsync();
			var averagePrice = await _context.Listings.AverageAsync(l => l.Price);
			var totalApplications = await _context.RentalApplications.CountAsync();

			return new PerformanceAnalytics
			{
				TotalListings = totalListings,
				AveragePrice = averagePrice,
				TotalApplications = totalApplications
			};
		}

		public async Task<IEnumerable<ListingAnalytics>> GetListingAnalyticsAsync()
		{
			return await _context.Listings
				.Select(l => new ListingAnalytics
				{
					ListingId = l.Id,
					Views = l.Views,
					Applications = l.Applications.Count,
					LastUpdated = l.UpdatedAt ?? DateTime.MinValue
				})
				.ToListAsync();
		}

		public async Task<object> GetFinancialReconciliationDataAsync()
		{
			var rentPayments = await _context.RentPayments
				.Select(rp => new
				{
					rp.TenantId,
					rp.Amount,
					rp.Currency,
					rp.PaymentIntentId,
					Status = rp.Status.ToString()
				})
				.ToListAsync();

			var ownerPayments = await _context.OwnerPayments
				.Select(op => new
				{
					op.OwnerId,
					op.PropertyId,
					op.Amount,
					op.PaymentDate,
					op.AdminFee,
					op.UtilityFees,
					op.MaintenanceCost,
					op.StripePaymentIntentId,
					Status = op.Status.ToString()
				})
				.ToListAsync();

			return new
			{
				RentPayments = rentPayments,
				OwnerPayments = ownerPayments
			};
		}

		public async Task<CustomPaginatedResult<MaintenanceRequestModel>> GetMaintenanceRequestAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.MaintenanceRequests.AsNoTracking().Where(o => !o.IsDeleted);
			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.ToListAsync();

			return new CustomPaginatedResult<MaintenanceRequestModel>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}

		public async Task<bool> UpdateMaintenanceRequestStatusAsync(Guid id, string action)
		{
			var request = await _context.MaintenanceRequests.FindAsync(id);
			if (request == null)
			{
				return false;
			}

			if (action.Equals("approve", StringComparison.OrdinalIgnoreCase))
			{
				request.Status = MaintenanceStatus.Completed;
			}
			else if (action.Equals("reject", StringComparison.OrdinalIgnoreCase))
			{
				request.Status = MaintenanceStatus.Cancelled;
			}
			else
			{
				throw new ArgumentException("Invalid action. Must be 'approve' or 'reject'.");
			}

			_context.MaintenanceRequests.Update(request);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<CustomPaginatedResult<PropertyModel>> GetPropertiesAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.Properties.AsNoTracking().Where(o => !o.IsDeleted);
			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.ToListAsync();

			return new CustomPaginatedResult<PropertyModel>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}

		// public async Task<bool> UpdatePropertyStatusAsync(Guid id, string status)
		// {
		//     var property = await _context.Properties.FindAsync(id);
		//     if (property == null)
		//     {
		//         return false;
		//     }

		//     property.Status = status;
		//     _context.Properties.Update(property);
		//     await _context.SaveChangesAsync();
		//     return true;
		// }

		public async Task<CustomPaginatedResult<ActivityLogModel>> GetActivityLogsAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.ActivityLogs.AsNoTracking().Where(o => !o.IsDeleted);
			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.ToListAsync();

			return new CustomPaginatedResult<ActivityLogModel>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}
	}
}