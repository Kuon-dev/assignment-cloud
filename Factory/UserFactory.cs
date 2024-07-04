using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Cloud.Models;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

public class UserFactory {
    private readonly UserManager<UserModel> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly Faker<UserModel> _userFaker;
    private readonly Randomizer _randomizer;

    public UserFactory(UserManager<UserModel> userManager, ApplicationDbContext dbContext) {
        _userManager = userManager;
        _dbContext = dbContext;

        // Initialize Bogus for generating fake user data
        _userFaker = new Faker<UserModel>()
          .RuleFor(u => u.UserName, f => f.Internet.Email())
          .RuleFor(u => u.Email, (f, u) => u.UserName)
          .RuleFor(u => u.FirstName, f => f.Name.FirstName())
          .RuleFor(u => u.LastName, f => f.Name.LastName())
          .RuleFor(u => u.Role, f => f.PickRandom<UserRole>());

        // Initialize Randomizer
        _randomizer = new Randomizer();
    }

    public async Task<UserModel> CreateFakeUserAsync() {
        var user = _userFaker.Generate();
        var roles = (UserRole[])Enum.GetValues(typeof(UserRole));
        user.Role = _randomizer.ArrayElement(roles);

        var result = await _userManager.CreateAsync(user, "Password123!");
        if (result.Succeeded) {
            await CreateRoleSpecificModelAsync(user);
            return user;
        }

        throw new System.Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    public async Task<UserModel> CreateTenantAsync(string email, string password, string firstName, string lastName) {
        var user = await CreateUserAsync(email, password, firstName, lastName, UserRole.Tenant);
        await CreateRoleSpecificModelAsync(user);
        return user;
    }

    public async Task<UserModel> CreateOwnerAsync(string email, string password, string firstName, string lastName) {
        var user = await CreateUserAsync(email, password, firstName, lastName, UserRole.Owner);
        await CreateRoleSpecificModelAsync(user);
        return user;
    }

    public async Task<UserModel> CreateAdminAsync(string email, string password, string firstName, string lastName) {
        var user = await CreateUserAsync(email, password, firstName, lastName, UserRole.Admin);
        await CreateRoleSpecificModelAsync(user);
        return user;
    }

    private async Task<UserModel> CreateUserAsync(string email, string password, string firstName, string lastName, UserRole role) {
        var user = new UserModel {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded) {
            await _userManager.AddToRoleAsync(user, role.ToString());
            await CreateRoleSpecificModelAsync(user);
            return user;
        }

        throw new System.Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    private async Task CreateRoleSpecificModelAsync(UserModel user) {
        switch (user.Role) {
            case UserRole.Tenant:
                var tenant = new TenantModel {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    User = user
                };
                await _dbContext.Tenants.AddAsync(tenant);
                break;
            case UserRole.Owner:
                var owner = new OwnerModel {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    User = user
                };
                await _dbContext.Owners.AddAsync(owner);
                break;
            case UserRole.Admin:
                var admin = new AdminModel {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    User = user
                };
                await _dbContext.Admins.AddAsync(admin);
                break;
        }

        await _dbContext.SaveChangesAsync();
    }
}
