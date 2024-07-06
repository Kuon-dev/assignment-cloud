namespace Cloud.Models.DTO
{
	public class CreateRentPaymentDto
	{
		public Guid TenantId { get; set; }
		public int Amount { get; set; } // Amount in cents
		public string Currency { get; set; } = "usd";
		public string PaymentIntentId { get; set; } = string.Empty;
		public string? PaymentMethodId { get; set; }
		public PaymentStatus Status { get; set; }
	}

	public class UpdateRentPaymentDto
	{
		public PaymentStatus? Status { get; set; }
		public string? PaymentMethodId { get; set; }
	}
}