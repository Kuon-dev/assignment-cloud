using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Cloud.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class DataSeeder {
  private readonly IServiceProvider _serviceProvider;

  public DataSeeder(IServiceProvider serviceProvider) {
	_serviceProvider = serviceProvider;
  }

  public async Task SeedAsync() {
	using (var scope = _serviceProvider.CreateScope()) {
	  var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
	  var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	  var userFactory = new UserFactory(userManager, dbContext);
	  var propertyFactory = new PropertyFactory(dbContext);

	  // Seed Users if table is empty
	  if (!await dbContext.Users.AnyAsync()) {
		await SeedUsersAsync(userFactory, 50);
	  }

	  // Seed Properties if table is empty
	  if (!await dbContext.Properties.AnyAsync()) {
		var owners = await dbContext.Users.Include(u => u.Owner)
										  .Where(u => u.Role == UserRole.Owner)
										  .Select(u => u.Owner)
										  .Where(o => o != null)
										  .ToListAsync();

		if (owners.Count > 0) { // Ensure there are owners before seeding properties
		  await SeedPropertiesAsync(propertyFactory, owners!, 50);
		}
		else {
		  Console.WriteLine("No owners found to seed properties.");
		}
	  }

	  // Add more seeding methods for other models here
	}
  }

  private async Task SeedUsersAsync(UserFactory userFactory, int count) {
	for (int i = 0; i < count; i++) {
	  try {
		await userFactory.CreateFakeUserAsync();
	  }
	  catch (Exception ex) {
		Console.WriteLine($"Error creating user: {ex.Message}");
	  }
	}
  }

  private async Task SeedPropertiesAsync(PropertyFactory propertyFactory, List<OwnerModel> owners, int count) {
	var random = new Random();
	for (int i = 0; i < count; i++) {
	  try {
		var owner = owners[random.Next(owners.Count)];
		await propertyFactory.CreateFakePropertyAsync(owner.Id);
	  }
	  catch (Exception ex) {
		Console.WriteLine($"Error creating property: {ex.Message}");
	  }
	}
  }
}
