using Microsoft.AspNetCore.Identity;
using Cloud.Models;
using Bogus;
using Cloud.Models.Validator;
using Stripe;
using static System.Runtime.InteropServices.JavaScript.JSType;

/// <summary>
/// Factory class for creating user models and related entities.
/// </summary>
public class UserFactory
{
	private readonly UserManager<UserModel> _userManager;
	private readonly ApplicationDbContext _dbContext;
	private readonly Faker<UserModel> _userFaker;
	private readonly Randomizer _randomizer;
	private readonly StripeCustomerValidator _stripeCustomerValidator;
	private readonly UserValidator _userValidator;

	/// <summary>
	/// Initializes a new instance of the UserFactory class.
	/// </summary>
	/// <param name="userManager">The UserManager instance for managing user operations.</param>
	/// <param name="dbContext">The database context for entity operations.</param>
	/// <param name="stripeCustomerValidator">The validator for Stripe customers.</param>
	public UserFactory(UserManager<UserModel> userManager, ApplicationDbContext dbContext, StripeCustomerValidator stripeCustomerValidator)
	{
		_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		_stripeCustomerValidator = stripeCustomerValidator ?? throw new ArgumentNullException(nameof(stripeCustomerValidator));

		// Initialize Bogus for generating fake user data
		_userFaker = new Faker<UserModel>()
			.RuleFor(u => u.UserName, f => f.Internet.Email())
			.RuleFor(u => u.Email, (f, u) => u.UserName)
			.RuleFor(u => u.FirstName, f => f.Name.FirstName())
			.RuleFor(u => u.LastName, f => f.Name.LastName())
			.RuleFor(u => u.Role, f => f.PickRandom<UserRole>());

		// Initialize Randomizer
		_randomizer = new Randomizer();

		// Initialize UserValidator
		_userValidator = new UserValidator();
		_userValidator.AddStrategy(new UserEmailValidationStrategy());
		_userValidator.AddStrategy(new UserNameValidationStrategy());
		_userValidator.AddStrategy(new UserDuplicateEmailValidationStrategy(dbContext));
		_userValidator.AddStrategy(new UserRoleValidationStrategy());
		/*_userValidator.AddStrategy(new UserPasswordValidationStrategy(userManager));*/
	}

	/// <summary>
	/// Creates a fake user with random data.
	/// </summary>
	/// <returns>The created UserModel.</returns>
	public async Task<UserModel> CreateFakeUserAsync()
	{
		var user = _userFaker.Generate();
		var roles = Enum.GetValues<UserRole>();
		user.Role = _randomizer.ArrayElement(roles);

		var result = await _userManager.CreateAsync(user, "Password123!");
		if (result.Succeeded)
		{
			await CreateRoleSpecificModelAsync(user);
			return user;
		}

		throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
	}

	/// <summary>
	/// Creates a tenant user.
	/// </summary>
	/// <param name="email">The email address of the user.</param>
	/// <param name="password">The password for the user.</param>
	/// <param name="firstName">The first name of the user.</param>
	/// <param name="lastName">The last name of the user.</param>
	/// <returns>The created UserModel or null if creation failed.</returns>
	/// <exception cref="InvalidOperationException">Thrown when user creation fails.</exception>
	public async Task<UserModel> CreateTenantAsync(string email, string password, string firstName, string lastName)
	{
		var user = await CreateUserAsync(email, password, firstName, lastName, UserRole.Tenant);
		if (user == null)
		{
			throw new InvalidOperationException("Failed to create tenant user.");
		}
		return user;
	}

	/// <summary>
	/// Creates an owner user.
	/// </summary>
	/// <param name="email">The email address of the user.</param>
	/// <param name="password">The password for the user.</param>
	/// <param name="firstName">The first name of the user.</param>
	/// <param name="lastName">The last name of the user.</param>
	/// <returns>The created UserModel or null if creation failed.</returns>
	/// <exception cref="InvalidOperationException">Thrown when user creation fails.</exception>
	public async Task<UserModel> CreateOwnerAsync(string email, string password, string firstName, string lastName)
	{
		var user = await CreateUserAsync(email, password, firstName, lastName, UserRole.Owner);
		if (user == null)
		{
			throw new InvalidOperationException("Failed to create owner user.");
		}
		return user;
	}

	/// <summary>
	/// Creates an admin user.
	/// </summary>
	/// <param name="email">The email address of the user.</param>
	/// <param name="password">The password for the user.</param>
	/// <param name="firstName">The first name of the user.</param>
	/// <param name="lastName">The last name of the user.</param>
	/// <returns>The created UserModel or null if creation failed.</returns>
	/// <exception cref="InvalidOperationException">Thrown when user creation fails.</exception>
	public async Task<UserModel> CreateAdminAsync(string email, string password, string firstName, string lastName)
	{
		var user = await CreateUserAsync(email, password, firstName, lastName, UserRole.Admin);
		if (user == null)
		{
			throw new InvalidOperationException("Failed to create admin user.");
		}
		return user;
	}

