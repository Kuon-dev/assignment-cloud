namespace Cloud.Models.Validator
{
	public interface IStripeCustomerValidationStrategy
	{
		void Validate(StripeCustomerModel stripeCustomer);
	}

	public class StripeCustomerValidator
	{
		private readonly List<IStripeCustomerValidationStrategy> _validationStrategies = new List<IStripeCustomerValidationStrategy>();

		public void AddStrategy(IStripeCustomerValidationStrategy strategy)
		{
			_validationStrategies.Add(strategy);
		}

		public void ValidateStripeCustomer(StripeCustomerModel stripeCustomer)
		{
			foreach (var strategy in _validationStrategies)
			{
				strategy.Validate(stripeCustomer);
			}
		}
	}

	public class UserIdValidationStrategy : IStripeCustomerValidationStrategy
	{
		public void Validate(StripeCustomerModel stripeCustomer)
		{
			if (string.IsNullOrWhiteSpace(stripeCustomer.UserId))
			{
				throw new ArgumentException("User ID cannot be empty.", nameof(stripeCustomer.UserId));
			}
		}
	}

	public class StripeCustomerIdValidationStrategy : IStripeCustomerValidationStrategy
	{
		public void Validate(StripeCustomerModel stripeCustomer)
		{
			if (string.IsNullOrWhiteSpace(stripeCustomer.StripeCustomerId.ToString()))
			{
				throw new ArgumentException("Stripe Customer ID cannot be empty.", nameof(stripeCustomer.StripeCustomerId));
			}
		}
	}

	public class DuplicateStripeCustomerValidationStrategy : IStripeCustomerValidationStrategy
	{
		private readonly ApplicationDbContext _dbContext;

		public DuplicateStripeCustomerValidationStrategy(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public void Validate(StripeCustomerModel stripeCustomer)
		{
			if (_dbContext.StripeCustomers.Any(c => c.StripeCustomerId == stripeCustomer.StripeCustomerId))
			{
				throw new ArgumentException("Duplicate Stripe Customer ID already exists.", nameof(stripeCustomer.StripeCustomerId));
			}
		}
	}
}