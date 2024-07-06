// IActivityLogService.cs
using Cloud.Models;
using Microsoft.EntityFrameworkCore;


namespace Cloud.Services
{
	public interface IActivityLogService
	{
		Task<IEnumerable<ActivityLogModel>> GetUserActivitiesAsync(Guid userId, int page, int size);
		Task<ActivityLogModel> CreateActivityAsync(ActivityLogModel activity);
		Task<IEnumerable<ActivityLogModel>> SearchActivitiesAsync(Guid? userId, string action, DateTime? startDate, DateTime? endDate);
	}

	public class ActivityLogService : IActivityLogService
	{
		private readonly ApplicationDbContext _context;

		public ActivityLogService(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<ActivityLogModel>> GetUserActivitiesAsync(Guid userId, int page, int size)
		{
			return await _context.ActivityLogs
				.Where(a => a.UserId == userId)
				.OrderByDescending(a => a.Timestamp)
				.Skip((page - 1) * size)
				.Take(size)
				.ToListAsync();
		}

		public async Task<ActivityLogModel> CreateActivityAsync(ActivityLogModel activity)
		{
			activity.Timestamp = DateTime.UtcNow;

			await _context.ActivityLogs.AddAsync(activity);
			await _context.SaveChangesAsync();

			return activity;
		}

		public async Task<IEnumerable<ActivityLogModel>> SearchActivitiesAsync(Guid? userId, string action, DateTime? startDate, DateTime? endDate)
		{
			var query = _context.ActivityLogs.AsQueryable();

			if (userId.HasValue)
				query = query.Where(a => a.UserId == userId);

			if (!string.IsNullOrEmpty(action))
				query = query.Where(a => a.Action == action);

			if (startDate.HasValue)
				query = query.Where(a => a.Timestamp >= startDate);

			if (endDate.HasValue)
				query = query.Where(a => a.Timestamp <= endDate);

			return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
		}
	}
}