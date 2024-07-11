// OwnerPaymentModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models
{
	public class OwnerPaymentModel : BaseEntity
	{

		[Required]
		public Guid OwnerId { get; set; }

		[ForeignKey("OwnerId")]
		public OwnerModel Owner { get; set; } = null!;

		[Required]
		public Guid PropertyId { get; set; }

		[ForeignKey("PropertyId")]
		public PropertyModel Property { get; set; } = null!;

		[Required]
		public int Year { get; set; }

		[Required]
		public int Month { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }

		[Required]
		public DateTime PaymentDate { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal AdminFee { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal UtilityFees { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal MaintenanceCost { get; set; }

		[Required]
		public string StripePaymentIntentId { get; set; } = null!;

		[Required]
		public OwnerPaymentStatus Status { get; set; }
	}

	public enum OwnerPaymentStatus
	{
		Pending,
		Processed,
		Failed
	}
}