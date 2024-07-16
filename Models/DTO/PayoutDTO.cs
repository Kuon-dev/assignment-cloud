
namespace Cloud.Models.DTO
{
	public class PayoutPeriodDto
	{
		public Guid Id { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Status { get; set; } = null!;
	}

	public class OwnerPayoutStatusDto
	{
		public Guid OwnerId { get; set; }
		public string OwnerName { get; set; } = string.Empty;
		public bool HasReceivedPayout { get; set; }
	}

	public class OwnerPayoutDto
	{
		public Guid Id { get; set; }
		public Guid OwnerId { get; set; }
		public Guid PayoutPeriodId { get; set; }
		public decimal Amount { get; set; }
		public string Currency { get; set; } = null!;
		public string Status { get; set; } = null!;
		public DateTime CreatedAt { get; set; }
		public DateTime? ProcessedAt { get; set; }
		public string? TransactionReference { get; set; }
		public string? Notes { get; set; }
	}

	public class PayoutSettingsDto
	{
		public Guid Id { get; set; }
		public int PayoutCutoffDay { get; set; }
		public int ProcessingDay { get; set; }
		public string DefaultCurrency { get; set; } = null!;
		public decimal MinimumPayoutAmount { get; set; }
	}

	public class CreatePayoutPeriodDto
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}

	public class CreateOwnerPayoutDto
	{
		public Guid OwnerId { get; set; }
		public Guid PayoutPeriodId { get; set; }
		public decimal Amount { get; set; }
	}

	public class UpdatePayoutSettingsDto
	{
		public int PayoutCutoffDay { get; set; }
		public int ProcessingDay { get; set; }
		public string DefaultCurrency { get; set; } = null!;
		public decimal MinimumPayoutAmount { get; set; }
	}


	public class PaymentDto
	{
		public Guid Id { get; set; }
		public decimal Amount { get; set; }
		public string Currency { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public Guid PropertyId { get; set; }
		public string PropertyAddress { get; set; }
		public string TenantName { get; set; } = string.Empty;
	}
}