// LeaseModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models {

  public class LeaseModel : BaseEntity {

	[Required]
	public Guid TenantId { get; set; }

	[ForeignKey("TenantId")]
	public TenantModel? Tenant { get; set; }

	[Required]
	public Guid PropertyId { get; set; }

	[ForeignKey("PropertyId")]
	public PropertyModel? PropertyModel { get; set; }

	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }

	[Column(TypeName = "decimal(18,2)")]
	public decimal RentAmount { get; set; }
	[Column(TypeName = "decimal(18,2)")]
	public decimal SecurityDeposit { get; set; }

	public bool IsActive { get; set; }
  }
}