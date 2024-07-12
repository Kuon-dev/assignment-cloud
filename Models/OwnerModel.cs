using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models
{
	public class OwnerModel : BaseEntity
	{
		[Required]
		public string UserId { get; set; } = null!;

		[ForeignKey("UserId")]
		public UserModel? User { get; set; }

		[Required]
		[MaxLength(255)]
		public string BusinessName { get; set; } = string.Empty;

		[Required]
		[MaxLength(500)]
		public string BusinessAddress { get; set; } = string.Empty;

		[Required]
		[Phone]
		public string BusinessPhone { get; set; } = string.Empty;

		[Required]
		[EmailAddress]
		public string BusinessEmail { get; set; } = string.Empty;

		public string? IdentityDocumentPath { get; set; }

		public DateTime? VerificationDate { get; set; }

		public OwnerVerificationStatus VerificationStatus { get; set; } = OwnerVerificationStatus.Pending;

		[Required]
		[MaxLength(50)]
		public string BankAccountNumber { get; set; } = string.Empty;

		[Required]
		[MaxLength(255)]
		public string BankAccountName { get; set; } = string.Empty;

		[Required]
		[MaxLength(11)]
		public string SwiftBicCode { get; set; } = string.Empty;

		[Required]
		[MaxLength(255)]
		public string BankName { get; set; } = string.Empty;

		// Navigation properties
		public ICollection<PropertyModel>? Properties { get; set; }
	}

	public enum OwnerVerificationStatus
	{
		Pending,
		Verified,
		Rejected
	}
}