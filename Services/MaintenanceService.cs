using Microsoft.EntityFrameworkCore;
using Cloud.Models;
using Cloud.Models.Validator;
using Cloud.Factories;
using Cloud.Models.DTO;

namespace Cloud.Services
{
	/// <summary>
	/// Interface for the combined maintenance service.
	/// </summary>
	public interface IMaintenanceService
	{
		// Maintenance Request methods
		Task<(IEnumerable<MaintenanceRequestModel> Requests, int TotalCount)> GetAllMaintenanceRequestsAsync(int page, int size);
		Task<MaintenanceRequestModel?> GetMaintenanceRequestByIdAsync(Guid id, string userId);
		Task<MaintenanceRequestModel> CreateMaintenanceRequestAsync(CreateMaintenanceRequestDto dto, string userId);
		Task UpdateMaintenanceRequestAsync(Guid id, UpdateMaintenanceRequestDto dto, string userId);
		Task DeleteMaintenanceRequestAsync(Guid id);
		Task<(IEnumerable<MaintenanceRequestModel> Requests, int TotalCount)> GetMaintenanceRequestsByStatusAsync(MaintenanceStatus status, int page, int size);

		// Maintenance Task methods
		Task<MaintenanceTaskModel> CreateTaskAsync(Guid requestId, string description, decimal estimatedCost);
		Task<MaintenanceTaskModel> GetTaskByIdAsync(Guid taskId);
		Task<IEnumerable<MaintenanceTaskModel>> GetTasksByRequestIdAsync(Guid requestId);
		Task<MaintenanceTaskModel> UpdateTaskAsync(Guid taskId, string? description, decimal? estimatedCost, decimal? actualCost, DateTime? startDate, DateTime? completionDate, Cloud.Models.TaskStatus status);
		Task DeleteTaskAsync(Guid taskId);
	}

	/// <summary>
	/// Service for handling both maintenance requests and tasks.
	/// </summary>
	public class MaintenanceService : IMaintenanceService
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly MaintenanceFactory _maintenanceFactory;
		private readonly MaintenanceRequestValidator _requestValidator;
		private readonly MaintenanceTaskValidator _taskValidator;
		private readonly S3Service _s3Service;

