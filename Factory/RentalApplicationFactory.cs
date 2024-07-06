using Cloud.Models;
using Bogus;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Factories
{
	/// <summary>
	/// Factory class for creating rental application models.
	/// </summary>
	public class RentalApplicationFactory
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly RentalApplicationValidator _applicationValidator;

		/// <summary>
		/// Initializes a new instance of the RentalApplicationFactory class.
		/// </summary>
		/// <param name="dbContext">The database context for entity operations.</param>
		/// <param name="applicationValidator">The validator for rental applications.</param>
		public RentalApplicationFactory(ApplicationDbContext dbContext, RentalApplicationValidator applicationValidator)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_applicationValidator = applicationValidator ?? throw new ArgumentNullException(nameof(applicationValidator));
		}

		/// <summary>
		/// Creates a fake rental application with random data.
		/// </summary>
		/// <returns>The created RentalApplicationModel.</returns>
		public async Task<RentalApplicationModel> CreateFakeApplicationAsync()
		{
			var (tenantIds, listingIds) = await GetTenantAndListingIdsAsync();
			var application = GenerateFakeApplication(tenantIds, listingIds);

			await SaveApplicationAsync(application);
			return application;
		}

		/// <summary>
		/// Creates a rental application with specified details.
		/// </summary>
		/// <param name="tenantId">The ID of the tenant.</param>
		/// <param name="listingId">The ID of the listing.</param>
		/// <param name="status">The application status.</param>
		/// <param name="applicationDate">The application date.</param>
		/// <param name="employmentInfo">Employment information.</param>
		/// <param name="references">References information.</param>
		/// <param name="additionalNotes">Additional notes.</param>
		/// <returns>The created RentalApplicationModel.</returns>
		public async Task<RentalApplicationModel> CreateApplicationAsync(Guid tenantId, Guid listingId, ApplicationStatus status, DateTime applicationDate, string? employmentInfo, string? references, string? additionalNotes)
		{
			var application = new RentalApplicationModel
			{
				TenantId = tenantId,
				ListingId = listingId,
				Status = status,
				ApplicationDate = applicationDate,
				EmploymentInfo = employmentInfo,
				References = references,
				AdditionalNotes = additionalNotes
			};

			await SaveApplicationAsync(application);
			return application;
		}

		/// <summary>
		/// Seeds the database with a specified number of fake rental applications.
		/// </summary>
		/// <param name="count">The number of applications to create.</param>
		public async Task SeedApplicationsAsync(int count)
		{
			var (tenantIdGuid, listingIds) = await GetTenantAndListingIdsAsync();
			var applications = Enumerable.Range(0, count)
				.Select(_ => GenerateFakeApplication(tenantIdGuid, listingIds))
				.ToList();

			await _dbContext.RentalApplications.AddRangeAsync(applications);
			await _dbContext.SaveChangesAsync();
		}

		private async Task<(List<Guid> TenantIds, List<Guid> ListingIds)> GetTenantAndListingIdsAsync()
		{
			var tenantIdGuid = await _dbContext.Users
				.Where(u => u.Role == UserRole.Tenant && u.Tenant != null)
				.Select(u => u.Tenant!.Id)
				.ToListAsync();

			var listingIds = await _dbContext.Listings
				.Select(l => l.Id)
				.ToListAsync();

			if (!tenantIdGuid.Any() || !listingIds.Any())
			{
				throw new InvalidOperationException("No tenants or listings available for creating rental applications.");
			}

			return (tenantIdGuid, listingIds);
		}

		private RentalApplicationModel GenerateFakeApplication(List<Guid> tenantIds, List<Guid> listingIds)
		{
			var faker = new Faker<RentalApplicationModel>()
				.RuleFor(r => r.TenantId, (f, _) => f.PickRandom(tenantIds))
				.RuleFor(r => r.ListingId, (f, _) => f.PickRandom(listingIds))
				.RuleFor(r => r.Status, (f, _) => f.PickRandom<ApplicationStatus>())
				.RuleFor(r => r.ApplicationDate, (f, _) => f.Date.Recent().ToUniversalTime())
				.RuleFor(r => r.EmploymentInfo, (f, _) => f.Lorem.Paragraph())
				.RuleFor(r => r.References, (f, _) => f.Lorem.Paragraph())
				.RuleFor(r => r.AdditionalNotes, (f, _) => f.Lorem.Paragraph());

			return faker.Generate();
		}

		private async Task SaveApplicationAsync(RentalApplicationModel application)
		{
			_applicationValidator.ValidateApplication(application);
			_dbContext.RentalApplications.Add(application);
			await _dbContext.SaveChangesAsync();
		}
	}
}