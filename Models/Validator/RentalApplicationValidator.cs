using Cloud.Models.DTO;

namespace Cloud.Models.Validator
{
	/// <summary>
	/// Interface for Rental Application validation strategies
	/// </summary>
	public interface IRentalApplicationValidationStrategy
	{
		void Validate(RentalApplicationModel application, Dictionary<string, List<string>> errors);
	}

	/// <summary>
	/// Extension methods for IRentalApplicationValidationStrategy
	/// </summary>
	public static class RentalApplicationValidationStrategyExtensions
	{
		/// <summary>
		/// Adds an error to the errors dictionary
		/// </summary>
		/// <param name="strategy">The validation strategy</param>
		/// <param name="errors">The errors dictionary</param>
		/// <param name="key">The key for the error (usually property name)</param>
		/// <param name="error">The error message</param>
		public static void AddError(this IRentalApplicationValidationStrategy strategy, Dictionary<string, List<string>> errors, string key, string error)
		{
			if (!errors.ContainsKey(key))
			{
				errors[key] = new List<string>();
			}
			errors[key].Add(error);
		}
	}

	/// <summary>
	/// Validator for RentalApplicationModel using strategy pattern
	/// </summary>
	public class RentalApplicationValidator
	{
		private readonly List<IRentalApplicationValidationStrategy> _validationStrategies = new List<IRentalApplicationValidationStrategy>();

		/// <summary>
		/// Initializes a new instance of the RentalApplicationValidator class
		/// </summary>
		public RentalApplicationValidator()
		{
			AddStrategy(new RentalTenantIdValidationStrategy());
			AddStrategy(new ListingIdValidationStrategy());
			AddStrategy(new ApplicationDateValidationStrategy());
			AddStrategy(new RentalStatusValidationStrategy());
			// Add more strategies as needed
		}

		/// <summary>
		/// Adds a new validation strategy
		/// </summary>
		/// <param name="strategy">The strategy to add</param>
		public void AddStrategy(IRentalApplicationValidationStrategy strategy)
		{
			_validationStrategies.Add(strategy);
		}

		/// <summary>
		/// Validates the RentalApplicationModel
		/// </summary>
		/// <param name="application">The application to validate</param>
		public void ValidateApplication(RentalApplicationModel application)
		{
			var errors = new Dictionary<string, List<string>>();

			foreach (var strategy in _validationStrategies)
			{
				strategy.Validate(application, errors);
			}

			if (errors.Any())
			{
				throw new ValidationException(errors);
			}
		}

		/// <summary>
		/// Validates the CreateRentalApplicationDto
		/// </summary>
		/// <param name="applicationDto">The DTO to validate</param>
		public void ValidateCreateDto(CreateRentalApplicationDto applicationDto)
		{
			var errors = new Dictionary<string, List<string>>();

			if (applicationDto.TenantId == Guid.Empty)
			{
				errors.Add("TenantId", new List<string> { "Tenant ID cannot be empty." });
			}

			if (applicationDto.ListingId == Guid.Empty)
			{
				errors.Add("ListingId", new List<string> { "Listing ID cannot be empty." });
			}

			// Add more DTO-specific validations as needed

			if (errors.Any())
			{
				throw new ValidationException(errors);
			}
		}
	}

	/// <summary>
	/// Validates that the TenantId is not empty
	/// </summary>
	public class RentalTenantIdValidationStrategy : IRentalApplicationValidationStrategy
	{
		public void Validate(RentalApplicationModel application, Dictionary<string, List<string>> errors)
		{
			if (application.TenantId == Guid.Empty)
			{
				this.AddError(errors, "TenantId", "Tenant ID cannot be empty.");
			}
		}
	}

	/// <summary>
	/// Validates that the ListingId is not empty
	/// </summary>
	public class ListingIdValidationStrategy : IRentalApplicationValidationStrategy
	{
		public void Validate(RentalApplicationModel application, Dictionary<string, List<string>> errors)
		{
			if (application.ListingId == Guid.Empty)
			{
				this.AddError(errors, "ListingId", "Listing ID cannot be empty.");
			}
		}
	}

	/// <summary>
	/// Validates that the ApplicationDate is set
	/// </summary>
	public class ApplicationDateValidationStrategy : IRentalApplicationValidationStrategy
	{
		public void Validate(RentalApplicationModel application, Dictionary<string, List<string>> errors)
		{
			if (application.ApplicationDate == default)
			{
				this.AddError(errors, "ApplicationDate", "Application date must be set.");
			}
		}
	}

	/// <summary>
	/// Validates that the Status is valid
	/// </summary>
	public class RentalStatusValidationStrategy : IRentalApplicationValidationStrategy
	{
		public void Validate(RentalApplicationModel application, Dictionary<string, List<string>> errors)
		{
			if (!Enum.IsDefined(typeof(ApplicationStatus), application.Status))
			{
				this.AddError(errors, "Status", "Invalid application status.");
			}
		}
	}

	/// <summary>
	/// Custom exception for validation errors
	/// </summary>
	public class ValidationException : Exception
	{
		public Dictionary<string, List<string>> Errors { get; }

		public ValidationException(Dictionary<string, List<string>> errors)
			: base("Validation failed. See Errors property for details.")
		{
			Errors = errors;
		}
	}
}