		/// <summary>
		/// Initializes a new instance of the MaintenanceService class.
		/// </summary>
		public MaintenanceService(
			ApplicationDbContext dbContext,
			MaintenanceFactory maintenanceFactory,
			MaintenanceRequestValidator requestValidator,
			MaintenanceTaskValidator taskValidator,
			S3Service s3Service)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_maintenanceFactory = maintenanceFactory ?? throw new ArgumentNullException(nameof(maintenanceFactory));
			_requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
			_taskValidator = taskValidator ?? throw new ArgumentNullException(nameof(taskValidator));
			_s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
		}

		#region Maintenance Request Methods

		/// <inheritdoc/>
		public async Task<(IEnumerable<MaintenanceRequestModel> Requests, int TotalCount)> GetAllMaintenanceRequestsAsync(int page, int size)
		{
			var requests = await _dbContext.MaintenanceRequests
				.Skip((page - 1) * size)
				.Take(size)
				.ToListAsync();

			var totalCount = await _dbContext.MaintenanceRequests.CountAsync();

			return (requests, totalCount);
		}

		/// <inheritdoc/>
		public async Task<MaintenanceRequestModel?> GetMaintenanceRequestByIdAsync(Guid id, string userId)
		{
			var request = await _dbContext.MaintenanceRequests
				.Include(r => r.Tenant)
				.FirstOrDefaultAsync(r => r.Id == id);

			if (request == null)
			{
				return null;
			}

			var user = await _dbContext.Users.FindAsync(userId);
			if (user == null)
			{
				throw new InvalidOperationException("User not found.");
			}

			if (request.TenantId.ToString() != userId && user.Role != UserRole.Admin && user.Role != UserRole.Owner)
			{
				throw new UnauthorizedAccessException("You do not have permission to view this maintenance request.");
			}

			return request;
		}

		/// <inheritdoc/>
		public async Task<MaintenanceRequestModel> CreateMaintenanceRequestAsync(CreateMaintenanceRequestDto dto, string userId)
		{
			var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.UserId == userId);
			if (tenant == null)
			{
				throw new InvalidOperationException("User is not a tenant.");
			}

			var property = await _dbContext.Properties.FindAsync(dto.PropertyId);
			if (property == null)
			{
				throw new InvalidOperationException("Property not found.");
			}

			var (request, task) = await _maintenanceFactory.CreateRequestWithTaskAsync(tenant.Id, dto.PropertyId, dto.Description);

			_requestValidator.ValidateRequest(request);
			_taskValidator.ValidateTask(task);

			return request;
		}

		/// <inheritdoc/>
		public async Task UpdateMaintenanceRequestAsync(Guid id, UpdateMaintenanceRequestDto dto, string userId)
		{
			var request = await _dbContext.MaintenanceRequests
				.Include(r => r.Tenant)
				.FirstOrDefaultAsync(r => r.Id == id);

			if (request == null)
			{
				throw new NotFoundException("Maintenance request not found.");
			}

			var user = await _dbContext.Users.FindAsync(userId);
			if (user == null)
			{
				throw new InvalidOperationException("User not found.");
			}

			if (request.TenantId.ToString() != userId && user.Role != UserRole.Admin && user.Role != UserRole.Owner)
			{
				throw new UnauthorizedAccessException("You do not have permission to update this maintenance request.");
			}

			if (!string.IsNullOrEmpty(dto.Description))
			{
				request.Description = dto.Description;
			}

			if (dto.Status.HasValue)
			{
				if (user.Role != UserRole.Admin && user.Role != UserRole.Owner)
				{
					throw new UnauthorizedAccessException("Only admins or owners can update the status of a maintenance request.");
				}
				request.Status = dto.Status.Value;
			}

			request.UpdateModifiedProperties(DateTime.UtcNow);
			await _dbContext.SaveChangesAsync();
		}

		/// <inheritdoc/>
		public async Task DeleteMaintenanceRequestAsync(Guid id)
		{
			var request = await _dbContext.MaintenanceRequests.FindAsync(id);
			if (request == null)
			{
				throw new NotFoundException("Maintenance request not found.");
			}

			_dbContext.MaintenanceRequests.Remove(request);
			await _dbContext.SaveChangesAsync();
		}

		/// <inheritdoc/>
		public async Task<(IEnumerable<MaintenanceRequestModel> Requests, int TotalCount)> GetMaintenanceRequestsByStatusAsync(MaintenanceStatus status, int page, int size)
		{
			var requests = await _dbContext.MaintenanceRequests
				.Where(r => r.Status == status)
				.Skip((page - 1) * size)
				.Take(size)
				.ToListAsync();

			var totalCount = await _dbContext.MaintenanceRequests.CountAsync(r => r.Status == status);

			return (requests, totalCount);
		}

		#endregion

		#region Maintenance Task Methods

		/// <inheritdoc/>
		public async Task<MaintenanceTaskModel> CreateTaskAsync(Guid requestId, string description, decimal estimatedCost)
		{
			try
			{
				var (_, task) = await _maintenanceFactory.CreateRequestWithTaskAsync(Guid.Empty, Guid.Empty, description);
				task.RequestId = requestId;
				task.EstimatedCost = estimatedCost;
				_taskValidator.ValidateTask(task);
				await _dbContext.SaveChangesAsync();
				return task;
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Error creating maintenance task.", ex);
			}
		}

		/// <inheritdoc/>
		public async Task<MaintenanceTaskModel> GetTaskByIdAsync(Guid taskId)
		{
			var task = await _dbContext.MaintenanceTasks.FindAsync(taskId);
			if (task == null)
			{
				throw new KeyNotFoundException($"Maintenance task with ID {taskId} not found.");
			}
			return task;
		}

		/// <inheritdoc/>
		public async Task<IEnumerable<MaintenanceTaskModel>> GetTasksByRequestIdAsync(Guid requestId)
		{
			return await _dbContext.MaintenanceTasks
				.Where(t => t.RequestId == requestId)
				.ToListAsync();
		}

		/// <inheritdoc/>
		public async Task<MaintenanceTaskModel> UpdateTaskAsync(Guid taskId, string? description, decimal? estimatedCost, decimal? actualCost, DateTime? startDate, DateTime? completionDate, Cloud.Models.TaskStatus status)
		{
			var task = await GetTaskByIdAsync(taskId);

			task.Description = description ?? task.Description;
			task.EstimatedCost = estimatedCost ?? task.EstimatedCost;
			task.ActualCost = actualCost ?? task.ActualCost;
			task.StartDate = startDate ?? task.StartDate;
			task.CompletionDate = completionDate ?? task.CompletionDate;
			task.Status = status;

			try
			{
				_taskValidator.ValidateTask(task);
				await _dbContext.SaveChangesAsync();
				return task;
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Error updating maintenance task.", ex);
			}
		}

		/// <inheritdoc/>
		public async Task DeleteTaskAsync(Guid taskId)
		{
			var task = await GetTaskByIdAsync(taskId);
			_dbContext.MaintenanceTasks.Remove(task);

			try
			{
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Error deleting maintenance task.", ex);
			}
		}

		#endregion
	}
}