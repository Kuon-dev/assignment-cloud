using Cloud.Models;
using Bogus;
using Cloud.Models.Validator;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Factories
{
	/// <summary>
	/// Factory class for creating maintenance requests and associated tasks.
	/// </summary>
	public class MaintenanceFactory
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly MaintenanceRequestValidator _requestValidator;
		private readonly MaintenanceTaskValidator _taskValidator;

		/// <summary>
		/// Initializes a new instance of the MaintenanceFactory class.
		/// </summary>
		/// <param name="dbContext">The database context for entity operations.</param>
		/// <param name="requestValidator">The validator for maintenance requests.</param>
		/// <param name="taskValidator">The validator for maintenance tasks.</param>
		public MaintenanceFactory(
			ApplicationDbContext dbContext,
			MaintenanceRequestValidator requestValidator,
			MaintenanceTaskValidator taskValidator)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
			_taskValidator = taskValidator ?? throw new ArgumentNullException(nameof(taskValidator));
		}

		/// <summary>
		/// Creates a maintenance request with an associated task and assigns it to a random admin.
		/// </summary>
		/// <param name="tenantId">The ID of the tenant.</param>
		/// <param name="propertyId">The ID of the property.</param>
		/// <param name="description">The request description.</param>
		/// <returns>A tuple containing the created MaintenanceRequestModel and MaintenanceTaskModel.</returns>
		public async Task<(MaintenanceRequestModel Request, MaintenanceTaskModel Task)> CreateRequestWithTaskAsync(
			Guid tenantId,
			Guid propertyId,
			string description)
		{
			using var transaction = await _dbContext.Database.BeginTransactionAsync();

			try
			{
				var request = new MaintenanceRequestModel
				{
					TenantId = tenantId,
					PropertyId = propertyId,
					Description = description,
					Status = MaintenanceStatus.Pending
				};

				_requestValidator.ValidateRequest(request);
				_dbContext.MaintenanceRequests.Add(request);
				await _dbContext.SaveChangesAsync();

				var task = new MaintenanceTaskModel
				{
					RequestId = request.Id,
					Description = $"Task for maintenance request: {description}",
					EstimatedCost = 0, // This can be updated later
					Status = Cloud.Models.TaskStatus.Pending,
				};

				_taskValidator.ValidateTask(task);
				_dbContext.MaintenanceTasks.Add(task);
				await _dbContext.SaveChangesAsync();

				await transaction.CommitAsync();

				return (request, task);
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		/// <summary>
		/// Creates a fake maintenance request with an associated task for testing or seeding purposes.
		/// </summary>
		/// <returns>A tuple containing the created MaintenanceRequestModel and MaintenanceTaskModel.</returns>
		public async Task<(MaintenanceRequestModel Request, MaintenanceTaskModel Task)> CreateFakeRequestWithTaskAsync()
		{
			var (tenantIds, propertyIds) = await GetTenantAndPropertyIdsAsync();
			var adminIds = await GetAdminIdsAsync();

			var request = GenerateFakeRequest(tenantIds, propertyIds);
			var task = GenerateFakeTask(request.Id, adminIds);

			using var transaction = await _dbContext.Database.BeginTransactionAsync();

			try
			{
				_requestValidator.ValidateRequest(request);
				_dbContext.MaintenanceRequests.Add(request);
				await _dbContext.SaveChangesAsync();

				_taskValidator.ValidateTask(task);
				_dbContext.MaintenanceTasks.Add(task);
				await _dbContext.SaveChangesAsync();

				await transaction.CommitAsync();

				return (request, task);
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		/// <summary>
		/// Seeds the database with a specified number of fake maintenance requests and associated tasks.
		/// </summary>
		/// <param name="count">The number of request-task pairs to create.</param>
		public async Task SeedRequestsAndTasksAsync(int count)
		{
			var (tenantIds, propertyIds) = await GetTenantAndPropertyIdsAsync();
			var adminIds = await GetAdminIdsAsync();

			var requestsAndTasks = Enumerable.Range(0, count)
				.Select(_ =>
				{
					var request = GenerateFakeRequest(tenantIds, propertyIds);
					var task = GenerateFakeTask(request.Id, adminIds);
					return (Request: request, Task: task);
				})
				.ToList();

			using var transaction = await _dbContext.Database.BeginTransactionAsync();

			try
			{
				await _dbContext.MaintenanceRequests.AddRangeAsync(requestsAndTasks.Select(rt => rt.Request));
				await _dbContext.SaveChangesAsync();

				await _dbContext.MaintenanceTasks.AddRangeAsync(requestsAndTasks.Select(rt => rt.Task));
				await _dbContext.SaveChangesAsync();

				await transaction.CommitAsync();
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		private async Task<(List<Guid> TenantIds, List<Guid> PropertyIds)> GetTenantAndPropertyIdsAsync()
		{
			var tenantIds = await _dbContext.Tenants.Select(t => t.Id).ToListAsync();
			var propertyIds = await _dbContext.Properties.Select(p => p.Id).ToListAsync();

			if (!tenantIds.Any() || !propertyIds.Any())
			{
				throw new InvalidOperationException("No tenants or properties available for creating maintenance requests.");
			}

			return (tenantIds, propertyIds);
		}

		private async Task<List<Guid>> GetAdminIdsAsync()
		{
			var adminIds = await _dbContext.Admins.Select(a => a.Id).ToListAsync();

			if (!adminIds.Any())
			{
				throw new InvalidOperationException("No admin users available for assigning maintenance tasks.");
			}

			return adminIds;
		}

		private async Task<Guid> GetRandomAdminIdAsync()
		{
			var adminIds = await GetAdminIdsAsync();
			return adminIds[new Random().Next(adminIds.Count)];
		}

		private MaintenanceRequestModel GenerateFakeRequest(List<Guid> tenantIds, List<Guid> propertyIds)
		{
			var faker = new Faker<MaintenanceRequestModel>()
				.RuleFor(r => r.TenantId, f => f.PickRandom(tenantIds))
				.RuleFor(r => r.PropertyId, f => f.PickRandom(propertyIds))
				.RuleFor(r => r.Description, f => f.Lorem.Paragraph())
				.RuleFor(r => r.Status, f => f.PickRandom<MaintenanceStatus>());

			return faker.Generate();
		}

		private MaintenanceTaskModel GenerateFakeTask(Guid requestId, List<Guid> adminIds)
		{
			var faker = new Faker<MaintenanceTaskModel>()
				.RuleFor(t => t.RequestId, requestId)
				.RuleFor(t => t.Description, (f, t) => $"Task for maintenance request: {f.Lorem.Sentence()}")
				.RuleFor(t => t.EstimatedCost, f => f.Random.Decimal(50, 1000))
				.RuleFor(t => t.ActualCost, f => f.Random.Decimal(50, 1000))
				.RuleFor(t => t.StartDate, f => f.Date.Past().ToUniversalTime())
				.RuleFor(t => t.CompletionDate, (f, t) => f.Date.Between(t.StartDate ?? DateTime.Now, DateTime.Now.AddDays(30)).ToUniversalTime())
				.RuleFor(t => t.Status, f => f.PickRandom<Cloud.Models.TaskStatus>());

			return faker.Generate();
		}
	}
}