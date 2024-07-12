using Cloud.Factories;
using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Cloud.Services;

public class DataSeeder
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<UserModel> _userManager;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly UserFactory _userFactory;
	private readonly PropertyFactory _propertyFactory;
	private readonly ListingFactory _listingFactory;
	private readonly RentalApplicationFactory _rentalApplicationFactory;
	private readonly LeaseFactory _leaseFactory;
	private readonly MaintenanceFactory _maintenanceFactory;
	private readonly IMediaService _mediaService;
	private readonly string _testImagePath = Path.Combine(".", "test", "temp");
	private readonly PayoutFactory _payoutSeeder;

	public DataSeeder(
		IServiceProvider serviceProvider,
		ApplicationDbContext dbContext,
		UserManager<UserModel> userManager,
		RoleManager<IdentityRole> roleManager,
		UserFactory userFactory,
		PropertyFactory propertyFactory,
		ListingFactory listingFactory,
		RentalApplicationFactory rentalApplicationFactory,
		LeaseFactory leaseFactory,
		MaintenanceFactory maintenanceFactory,
		IMediaService mediaService,
		PayoutFactory payoutSeeder
		)
	{
		_serviceProvider = serviceProvider;
		_dbContext = dbContext;
		_userManager = userManager;
		_roleManager = roleManager;
		_userFactory = userFactory;
		_propertyFactory = propertyFactory;
		_listingFactory = listingFactory;
		_rentalApplicationFactory = rentalApplicationFactory;
		_leaseFactory = leaseFactory;
		_maintenanceFactory = maintenanceFactory;
		_mediaService = mediaService;
		_payoutSeeder = payoutSeeder;
	}

	public async Task SeedAsync()
	{
		using var scope = _serviceProvider.CreateScope();

		if (_dbContext == null)
		{
			throw new InvalidOperationException("Database context is not initialized.");
		}

		await SeedRolesAsync();
		await SeedUsersAsync();
		await SeedMediaAsync();
		await SeedPropertiesAsync();
		await SeedListingsAsync();
		await SeedRentalApplicationsAsync();
		await SeedLeasesAsync();
		await SeedMaintenanceRequestsAndTasksAsync(); // Add call to seed maintenance requests and tasks
		await SeedPayoutsAsync();
	}

	private async Task SeedRolesAsync()
	{
		var roles = new[] { "Owner", "Tenant", "Admin" };

		foreach (var role in roles)
		{
			if (!await _roleManager.RoleExistsAsync(role))
			{
				await _roleManager.CreateAsync(new IdentityRole(role));
			}
		}
	}

	private async Task SeedUsersAsync()
	{
		if (!await _dbContext.Users.AnyAsync())
		{
			await _userFactory.SeedUsersAsync(50);
		}
	}

	private async Task SeedPropertiesAsync()
	{
		if (_dbContext != null && !await _dbContext.Properties.AnyAsync())
		{
			var owners = await _dbContext.Users
				.AsNoTracking()
				.Include(u => u.Owner)
				.Where(u => u.Role == UserRole.Owner && u.Owner != null)
				.Select(u => u.Owner)
				.Where(o => o != null)
				.ToListAsync();

			Console.WriteLine($"Number of owners found: {owners.Count}");

			if (owners.Any())
			{
				try
				{
					await SeedPropertiesForOwnersAsync(owners, 500);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error in SeedPropertiesForOwnersAsync: {ex.Message}");
					Console.WriteLine($"Stack trace: {ex.StackTrace}");
				}
			}
			else
			{
				Console.WriteLine("No owners found to seed properties.");
			}
		}
		else
		{
			Console.WriteLine("Properties already exist in the database.");
		}
	}

	private async Task SeedPropertiesForOwnersAsync(List<OwnerModel?> owners, int count)
	{
		if (!owners.Any())
		{
			Console.WriteLine("No owners available to seed properties.");
			return;
		}

		var random = new Random();
		for (int i = 0; i < count; i++)
		{
			try
			{
				var owner = owners[random.Next(owners.Count)];
				if (owner == null)
				{
					Console.WriteLine("Selected owner is null.");
					continue;
				}
				Console.WriteLine($"Creating property for owner with ID: {owner.Id}");
				await _propertyFactory.CreateFakePropertyAsync(owner.Id);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error creating property: {ex.Message}");
				Console.WriteLine($"Stack trace: {ex.StackTrace}");
			}
		}
	}

	private async Task SeedListingsAsync()
	{
		if (!await _dbContext.Listings.AnyAsync())
		{
			var propertyIds = await _dbContext.Properties.Select(p => p.Id).ToListAsync();

			if (propertyIds.Count > 0)
			{
				foreach (var propertyId in propertyIds)
				{
					try
					{
						await _listingFactory.CreateFakeListingAsync(propertyId);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error creating listing for property {propertyId}: {ex.Message}");
					}
				}
			}
			else
			{
				Console.WriteLine("No properties found to seed listings.");
			}
		}
	}

	private async Task SeedRentalApplicationsAsync()
	{
		if (!await _dbContext.RentalApplications.AnyAsync())
		{
			await _rentalApplicationFactory.SeedApplicationsAsync(2000);
		}
	}

	private async Task SeedLeasesAsync()
	{
		if (!await _dbContext.Leases.AnyAsync())
		{
			await _leaseFactory.SeedLeasesAsync(1500);
		}
	}

	private async Task SeedMaintenanceRequestsAndTasksAsync()
	{
		if (!await _dbContext.MaintenanceRequests.AnyAsync())
		{
			await _maintenanceFactory.SeedRequestsAndTasksAsync(5000);
		}
	}

	private async Task SeedMediaAsync()
	{
		if (!await _dbContext.Medias.AnyAsync())
		{
			var imageFiles = Directory.GetFiles(_testImagePath, "*.png");
			var users = await _dbContext.Users.ToListAsync();

			if (!users.Any())
			{
				Console.WriteLine("No users found to associate with media. Skipping media seeding.");
				return;
			}

			foreach (var imagePath in imageFiles)
			{
				try
				{
					var fileName = Path.GetFileName(imagePath);
					var user = users[new Random().Next(users.Count)];

					using var stream = new FileStream(imagePath, FileMode.Open);
					var file = new FormFile(stream, 0, stream.Length, null, fileName)
					{
						Headers = new HeaderDictionary(),
						ContentType = "image/png"
					};

					var createMediaDto = new CreateMediaDto
					{
						File = file,
						CustomFileName = null // Use original filename
					};

					await _mediaService.CreateMediaAsync(createMediaDto, user.Id);
					Console.WriteLine($"Seeded media: {fileName}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error seeding media {imagePath}: {ex.Message}");
				}
			}
		}
		else
		{
			Console.WriteLine("Media already exist in the database.");
		}
	}

	private async Task SeedPayoutsAsync()
	{
		if (!await _dbContext.PayoutPeriods.AnyAsync() && !await _dbContext.OwnerPayouts.AnyAsync())
		{
			try
			{
				await _payoutSeeder.SeedPayoutsAsync(12, 5); // Seed 12 payout periods with 5 owner payouts each
				Console.WriteLine("Successfully seeded payouts.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error seeding payouts: {ex.Message}");
				Console.WriteLine($"Stack trace: {ex.StackTrace}");
			}
		}
		else
		{
			Console.WriteLine("Payouts already exist in the database.");
		}
	}
}