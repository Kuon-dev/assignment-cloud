using Microsoft.EntityFrameworkCore;
using Cloud.Models;
using Cloud.Models.Validator;
using Cloud.Factories;

namespace Cloud.Services
{
	/// <summary>
	/// Interface for the MaintenanceTaskService.
	/// </summary>
	public interface IMaintenanceTaskService
	{
		Task<MaintenanceTaskModel> CreateTaskAsync(Guid requestId, string description, decimal estimatedCost);
		Task<MaintenanceTaskModel> GetTaskByIdAsync(Guid taskId);
		Task<IEnumerable<MaintenanceTaskModel>> GetTasksByRequestIdAsync(Guid requestId);
		Task<MaintenanceTaskModel> UpdateTaskAsync(Guid taskId, string description, decimal? estimatedCost, decimal? actualCost, DateTime? startDate, DateTime? completionDate, Cloud.Models.TaskStatus status);
		Task DeleteTaskAsync(Guid taskId);
	}

	/// <summary>
	/// Service for handling maintenance task operations.
	/// </summary>
	public class MaintenanceTaskService : IMaintenanceTaskService
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly MaintenanceTaskFactory _taskFactory;
		private readonly MaintenanceTaskValidator _taskValidator;

		/// <summary>
		/// Initializes a new instance of the MaintenanceTaskService class.
		/// </summary>
		/// <param name="dbContext">The database context.</param>
		/// <param name="taskFactory">The factory for creating maintenance tasks.</param>
		/// <param name="taskValidator">The validator for maintenance tasks.</param>
		public MaintenanceTaskService(ApplicationDbContext dbContext, MaintenanceTaskFactory taskFactory, MaintenanceTaskValidator taskValidator)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_taskFactory = taskFactory ?? throw new ArgumentNullException(nameof(taskFactory));
			_taskValidator = taskValidator ?? throw new ArgumentNullException(nameof(taskValidator));
		}

		/// <summary>
		/// Creates a new maintenance task.
		/// </summary>
		/// <param name="requestId">The ID of the associated maintenance request.</param>
		/// <param name="description">The description of the task.</param>
		/// <param name="estimatedCost">The estimated cost of the task.</param>
		/// <returns>The created maintenance task.</returns>
		public async Task<MaintenanceTaskModel> CreateTaskAsync(Guid requestId, string description, decimal estimatedCost)
		{
			try
			{
				var task = await _taskFactory.CreateTaskAsync(requestId, description, estimatedCost, Cloud.Models.TaskStatus.Pending);
				_taskValidator.ValidateTask(task);
				return task;
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Error creating maintenance task.", ex);
			}
		}

		/// <summary>
		/// Retrieves a maintenance task by its ID.
		/// </summary>
		/// <param name="taskId">The ID of the task to retrieve.</param>
		/// <returns>The retrieved maintenance task.</returns>
		public async Task<MaintenanceTaskModel> GetTaskByIdAsync(Guid taskId)
		{
			var task = await _dbContext.MaintenanceTasks.FindAsync(taskId);
			if (task == null)
			{
				throw new KeyNotFoundException($"Maintenance task with ID {taskId} not found.");
			}
			return task;
		}

		/// <summary>
		/// Retrieves all maintenance tasks associated with a specific request.
		/// </summary>
		/// <param name="requestId">The ID of the maintenance request.</param>
		/// <returns>A collection of maintenance tasks.</returns>
		public async Task<IEnumerable<MaintenanceTaskModel>> GetTasksByRequestIdAsync(Guid requestId)
		{
			return await _dbContext.MaintenanceTasks
				.Where(t => t.RequestId == requestId)
				.ToListAsync();
		}

		/// <summary>
		/// Updates an existing maintenance task.
		/// </summary>
		/// <param name="taskId">The ID of the task to update.</param>
		/// <param name="description">The updated description.</param>
		/// <param name="estimatedCost">The updated estimated cost.</param>
		/// <param name="actualCost">The updated actual cost.</param>
		/// <param name="startDate">The updated start date.</param>
		/// <param name="completionDate">The updated completion date.</param>
		/// <param name="status">The updated status.</param>
		/// <returns>The updated maintenance task.</returns>
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

		/// <summary>
		/// Deletes a maintenance task.
		/// </summary>
		/// <param name="taskId">The ID of the task to delete.</param>
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
	}
}