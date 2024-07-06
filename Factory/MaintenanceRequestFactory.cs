using Cloud.Models;
using Bogus;
using Cloud.Models.Validator;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Factories
{
	/// <summary>
	/// Factory class for creating maintenance request models.
	/// </summary>
	public class MaintenanceRequestFactory
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly MaintenanceRequestValidator _requestValidator;

		/// <summary>
		/// Initializes a new instance of the MaintenanceRequestFactory class.
		/// </summary>
		/// <param name="dbContext">The database context for entity operations.</param>
		/// <param name="requestValidator">The validator for maintenance requests.</param>
		public MaintenanceRequestFactory(ApplicationDbContext dbContext, MaintenanceRequestValidator requestValidator)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
		}

		/// <summary>
		/// Creates a fake maintenance request with random data.
		/// </summary>
		/// <returns>The created MaintenanceRequestModel.</returns>
		public async Task<MaintenanceRequestModel> CreateFakeRequestAsync()
		{
			var (tenantIds, propertyIds) = await GetTenantAndPropertyIdsAsync();
			var request = GenerateFakeRequest(tenantIds, propertyIds);

			await SaveRequestAsync(request);
			return request;
		}

		/// <summary>
		/// Creates a maintenance request with specified details.
		/// </summary>
		/// <param name="tenantId">The ID of the tenant.</param>
		/// <param name="propertyId">The ID of the property.</param>
		/// <param name="description">The request description.</param>
		/// <param name="status">The request status.</param>
		/// <returns>The created MaintenanceRequestModel.</returns>
		public async Task<MaintenanceRequestModel> CreateRequestAsync(Guid tenantId, Guid propertyId, string description, MaintenanceStatus status)
		{
			var request = new MaintenanceRequestModel
			{
				TenantId = tenantId,
				PropertyId = propertyId,
				Description = description,
				Status = status
			};

			await SaveRequestAsync(request);
			return request;
		}

		/// <summary>
		/// Seeds the database with a specified number of fake maintenance requests.
		/// </summary>
		/// <param name="count">The number of requests to create.</param>
		public async Task SeedRequestsAsync(int count)
		{
			var (tenantIds, propertyIds) = await GetTenantAndPropertyIdsAsync();
			var requests = Enumerable.Range(0, count)
				.Select(_ => GenerateFakeRequest(tenantIds, propertyIds))
				.ToList();

			await _dbContext.MaintenanceRequests.AddRangeAsync(requests);
			await _dbContext.SaveChangesAsync();
		}

		private async Task<(List<Guid> TenantIds, List<Guid> PropertyIds)> GetTenantAndPropertyIdsAsync()
		{
			var tenantIds = await _dbContext.Tenants
				.Select(t => t.Id)
				.ToListAsync();

			var propertyIds = await _dbContext.Properties
				.Select(p => p.Id)
				.ToListAsync();

			if (!tenantIds.Any() || !propertyIds.Any())
			{
				throw new InvalidOperationException("No tenants or properties available for creating maintenance requests.");
			}

			return (tenantIds, propertyIds);
		}

		private MaintenanceRequestModel GenerateFakeRequest(List<Guid> tenantIds, List<Guid> propertyIds)
		{
			var faker = new Faker<MaintenanceRequestModel>()
				.RuleFor(r => r.TenantId, (f, _) => f.PickRandom(tenantIds))
				.RuleFor(r => r.PropertyId, (f, _) => f.PickRandom(propertyIds))
				.RuleFor(r => r.Description, (f, _) => f.Lorem.Paragraph())
				.RuleFor(r => r.Status, (f, _) => f.PickRandom<MaintenanceStatus>());

			return faker.Generate();
		}

		private async Task SaveRequestAsync(MaintenanceRequestModel request)
		{
			_requestValidator.ValidateRequest(request);
			_dbContext.MaintenanceRequests.Add(request);
			await _dbContext.SaveChangesAsync();
		}
	}
}