namespace Cloud.Models.DTO {
  public class CreateRentPaymentDto {
	public Guid TenantId { get; set; }
	public int Amount { get; set; } // Amount in cents
	public string Currency { get; set; } = "usd";
	public string PaymentIntentId { get; set; } = string.Empty;
	public string? PaymentMethodId { get; set; }
	public PaymentStatus Status { get; set; }
  }

  public class UpdateRentPaymentDto {
	public PaymentStatus? Status { get; set; }
	public string? PaymentMethodId { get; set; }
  }

  public class PaymentValidator {
	public void ValidatePayment(RentPaymentModel payment) {
	  if (payment.TenantId == Guid.Empty) {
		throw new ArgumentException("Tenant ID cannot be empty.", nameof(payment.TenantId));
	  }

	  if (payment.Amount <= 0) {
		throw new ArgumentException("Amount must be greater than zero.", nameof(payment.Amount));
	  }

	  if (string.IsNullOrWhiteSpace(payment.Currency)) {
		throw new ArgumentException("Currency cannot be empty.", nameof(payment.Currency));
	  }

	  if (string.IsNullOrWhiteSpace(payment.PaymentIntentId)) {
		throw new ArgumentException("Payment Intent ID cannot be empty.", nameof(payment.PaymentIntentId));
	  }

	  // Add more validation rules as necessary
	}
  }
}
