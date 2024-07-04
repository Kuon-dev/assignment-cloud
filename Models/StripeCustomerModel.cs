// StripeCustomerModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models {

  public class StripeCustomerModel : BaseEntity {
	[Required]
	public String UserId { get; set; } = null!;

	[ForeignKey("UserId")]
	public UserModel? User { get; set; }

	[Required]
	public string StripeCustomerId { get; set; } = string.Empty;
  }
}