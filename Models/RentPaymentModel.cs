// RentPaymentModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models
{

	public class RentPaymentModel : BaseEntity
	{
		[Required]
		public Guid TenantId { get; set; }

		[ForeignKey("TenantId")]
		public TenantModel? Tenant { get; set; }

		public int Amount { get; set; } // Amount in cents

		[Required]
		public string Currency { get; set; } = "usd";

		[Required]
		public string PaymentIntentId { get; set; } = string.Empty;

		public string? PaymentMethodId { get; set; }

		public PaymentStatus Status { get; set; }
	}

	// PaymentStatusModel.cs
	public enum PaymentStatus
	{
		RequiresPaymentMethod,
		RequiresConfirmation,
		RequiresAction,
		Processing,
		RequiresCapture,
		Cancelled,
		Succeeded
	}
}