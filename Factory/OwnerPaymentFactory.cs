using Cloud.Models;
using Bogus;
using Cloud.Models.Validator;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Factories
{
	/// <summary>
	/// Factory class for creating owner payment models.
	/// </summary>
	public class OwnerPaymentFactory
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly OwnerPaymentValidator _paymentValidator;

		/// <summary>
		/// Initializes a new instance of the OwnerPaymentFactory class.
		/// </summary>
		/// <param name="dbContext">The database context for entity operations.</param>
		/// <param name="paymentValidator">The validator for owner payments.</param>
		public OwnerPaymentFactory(ApplicationDbContext dbContext, OwnerPaymentValidator paymentValidator)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_paymentValidator = paymentValidator ?? throw new ArgumentNullException(nameof(paymentValidator));
		}

		/// <summary>
		/// Creates a fake owner payment with random data.
		/// </summary>
		/// <returns>The created OwnerPaymentModel.</returns>
		public async Task<OwnerPaymentModel> CreateFakePaymentAsync()
		{
			var (ownerIds, propertyIds) = await GetOwnerAndPropertyIdsAsync();
			var payment = GenerateFakePayment(ownerIds, propertyIds);
			await SavePaymentAsync(payment);
			return payment;
		}

		/// <summary>
		/// Creates an owner payment with specified details.
		/// </summary>
		/// <param name="ownerId">The ID of the owner.</param>
		/// <param name="propertyId">The ID of the property.</param>
		/// <param name="amount">The payment amount.</param>
		/// <param name="status">The payment status.</param>
		/// <returns>The created OwnerPaymentModel.</returns>
		public async Task<OwnerPaymentModel> CreatePaymentAsync(Guid ownerId, int year, int month, decimal amount, OwnerPaymentStatus status)
		{
			var payment = new OwnerPaymentModel
			{
				OwnerId = ownerId,
				Year = year,
				Month = month,
				Amount = amount,
				Status = status,
				PaymentDate = DateTime.UtcNow
			};
			await SavePaymentAsync(payment);
			return payment;
		}

		/// <summary>
		/// Seeds the database with a specified number of fake owner payments.
		/// </summary>
		/// <param name="count">The number of payments to create.</param>
		public async Task SeedPaymentsAsync(int count)
		{
			var (ownerIds, propertyIds) = await GetOwnerAndPropertyIdsAsync();
			var payments = Enumerable.Range(0, count)
				.Select(_ => GenerateFakePayment(ownerIds, propertyIds))
				.ToList();
			await _dbContext.OwnerPayments.AddRangeAsync(payments);
			await _dbContext.SaveChangesAsync();
		}

		private async Task<(List<Guid> OwnerIds, List<Guid> PropertyIds)> GetOwnerAndPropertyIdsAsync()
		{
			var ownerIds = await _dbContext.Owners
				.Select(o => o.Id)
				.ToListAsync();
			var propertyIds = await _dbContext.Properties
				.Select(p => p.Id)
				.ToListAsync();
			if (!ownerIds.Any() || !propertyIds.Any())
			{
				throw new InvalidOperationException("No owners or properties available for creating owner payments.");
			}
			return (ownerIds, propertyIds);
		}

		private OwnerPaymentModel GenerateFakePayment(List<Guid> ownerIds, List<Guid> propertyIds)
		{
			var faker = new Faker<OwnerPaymentModel>()
				.RuleFor(p => p.OwnerId, (f, _) => f.PickRandom(ownerIds))
				.RuleFor(p => p.PropertyId, (f, _) => f.PickRandom(propertyIds))
				.RuleFor(p => p.Amount, (f, _) => f.Finance.Amount(100, 10000))
				.RuleFor(p => p.Status, (f, _) => f.PickRandom<OwnerPaymentStatus>())
				.RuleFor(p => p.PaymentDate, (f, _) => f.Date.Past(1))
				.RuleFor(p => p.StripePaymentIntentId, (f, _) => f.Random.AlphaNumeric(24));
			return faker.Generate();
		}

		private async Task SavePaymentAsync(OwnerPaymentModel payment)
		{
			_paymentValidator.ValidatePayment(payment);
			_dbContext.OwnerPayments.Add(payment);
			await _dbContext.SaveChangesAsync();
		}
	}
}