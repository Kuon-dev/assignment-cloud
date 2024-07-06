namespace Cloud.Models.Validator
{
	public interface IMaintenanceRequestValidationStrategy
	{
		void Validate(MaintenanceRequestModel request);
	}

	public class MaintenanceRequestValidator
	{
		private readonly List<IMaintenanceRequestValidationStrategy> _validationStrategies = new List<IMaintenanceRequestValidationStrategy>();

		public void AddStrategy(IMaintenanceRequestValidationStrategy strategy)
		{
			_validationStrategies.Add(strategy);
		}

		public void ValidateRequest(MaintenanceRequestModel request)
		{
			foreach (var strategy in _validationStrategies)
			{
				strategy.Validate(request);
			}
		}
	}

	public class PropertyIdValidationStrategy : IMaintenanceRequestValidationStrategy
	{
		public void Validate(MaintenanceRequestModel request)
		{
			if (request.PropertyId == Guid.Empty)
			{
				throw new ArgumentException("Property ID cannot be empty.", nameof(request.PropertyId));
			}
		}
	}

	public class DescriptionValidationStrategy : IMaintenanceRequestValidationStrategy
	{
		public void Validate(MaintenanceRequestModel request)
		{
			if (string.IsNullOrWhiteSpace(request.Description))
			{
				throw new ArgumentException("Description cannot be empty.", nameof(request.Description));
			}
		}
	}

	public class DuplicateRequestValidationStrategy : IMaintenanceRequestValidationStrategy
	{
		private readonly ApplicationDbContext _dbContext;

		public DuplicateRequestValidationStrategy(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public void Validate(MaintenanceRequestModel request)
		{
			if (_dbContext.MaintenanceRequests.Any(r => r.TenantId == request.TenantId && r.PropertyId == request.PropertyId && r.Description == request.Description && r.Status == request.Status))
			{
				throw new ArgumentException("Duplicate maintenance request already exists.", nameof(request));
			}
		}
	}
}