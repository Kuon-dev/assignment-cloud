using Cloud.Models;
using Bogus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cloud.Models.DTO;

namespace Cloud.Factory {
  /// <summary>
  /// Factory class for creating rental application models.
  /// </summary>
  public class RentalApplicationFactory {
	private readonly ApplicationDbContext _dbContext;
	private readonly Faker<RentalApplicationModel> _applicationFaker;
	private readonly RentalApplicationValidator _applicationValidator;

	/// <summary>
	/// Initializes a new instance of the RentalApplicationFactory class.
	/// </summary>
	/// <param name="dbContext">The database context for entity operations.</param>
	public RentalApplicationFactory(ApplicationDbContext dbContext, RentalApplicationValidator applicationValidator) {
	  _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	  _applicationValidator = applicationValidator ?? throw new ArgumentNullException(nameof(applicationValidator));

	  // Initialize Bogus for generating fake rental application data
	  _applicationFaker = new Faker<RentalApplicationModel>()
		  .RuleFor(r => r.TenantId, f => f.Random.Guid())
		  .RuleFor(r => r.ListingId, f => f.Random.Guid())
		  .RuleFor(r => r.Status, f => f.PickRandom<ApplicationStatus>())
		  .RuleFor(r => r.ApplicationDate, f => f.Date.Recent())
		  .RuleFor(r => r.EmploymentInfo, f => f.Lorem.Paragraph())
		  .RuleFor(r => r.References, f => f.Lorem.Paragraph())
		  .RuleFor(r => r.AdditionalNotes, f => f.Lorem.Paragraph());
	}

	/// <summary>
	/// Creates a fake rental application with random data.
	/// </summary>
	/// <returns>The created RentalApplicationModel.</returns>
	public async Task<RentalApplicationModel> CreateFakeApplicationAsync() {
	  if (_dbContext.RentalApplications == null) {
		throw new InvalidOperationException("RentalApplication DbSet is not initialized.");
	  }

	  var application = _applicationFaker.Generate();
	  _applicationValidator.ValidateApplication(application);
	  _dbContext.RentalApplications.Add(application);
	  await _dbContext.SaveChangesAsync();
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
	public async Task<RentalApplicationModel> CreateApplicationAsync(Guid tenantId, Guid listingId, ApplicationStatus status, DateTime applicationDate, string? employmentInfo, string? references, string? additionalNotes) {
	  if (_dbContext.RentalApplications == null) {
		throw new InvalidOperationException("RentalApplication DbSet is not initialized.");
	  }

	  var application = new RentalApplicationModel {
		TenantId = tenantId,
		ListingId = listingId,
		Status = status,
		ApplicationDate = applicationDate,
		EmploymentInfo = employmentInfo,
		References = references,
		AdditionalNotes = additionalNotes
	  };

	  _applicationValidator.ValidateApplication(application);
	  _dbContext.RentalApplications.Add(application);
	  await _dbContext.SaveChangesAsync();
	  return application;
	}

	/// <summary>
	/// Seeds the database with a specified number of fake rental applications.
	/// </summary>
	/// <param name="count">The number of applications to create.</param>
	public async Task SeedApplicationsAsync(int count) {
	  if (_dbContext.RentalApplications == null) {
		throw new InvalidOperationException("RentalApplication DbSet is not initialized.");
	  }

	  var applications = new List<RentalApplicationModel>(count);

	  for (int i = 0; i < count; i++) {
		var application = _applicationFaker.Generate();
		_applicationValidator.ValidateApplication(application);
		applications.Add(application);
	  }

	  await _dbContext.RentalApplications.AddRangeAsync(applications);
	  await _dbContext.SaveChangesAsync();
	}
  }
}
