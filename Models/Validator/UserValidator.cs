using System.ComponentModel.DataAnnotations;

namespace Cloud.Models.Validator
{
	/// <summary>
	/// Interface for user validation strategies.
	/// </summary>
	public interface IUserValidationStrategy
	{
		void Validate(UserModel user);
	}

	/// <summary>
	/// Main validator class for users.
	/// </summary>
	public class UserValidator
	{
		private readonly List<IUserValidationStrategy> _validationStrategies = new List<IUserValidationStrategy>();

		/// <summary>
		/// Adds a validation strategy to the validator.
		/// </summary>
		/// <param name="strategy">The strategy to add.</param>
		public void AddStrategy(IUserValidationStrategy strategy)
		{
			_validationStrategies.Add(strategy);
		}

		/// <summary>
		/// Validates a user using all added strategies.
		/// </summary>
		/// <param name="user">The user to validate.</param>
		public void ValidateUser(UserModel user)
		{
			foreach (var strategy in _validationStrategies)
			{
				strategy.Validate(user);
			}
		}
	}

	/// <summary>
	/// Validates that the email of a user is not empty and in correct format.
	/// </summary>
	public class UserEmailValidationStrategy : IUserValidationStrategy
	{
		public void Validate(UserModel user)
		{
			if (string.IsNullOrWhiteSpace(user.Email))
			{
				throw new ArgumentException("Email cannot be empty.", nameof(user.Email));
			}

			if (!new EmailAddressAttribute().IsValid(user.Email))
			{
				throw new ArgumentException("Invalid email format.", nameof(user.Email));
			}
		}
	}

	/// <summary>
	/// Validates that the user's name is not empty.
	/// </summary>
	public class UserNameValidationStrategy : IUserValidationStrategy
	{
		public void Validate(UserModel user)
		{
			if (string.IsNullOrWhiteSpace(user.FirstName))
			{
				throw new ArgumentException("First name cannot be empty.", nameof(user.FirstName));
			}

			if (string.IsNullOrWhiteSpace(user.LastName))
			{
				throw new ArgumentException("Last name cannot be empty.", nameof(user.LastName));
			}
		}
	}

	/// <summary>
	/// Validates that there are no duplicate user emails.
	/// </summary>
	public class UserDuplicateEmailValidationStrategy : IUserValidationStrategy
	{
		private readonly ApplicationDbContext _dbContext;

		public UserDuplicateEmailValidationStrategy(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public void Validate(UserModel user)
		{
			if (_dbContext.Users.Any(u => u.Email == user.Email && u.Id != user.Id))
			{
				throw new ArgumentException("User with this email already exists.", nameof(user.Email));
			}
		}
	}

	/// <summary>
	/// Validates that the user's role is valid.
	/// </summary>
	public class UserRoleValidationStrategy : IUserValidationStrategy
	{
		public void Validate(UserModel user)
		{
			if (!Enum.IsDefined(typeof(UserRole), user.Role))
			{
				throw new ArgumentException("Invalid user role.", nameof(user.Role));
			}
		}
	}

}