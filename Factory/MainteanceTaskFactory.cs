using Cloud.Models;
using Bogus;
using Cloud.Models.Validator;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Factories
{
	/// <summary>
	/// Factory class for creating maintenance task models.
	/// </summary>
	public class MaintenanceTaskFactory
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly MaintenanceTaskValidator _taskValidator;

		/// <summary>
		/// Initializes a new instance of the MaintenanceTaskFactory class.
		/// </summary>
		/// <param name="dbContext">The database context for entity operations.</param>
		/// <param name="taskValidator">The validator for maintenance tasks.</param>
		public MaintenanceTaskFactory(ApplicationDbContext dbContext, MaintenanceTaskValidator taskValidator)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_taskValidator = taskValidator ?? throw new ArgumentNullException(nameof(taskValidator));
		}

		/// <summary>
		/// Creates a fake maintenance task with random data.
		/// </summary>
		/// <returns>The created MaintenanceTaskModel.</returns>
		public async Task<MaintenanceTaskModel> CreateFakeTaskAsync()
		{
			var requestIds = await GetRequestsAsync();
			var task = GenerateFakeTask(requestIds);
			await SaveTaskAsync(task);
			return task;
		}

		/// <summary>
		/// Creates a maintenance task with specified details.
		/// </summary>
		/// <param name="requestId">The ID of the maintenance request.</param>
		/// <param name="description">The task description.</param>
		/// <param name="estimatedCost">The estimated cost of the task.</param>
		/// <param name="status">The task status.</param>
		/// <returns>The created MaintenanceTaskModel.</returns>
		public async Task<MaintenanceTaskModel> CreateTaskAsync(Guid requestId, string description, decimal estimatedCost, Cloud.Models.TaskStatus status)
		{
			var task = new MaintenanceTaskModel
			{
				RequestId = requestId,
				Description = description,
				EstimatedCost = estimatedCost,
				Status = status
			};
			await SaveTaskAsync(task);
			return task;
		}

		/// <summary>
		/// Seeds the database with a specified number of fake maintenance tasks.
		/// </summary>
		/// <param name="count">The number of tasks to create.</param>
		public async Task SeedTasksAsync(int count)
		{
			var requestIds = await GetRequestsAsync();
			var tasks = Enumerable.Range(0, count)
				.Select(_ => GenerateFakeTask(requestIds))
				.ToList();
			await _dbContext.MaintenanceTasks.AddRangeAsync(tasks);
			await _dbContext.SaveChangesAsync();
		}

		private async Task<List<Guid>> GetRequestsAsync()
		{
			var requestIds = await _dbContext.MaintenanceRequests
				.Select(r => r.Id)
				.ToListAsync();

			if (!requestIds.Any())
			{
				throw new InvalidOperationException("No maintenance requests or staff members available for creating maintenance tasks.");
			}

			return (requestIds);
		}

		private MaintenanceTaskModel GenerateFakeTask(List<Guid> requestIds)
		{
			var faker = new Faker<MaintenanceTaskModel>()
				.RuleFor(t => t.RequestId, (f, _) => f.PickRandom(requestIds))
				.RuleFor(t => t.Description, (f, _) => f.Lorem.Paragraph())
				.RuleFor(t => t.EstimatedCost, (f, _) => f.Random.Decimal(50, 1000))
				.RuleFor(t => t.ActualCost, (f, _) => f.Random.Decimal(50, 1000))
				.RuleFor(t => t.StartDate, (f, _) => f.Date.Past())
				.RuleFor(t => t.CompletionDate, (f, t) => f.Date.Between(t.StartDate ?? DateTime.Now, DateTime.Now.AddDays(30)))
				.RuleFor(t => t.Status, (f, _) => f.PickRandom<Cloud.Models.TaskStatus>());

			return faker.Generate();
		}

		private async Task SaveTaskAsync(MaintenanceTaskModel task)
		{
			_taskValidator.ValidateTask(task);
			_dbContext.MaintenanceTasks.Add(task);
			await _dbContext.SaveChangesAsync();
		}
	}
}