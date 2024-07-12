using Cloud.Models.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Models
{
	public class PayoutPeriod : BaseEntity
	{
		[Required]
		public DateTime StartDate { get; set; }

		[Required]
		public DateTime EndDate { get; set; }

		[Required]
		public PayoutPeriodStatus Status { get; set; }
	}

	public enum PayoutPeriodStatus
	{
		Pending,
		Processing,
		Completed
	}

	public class OwnerPayout : BaseEntity
	{
		[Required]
		public Guid OwnerId { get; set; }

		[ForeignKey("OwnerId")]
		public OwnerModel? Owner { get; set; }

		[Required]
		public Guid PayoutPeriodId { get; set; }

		[ForeignKey("PayoutPeriodId")]
		public PayoutPeriod? PayoutPeriod { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }

		[Required]
		[StringLength(3)]
		public string Currency { get; set; } = "USD";

		[Required]
		public PayoutStatus Status { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		public DateTime? ProcessedAt { get; set; }

		[StringLength(100)]
		public string? TransactionReference { get; set; }

		public string? Notes { get; set; }
	}

	public enum PayoutStatus
	{
		Pending,
		Processing,
		Completed,
		Failed
	}

	public class PayoutSettings : BaseEntity
	{
		[Required]
		[Range(1, 31)]
		public int PayoutCutoffDay { get; set; }

		[Required]
		[Range(1, 31)]
		public int ProcessingDay { get; set; }

		[Required]
		[StringLength(3)]
		public string DefaultCurrency { get; set; } = "USD";

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal MinimumPayoutAmount { get; set; }
	}
}