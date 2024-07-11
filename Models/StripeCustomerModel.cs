// StripeCustomerModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models
{
	public class StripeCustomerModel : BaseEntity
	{
		[Required]
		public String UserId { get; set; } = null!;

		[ForeignKey("UserId")]
		public UserModel? User { get; set; }

		[Required]
		public string StripeCustomerId { get; set; } = string.Empty;

		[Required]
		public bool IsVerified { get; set; } = false;

		public string? DefaultPaymentMethodId { get; set; }

		public string? DefaultSourceId { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Balance { get; set; }

		public string? Currency { get; set; }

		public DateTime? Delinquent { get; set; }

		public DateTime? Created { get; set; }

		public string? InvoicePrefix { get; set; }

		public int? InvoiceSequence { get; set; }

		public string? BusinessVatId { get; set; }

		public string? AccountType { get; set; }

		public string? Metadata { get; set; }
	}
}