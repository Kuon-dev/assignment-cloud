using Microsoft.AspNetCore.Identity;
using Cloud.Models;
using Bogus;
using Cloud.Models.Validator;
using Stripe;
/*using Microsoft.Extensions.Logging;*/
using System.Transactions;

/// <summary>
/// Factory class for creating user models and related entities.
/// </summary>
public class UserFactory
{
	private readonly UserManager<UserModel> _userManager;
	private readonly ApplicationDbContext _dbContext;
	private readonly Faker<UserModel> _userFaker;
	private readonly Faker<OwnerModel> _ownerFaker;
	private readonly Randomizer _randomizer;
	private readonly StripeCustomerValidator _stripeCustomerValidator;
	private readonly UserValidator _userValidator;
	private readonly ILogger<UserFactory> _logger;

	/// <summary>
	/// Initializes a new instance of the UserFactory class.
	/// </summary>
	/// <param name="userManager">The UserManager instance for managing user operations.</param>
	/// <param name="dbContext">The database context for entity operations.</param>
	/// <param name="stripeCustomerValidator">The validator for Stripe customers.</param>
	/// <param name="logger">The logger for logging operations.</param>
	public UserFactory(
		UserManager<UserModel> userManager,
		ApplicationDbContext dbContext,
		StripeCustomerValidator stripeCustomerValidator,
		ILogger<UserFactory> logger)
	{
		_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		_stripeCustomerValidator = stripeCustomerValidator ?? throw new ArgumentNullException(nameof(stripeCustomerValidator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));

		// Initialize Bogus for generating fake user data
		_userFaker = new Faker<UserModel>()
			.RuleFor(u => u.UserName, f => f.Internet.Email())
			.RuleFor(u => u.Email, (f, u) => u.UserName)
			.RuleFor(u => u.FirstName, f => f.Name.FirstName())
			.RuleFor(u => u.LastName, f => f.Name.LastName())
			.RuleFor(u => u.Role, f => f.PickRandom<UserRole>());

		// Initialize Bogus for generating fake owner data
		_ownerFaker = new Faker<OwnerModel>()
			.RuleFor(o => o.BusinessName, f => f.Company.CompanyName())
			.RuleFor(o => o.BusinessAddress, f => f.Address.FullAddress())
			.RuleFor(o => o.BusinessPhone, f => f.Phone.PhoneNumber())
			.RuleFor(o => o.BusinessEmail, f => f.Internet.Email())
			.RuleFor(o => o.BankAccountNumber, f => f.Finance.Account())
			.RuleFor(o => o.BankAccountName, (f, o) => f.Company.CompanyName())
			.RuleFor(o => o.SwiftBicCode, f => f.Finance.Bic())
			.RuleFor(o => o.BankName, f => f.Company.CompanyName() + " Bank")
			.RuleFor(o => o.VerificationStatus, f => f.PickRandom<OwnerVerificationStatus>())
			.RuleFor(o => o.VerificationDate, f => f.Date.Past());

		// Initialize Randomizer
		_randomizer = new Randomizer();

		// Initialize UserValidator
		_userValidator = new UserValidator();
		_userValidator.AddStrategy(new UserEmailValidationStrategy());
		_userValidator.AddStrategy(new UserNameValidationStrategy());
		_userValidator.AddStrategy(new UserDuplicateEmailValidationStrategy(dbContext));
		_userValidator.AddStrategy(new UserRoleValidationStrategy());
	}

	/// <summary>
	/// Creates a fake user with random data.
	/// </summary>
	/// <returns>The created UserModel.</returns>
	public async Task<UserModel> CreateFakeUserAsync(UserRole? specificRole = null)
	{
		var user = _userFaker.Generate();
		user.Role = specificRole ?? _randomizer.ArrayElement(Enum.GetValues<UserRole>());

		var result = await _userManager.CreateAsync(user, "Password123!");
		if (result.Succeeded)
		{
			await _userManager.AddToRoleAsync(user, user.Role.ToString());
			await CreateRoleSpecificModelAsync(user, true);
			return user;
		}

		var errorMessage = $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}";
		_logger.LogError(errorMessage);
		throw new InvalidOperationException(errorMessage);
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
	public async Task<UserModel> CreateUserAsync(string email, string password, string firstName, string lastName, UserRole role)
	{
		using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
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

			_userValidator.ValidateUser(user);

			var result = await _userManager.CreateAsync(user, password);
			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				_logger.LogError("Failed to create user: {Errors}", errors);
				throw new InvalidOperationException($"Failed to create user: {errors}");
			}

			var stripeCustomer = await CreateStripeCustomerAsync(user);
			await _userManager.AddToRoleAsync(user, role.ToString());
			await CreateRoleSpecificModelAsync(user, false);
			await CreateStripeCustomerModelAsync(user, stripeCustomer.Id);

			scope.Complete();
			return user;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to create user");
			throw;
		}
	}

	/// <summary>
	/// Creates a role-specific model for the user.
	/// </summary>
	/// <param name="user">The user model.</param>
	/// <param name="isFakeData">Indicates whether to generate fake data for the role-specific model.</param>
	private async Task CreateRoleSpecificModelAsync(UserModel user, bool isFakeData)
	{
		switch (user.Role)
		{
			case UserRole.Tenant:
				await _dbContext.Tenants.AddAsync(new TenantModel { UserId = user.Id, User = user });
				break;
			case UserRole.Owner:
				OwnerModel owner;
				if (isFakeData)
				{
					owner = _ownerFaker.Generate();
					owner.UserId = user.Id;
					owner.User = user;
				}
				else
				{
					owner = new OwnerModel
					{
						UserId = user.Id,
						User = user,
						BusinessName = $"{user.FirstName} {user.LastName}'s Business",
						BusinessAddress = "To be updated",
						BusinessPhone = "To be updated",
						BusinessEmail = user.Email,
						VerificationStatus = OwnerVerificationStatus.Pending
					};
				}
				await _dbContext.Owners.AddAsync(owner);
				break;
			case UserRole.Admin:
				await _dbContext.Admins.AddAsync(new AdminModel { UserId = user.Id, User = user });
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(user.Role), user.Role, "Invalid user role");
		}

		await _dbContext.SaveChangesAsync();
	}

	private async Task<Customer> CreateStripeCustomerAsync(UserModel user)
	{
		var options = new CustomerCreateOptions
		{
			Name = $"{user.FirstName} {user.LastName}",
			Email = user.Email,
		};
		var service = new CustomerService();
		return await service.CreateAsync(options);
	}

	private async Task CreateStripeCustomerModelAsync(UserModel user, string stripeCustomerId)
	{
		var customerStripe = new StripeCustomerModel
		{
			UserId = user.Id,
			StripeCustomerId = stripeCustomerId,
			IsVerified = false,
			Balance = 0,
			Currency = "myr",
			Created = DateTime.UtcNow,
			AccountType = "individual"
		};

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

		// Create static users for testing
		await CreateUserAsync("tenant@example.com", "Password123!", "Test", "Tenant", UserRole.Tenant);
		await CreateUserAsync("owner@example.com", "Password123!", "Test", "Owner", UserRole.Owner);
		await CreateUserAsync("admin@example.com", "Password123!", "Test", "Admin", UserRole.Admin);

		for (int i = 0; i < count; i++)
		{
			var user = await CreateFakeUserAsync();
			users.Add(user);
		}

		_logger.LogInformation("Seeded {Count} users", count + 3);
	}
}