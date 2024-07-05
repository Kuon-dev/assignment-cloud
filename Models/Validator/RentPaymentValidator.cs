namespace Cloud.Models.Validator {
  public interface IRentPaymentValidationStrategy {
	void Validate(RentPaymentModel payment);
  }

  public class RentPaymentValidator {
	private readonly List<IRentPaymentValidationStrategy> _validationStrategies = new List<IRentPaymentValidationStrategy>();

	public void AddStrategy(IRentPaymentValidationStrategy strategy) {
	  _validationStrategies.Add(strategy);
	}

	public void ValidatePayment(RentPaymentModel payment) {
	  foreach (var strategy in _validationStrategies) {
		strategy.Validate(payment);
	  }
	}
  }

  public class TenantIdValidationStrategy : IRentPaymentValidationStrategy {
	public void Validate(RentPaymentModel payment) {
	  if (payment.TenantId == Guid.Empty) {
		throw new ArgumentException("Tenant ID cannot be empty.", nameof(payment.TenantId));
	  }
	}
  }

  public class AmountValidationStrategy : IRentPaymentValidationStrategy {
	public void Validate(RentPaymentModel payment) {
	  if (payment.Amount <= 0) {
		throw new ArgumentException("Amount must be greater than zero.", nameof(payment.Amount));
	  }
	}
  }

  public class CurrencyValidationStrategy : IRentPaymentValidationStrategy {
	private readonly HashSet<string> _validCurrencies = new HashSet<string> { "usd", "eur", "gbp", "cad" }; // Add more valid currency codes as needed

	public void Validate(RentPaymentModel payment) {
	  if (string.IsNullOrWhiteSpace(payment.Currency)) {
		throw new ArgumentException("Currency cannot be empty.", nameof(payment.Currency));
	  }

	  if (!_validCurrencies.Contains(payment.Currency.ToLower())) {
		throw new ArgumentException("Invalid currency code.", nameof(payment.Currency));
	  }
	}
  }

  public class PaymentIntentIdValidationStrategy : IRentPaymentValidationStrategy {
	public void Validate(RentPaymentModel payment) {
	  if (string.IsNullOrWhiteSpace(payment.PaymentIntentId)) {
		throw new ArgumentException("Payment Intent ID cannot be empty.", nameof(payment.PaymentIntentId));
	  }
	}
  }

  public class PaymentMethodIdValidationStrategy : IRentPaymentValidationStrategy {
	public void Validate(RentPaymentModel payment) {
	  if (payment.Status == PaymentStatus.RequiresPaymentMethod && string.IsNullOrWhiteSpace(payment.PaymentMethodId)) {
		throw new ArgumentException("Payment Method ID cannot be empty when status requires a payment method.", nameof(payment.PaymentMethodId));
	  }
	}
  }

  public class StatusValidationStrategy : IRentPaymentValidationStrategy {
	public void Validate(RentPaymentModel payment) {
	  if (!Enum.IsDefined(typeof(PaymentStatus), payment.Status)) {
		throw new ArgumentException("Invalid payment status.", nameof(payment.Status));
	  }
	}
  }

  public class DuplicatePaymentValidationStrategy : IRentPaymentValidationStrategy {
	private readonly ApplicationDbContext _dbContext;

	public DuplicatePaymentValidationStrategy(ApplicationDbContext dbContext) {
	  _dbContext = dbContext;
	}

	public void Validate(RentPaymentModel payment) {
	  if (_dbContext.RentPayments.Any(p => p.PaymentIntentId == payment.PaymentIntentId)) {
		throw new ArgumentException("Duplicate payment with the same Payment Intent ID already exists.", nameof(payment.PaymentIntentId));
	  }
	}
  }

}