	/// <summary>
	/// Creates a user with the specified role.
	/// </summary>
	/// <param name="email">The email address of the user.</param>
	/// <param name="password">The password for the user.</param>
	/// <param name="firstName">The first name of the user.</param>
	/// <param name="lastName">The last name of the user.</param>
	/// <param name="role">The role of the user.</param>
	/// <returns>The created UserModel.</returns>
	private async Task<UserModel?> CreateUserAsync(string email, string password, string firstName, string lastName, UserRole role)
	{
		// start transaction
		using var transaction = await _dbContext.Database.BeginTransactionAsync();

		try
		{
			var user = new UserModel
			{
				UserName = email,
				Email = email,
				FirstName = firstName,
				LastName = lastName,
				Role = role
			};

			var result = await _userManager.CreateAsync(user, password);
			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				Console.WriteLine($"Failed to create user: {errors}"); // Log errors for debugging
				throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
			}

			// stripe api generate stripe customer id
			var options = new CustomerCreateOptions
			{
				Name = $"{user.FirstName} {user.LastName}",
				Email = user.Email,
			};
			var service = new CustomerService();
			var stripeCustomer = await service.CreateAsync(options);

			await _userManager.AddToRoleAsync(user, role.ToString());
			await CreateRoleSpecificModelAsync(user);
			await CreateStripeCustomerAsync(user, stripeCustomer.Id);

			await transaction.CommitAsync();
			return user;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to create user: {ex}"); // Log errors for debugging
			await transaction.RollbackAsync();
			return null;
		}
	}

	/// <summary>
	/// Creates a role-specific model for the user.
	/// </summary>
	/// <param name="user">The user model.</param>
	private async Task CreateRoleSpecificModelAsync(UserModel user)
	{
		switch (user.Role)
		{
			case UserRole.Tenant:
				await _dbContext.Tenants.AddAsync(new TenantModel { UserId = user.Id, User = user });
				break;
			case UserRole.Owner:
				await _dbContext.Owners.AddAsync(new OwnerModel { UserId = user.Id, User = user });
				break;
			case UserRole.Admin:
				await _dbContext.Admins.AddAsync(new AdminModel { UserId = user.Id, User = user });
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(user.Role), user.Role, "Invalid user role");
		}

		await _dbContext.SaveChangesAsync();
	}

	private async Task CreateStripeCustomerAsync(UserModel user, string stripeCustomerId)
	{
		if (_dbContext.StripeCustomers == null)
		{
			throw new InvalidOperationException("StripeCustomers DbSet is not initialized.");
		}

		var customerStripe = new StripeCustomerModel
		{
			UserId = user.Id,
			StripeCustomerId = stripeCustomerId,
			IsVerified = false,
			Balance = 0,
			Currency = "myr", // Default currency, change if needed
			Created = DateTime.UtcNow,
			AccountType = "individual" // Default account type, change if needed
		};

		_stripeCustomerValidator.AddStrategy(new UserIdValidationStrategy());
		_stripeCustomerValidator.AddStrategy(new StripeCustomerIdValidationStrategy());
		_stripeCustomerValidator.AddStrategy(new DuplicateStripeCustomerValidationStrategy(_dbContext));
		_stripeCustomerValidator.ValidateStripeCustomer(customerStripe);

		_dbContext.StripeCustomers.Add(customerStripe);
		await _dbContext.SaveChangesAsync();
	}

	/// <summary>
	/// Seeds the database with a specified number of fake users.
	/// </summary>
	/// <param name="count">The number of users to create.</param>
	public async Task SeedUsersAsync(int count)
	{
		var users = new List<UserModel>(count);
		var roleSpecificModels = new List<object>(count);

		// static users for testing
		await CreateTenantAsync("tenant@example.com", "Password123!", "Test111", "Tenant");
		await CreateOwnerAsync("owner@example.com", "Password123!", "Test11", "Owner");
		await CreateAdminAsync("admin@example.com", "Password123!", "Test23", "Admin");
		for (int i = 0; i < count; i++)
		{
			var user = _userFaker.Generate();
			var roles = Enum.GetValues<UserRole>();
			user.Role = _randomizer.ArrayElement(roles);
			users.Add(user);

			switch (user.Role)
			{
				case UserRole.Tenant:
					roleSpecificModels.Add(new TenantModel { UserId = user.Id, User = user });
					break;
				case UserRole.Owner:
					roleSpecificModels.Add(new OwnerModel { UserId = user.Id, User = user });
					break;
				case UserRole.Admin:
					roleSpecificModels.Add(new AdminModel { UserId = user.Id, User = user });
					break;
			}
		}

		await _dbContext.Users.AddRangeAsync(users);
		await _dbContext.AddRangeAsync(roleSpecificModels);
		await _dbContext.SaveChangesAsync();

		foreach (var user in users)
		{
			await _userManager.AddPasswordAsync(user, "Password123!");
			await _userManager.AddToRoleAsync(user, user.Role.ToString());
		}
	}
}