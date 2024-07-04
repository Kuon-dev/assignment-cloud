// OwnerPaymentModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Models {
  public class OwnerPaymentModel {
	[Key]
	public Guid Id { get; set; }

	[Required]
	public Guid OwnerId { get; set; }

	[ForeignKey("OwnerId")]
	public OwnerModel Owner { get; set; }

	[Required]
	public Guid PropertyId { get; set; }

	[ForeignKey("PropertyId")]
	public PropertyModel Property { get; set; }

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

	public string StripePaymentIntentId { get; set; }

	[Required]
	public OwnerPaymentStatus Status { get; set; }
  }

  public enum OwnerPaymentStatus {
	Pending,
	Processed,
	Failed
  }
}