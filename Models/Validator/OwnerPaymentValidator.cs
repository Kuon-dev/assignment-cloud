
namespace Cloud.Models.Validator {
  public interface IOwnerPaymentValidationStrategy {
	void Validate(OwnerPaymentModel payment);
  }

  public class OwnerPaymentValidator {
	private readonly List<IOwnerPaymentValidationStrategy> _validationStrategies = new List<IOwnerPaymentValidationStrategy>();

	public void AddStrategy(IOwnerPaymentValidationStrategy strategy) {
	  _validationStrategies.Add(strategy);
	}

	public void ValidatePayment(OwnerPaymentModel payment) {
	  foreach (var strategy in _validationStrategies) {
		strategy.Validate(payment);
	  }
	}
  }

  public class OwnerIdValidationStrategy : IOwnerPaymentValidationStrategy {
	public void Validate(OwnerPaymentModel payment) {
	  if (payment.OwnerId == Guid.Empty) {
		throw new ArgumentException("Owner ID cannot be empty.", nameof(payment.OwnerId));
	  }
	}
  }

  public class OwnerPropertyIdValidationStrategy : IOwnerPaymentValidationStrategy {
	public void Validate(OwnerPaymentModel payment) {
	  if (payment.PropertyId == Guid.Empty) {
		throw new ArgumentException("Property ID cannot be empty.", nameof(payment.PropertyId));
	  }
	}
  }

  public class OwnerAmountValidationStrategy : IOwnerPaymentValidationStrategy {
	public void Validate(OwnerPaymentModel payment) {
	  if (payment.Amount <= 0) {
		throw new ArgumentException("Amount must be greater than zero.", nameof(payment.Amount));
	  }
	}
  }

  public class OwnerStatusValidationStrategy : IOwnerPaymentValidationStrategy {
	public void Validate(OwnerPaymentModel payment) {
	  if (!Enum.IsDefined(typeof(OwnerPaymentStatus), payment.Status)) {
		throw new ArgumentException("Invalid payment status.", nameof(payment.Status));
	  }
	}
  }

  public class PaymentDateValidationStrategy : IOwnerPaymentValidationStrategy {
	public void Validate(OwnerPaymentModel payment) {
	  if (payment.PaymentDate == default) {
		throw new ArgumentException("Payment date cannot be empty.", nameof(payment.PaymentDate));
	  }
	  if (payment.PaymentDate > DateTime.UtcNow) {
		throw new ArgumentException("Payment date cannot be in the future.", nameof(payment.PaymentDate));
	  }
	}
  }

  public class StripePaymentIntentIdValidationStrategy : IOwnerPaymentValidationStrategy {
	public void Validate(OwnerPaymentModel payment) {
	  if (string.IsNullOrWhiteSpace(payment.StripePaymentIntentId)) {
		throw new ArgumentException("Stripe Payment Intent ID cannot be empty.", nameof(payment.StripePaymentIntentId));
	  }
	}
  }

  public class DuplicateOwnerPaymentValidationStrategy : IOwnerPaymentValidationStrategy {
	private readonly ApplicationDbContext _dbContext;

	public DuplicateOwnerPaymentValidationStrategy(ApplicationDbContext dbContext) {
	  _dbContext = dbContext;
	}

	public void Validate(OwnerPaymentModel payment) {
	  if (_dbContext.OwnerPayments.Any(p => p.StripePaymentIntentId == payment.StripePaymentIntentId)) {
		throw new ArgumentException("Duplicate payment with the same Stripe Payment Intent ID already exists.", nameof(payment.StripePaymentIntentId));
	  }
	}
  }
}