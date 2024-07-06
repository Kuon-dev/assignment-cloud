namespace Cloud.Models.Validator
{
	/// <summary>
	/// Interface for maintenance task validation strategies.
	/// </summary>
	public interface IMaintenanceTaskValidationStrategy
	{
		void Validate(MaintenanceTaskModel task);
	}

	/// <summary>
	/// Main validator class for maintenance tasks.
	/// </summary>
	public class MaintenanceTaskValidator
	{
		private readonly List<IMaintenanceTaskValidationStrategy> _validationStrategies = new List<IMaintenanceTaskValidationStrategy>();

		/// <summary>
		/// Adds a validation strategy to the validator.
		/// </summary>
		/// <param name="strategy">The strategy to add.</param>
		public void AddStrategy(IMaintenanceTaskValidationStrategy strategy)
		{
			_validationStrategies.Add(strategy);
		}

		/// <summary>
		/// Validates a maintenance task using all added strategies.
		/// </summary>
		/// <param name="task">The task to validate.</param>
		public void ValidateTask(MaintenanceTaskModel task)
		{
			foreach (var strategy in _validationStrategies)
			{
				strategy.Validate(task);
			}
		}
	}

	/// <summary>
	/// Validates that the RequestId of a maintenance task is not empty.
	/// </summary>
	public class RequestIdValidationStrategy : IMaintenanceTaskValidationStrategy
	{
		public void Validate(MaintenanceTaskModel task)
		{
			if (task.RequestId == Guid.Empty)
			{
				throw new ArgumentException("Request ID cannot be empty.", nameof(task.RequestId));
			}
		}
	}

	/// <summary>
	/// Validates that the EstimatedCost of a maintenance task is not negative.
	/// </summary>
	public class EstimatedCostValidationStrategy : IMaintenanceTaskValidationStrategy
	{
		public void Validate(MaintenanceTaskModel task)
		{
			if (task.EstimatedCost < 0)
			{
				throw new ArgumentException("Estimated cost cannot be negative.", nameof(task.EstimatedCost));
			}
		}
	}

	/// <summary>
	/// Validates that there are no duplicate maintenance tasks.
	/// </summary>
	public class DuplicateTaskValidationStrategy : IMaintenanceTaskValidationStrategy
	{
		private readonly ApplicationDbContext _dbContext;

		public DuplicateTaskValidationStrategy(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public void Validate(MaintenanceTaskModel task)
		{
			if (_dbContext.MaintenanceTasks.Any(t =>
				t.RequestId == task.RequestId &&
				t.Description == task.Description &&
				t.Status == task.Status))
			{
				throw new ArgumentException("Duplicate maintenance task already exists.", nameof(task));
			}
		}
	}

	/// <summary>
	/// Validates that the dates in a maintenance task are logical.
	/// </summary>
	public class DateValidationStrategy : IMaintenanceTaskValidationStrategy
	{
		public void Validate(MaintenanceTaskModel task)
		{
			if (task.StartDate.HasValue && task.CompletionDate.HasValue && task.CompletionDate < task.StartDate)
			{
				throw new ArgumentException("Completion date cannot be earlier than start date.", nameof(task.CompletionDate));
			}
		}
	}
}