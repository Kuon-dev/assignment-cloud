using Cloud.Factories;
using Cloud.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

	public DataSeeder(
		IServiceProvider serviceProvider,
		ApplicationDbContext dbContext,
		UserManager<UserModel> userManager,
		RoleManager<IdentityRole> roleManager,
		UserFactory userFactory,
		PropertyFactory propertyFactory,
		ListingFactory listingFactory,
		RentalApplicationFactory rentalApplicationFactory
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
		await SeedPropertiesAsync();
		await SeedListingsAsync();
		await SeedRentalApplicationsAsync();
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
				.Where(o => o != null) // Filter out null owners here
				.ToListAsync();

			if (owners.Count > 0)
			{
				// Now 'owners' is of type List<OwnerModel> (non-nullable)
				await SeedPropertiesForOwnersAsync(owners, 50);
			}
			else
			{
				Console.WriteLine("No owners found to seed properties.");
			}
		}
	}

	private async Task SeedPropertiesForOwnersAsync(List<OwnerModel?> owners, int count)
	{
		var random = new Random();
		for (int i = 0; i < count; i++)
		{
			try
			{
				var owner = owners[random.Next(owners.Count)];
				if (owner == null)
				{
					Console.WriteLine("Owner is null.");
					continue;
				}
				await _propertyFactory.CreateFakePropertyAsync(owner.Id);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error creating property: {ex.Message}");
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
			await _rentalApplicationFactory.SeedApplicationsAsync(50);
		}
	}

}