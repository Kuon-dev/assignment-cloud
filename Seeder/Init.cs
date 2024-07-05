using Cloud.Factories;
using Cloud.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

public class DataSeeder {
  private readonly IServiceProvider _serviceProvider;
  private readonly ApplicationDbContext _dbContext;
  private readonly UserManager<UserModel> _userManager;
  private readonly UserFactory _userFactory;
  private readonly PropertyFactory _propertyFactory;
  private readonly ListingFactory _listingFactory;

  public DataSeeder(IServiceProvider serviceProvider, ApplicationDbContext dbContext, UserManager<UserModel> userManager, UserFactory userFactory, PropertyFactory propertyFactory, ListingFactory listingFactory) {
	_serviceProvider = serviceProvider;
	_dbContext = dbContext;
	_userManager = userManager;
	_userFactory = userFactory;
	_propertyFactory = propertyFactory;
	_listingFactory = listingFactory;
  }

  public async Task SeedAsync() {
	using var scope = _serviceProvider.CreateScope();

	if (_dbContext == null) {
	  throw new InvalidOperationException("Database context is not initialized.");
	}

	await SeedUsersAsync();
	await SeedPropertiesAsync();
	await SeedListingsAsync();
  }

  private async Task SeedUsersAsync() {
	if (!await _dbContext.Users.AnyAsync()) {
	  await _userFactory.SeedUsersAsync(50);
	}
  }

  private async Task SeedPropertiesAsync() {
	if (!await _dbContext.Properties?.AnyAsync()) {
	  var owners = await _dbContext.Users.AsNoTracking()
										 .Include(u => u.Owner)
										 .Where(u => u.Role == UserRole.Owner && u.Owner != null)
										 .Select(u => u.Owner)
										 .ToListAsync();

	  if (owners.Count > 0) {
		await SeedPropertiesForOwnersAsync(owners, 50);
	  }
	  else {
		Console.WriteLine("No owners found to seed properties.");
	  }
	}
  }

  private async Task SeedPropertiesForOwnersAsync(List<OwnerModel> owners, int count) {
	var random = new Random();
	for (int i = 0; i < count; i++) {
	  try {
		var owner = owners[random.Next(owners.Count)];
		await _propertyFactory.CreateFakePropertyAsync(owner.Id);
	  }
	  catch (Exception ex) {
		Console.WriteLine($"Error creating property: {ex.Message}");
	  }
	}
  }

  private async Task SeedListingsAsync() {
	if (!await _dbContext.Listings.AnyAsync()) {
	  var propertyIds = await _dbContext.Properties.Select(p => p.Id).ToListAsync();

	  if (propertyIds.Count > 0) {
		foreach (var propertyId in propertyIds) {
		  try {
			await _listingFactory.CreateFakeListingAsync(propertyId);
		  }
		  catch (Exception ex) {
			Console.WriteLine($"Error creating listing for property {propertyId}: {ex.Message}");
		  }
		}
	  }
	  else {
		Console.WriteLine("No properties found to seed listings.");
	  }
	}
  }
}
