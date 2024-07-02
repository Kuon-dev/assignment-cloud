// RentPaymentModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Models {

  public class RentPaymentModel {
	[Key]
	public Guid Id { get; set; }

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

	public DateTime CreatedAt { get; set; }

	public DateTime UpdatedAt { get; set; }
  }

  // PaymentStatusModel.cs
  public enum PaymentStatus {
	RequiresPaymentMethod,
	RequiresConfirmation,
	RequiresAction,
	Processing,
	RequiresCapture,
	Cancelled,
	Succeeded
  }
}