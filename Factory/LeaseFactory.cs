using Bogus;
using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Factories
{
	/// <summary>
	/// Factory class for creating lease models with validations.
	/// </summary>
	public class LeaseFactory
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly Faker<LeaseModel> _leaseFaker;
		private readonly LeaseValidator _leaseValidator;

		/// <summary>
		/// Initializes a new instance of the LeaseFactory class.
		/// </summary>
		/// <param name="dbContext">The database context for entity operations.</param>
		public LeaseFactory(ApplicationDbContext dbContext, LeaseValidator leaseValidator)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_leaseValidator = leaseValidator ?? throw new ArgumentNullException(nameof(leaseValidator));
			// Initialize Bogus for generating fake lease data
			_leaseFaker = new Faker<LeaseModel>()
				.RuleFor(l => l.StartDate, f => f.Date.Future().ToUniversalTime())
				.RuleFor(l => l.EndDate, (f, l) => l.StartDate.AddMonths(f.Random.Int(6, 24)).ToUniversalTime())
				.RuleFor(l => l.RentAmount, f => f.Finance.Amount(500, 5000))
				.RuleFor(l => l.SecurityDeposit, f => f.Finance.Amount(500, 5000))
				.RuleFor(l => l.IsActive, f => f.Random.Bool());
		}

		/// <summary>
		/// Creates a fake lease with random data.
		/// </summary>
		/// <returns>The created LeaseModel.</returns>
		public async Task<LeaseModel> CreateFakeLeaseAsync()
		{
			if (_dbContext.Leases == null)
			{
				throw new InvalidOperationException("Lease DbSet is not initialized.");
			}

			var lease = _leaseFaker.Generate();
			_leaseValidator.ValidateLease(lease);
			_dbContext.Leases.Add(lease);
			await _dbContext.SaveChangesAsync();
			return lease;
		}

		/// <summary>
		/// Creates a lease with specified details.
		/// </summary>
		/// <param name="tenantId">The ID of the tenant.</param>
		/// <param name="startDate">The start date of the lease.</param>
		/// <param name="endDate">The end date of the lease.</param>
		/// <param name="rentAmount">The rent amount for the lease.</param>
		/// <param name="securityDeposit">The security deposit for the lease.</param>
		/// <param name="isActive">Indicates whether the lease is active.</param>
		/// <returns>The created LeaseModel.</returns>
		public async Task<LeaseModel> CreateLeaseAsync(Guid tenantId, Guid propertyId, DateTime startDate, DateTime endDate, decimal rentAmount, decimal securityDeposit, bool isActive)
		{
			if (_dbContext.Leases == null)
			{
				throw new InvalidOperationException("Lease DbSet is not initialized.");
			}

			var lease = new LeaseModel
			{
				TenantId = tenantId,
				PropertyId = propertyId,
				StartDate = startDate,
				EndDate = endDate,
				RentAmount = rentAmount,
				SecurityDeposit = securityDeposit,
				IsActive = isActive
			};

			_leaseValidator.ValidateLease(lease);
			_dbContext.Leases.Add(lease);
			await _dbContext.SaveChangesAsync();
			return lease;
		}

		/// <summary>
		/// Seeds the database with a specified number of fake leases.
		/// </summary>
		/// <param name="count">The number of leases to create.</param>
		public async Task SeedLeasesAsync(int count)
		{
			if (_dbContext.Leases == null)
			{
				throw new InvalidOperationException("Lease DbSet is not initialized.");
			}

			var leases = new List<LeaseModel>(count);

			for (int i = 0; i < count; i++)
			{
				var lease = _leaseFaker.Generate();
				var Tenants = await _dbContext.Tenants.ToListAsync();
				var Properties = await _dbContext.Properties.ToListAsync();
				lease.TenantId = Tenants[new Random().Next(Tenants.Count)].Id;
				lease.PropertyId = Properties[new Random().Next(Properties.Count)].Id;

				_leaseValidator.ValidateLease(lease);
				leases.Add(lease);
			}

			await _dbContext.Leases.AddRangeAsync(leases);
			await _dbContext.SaveChangesAsync();
		}
	}
}