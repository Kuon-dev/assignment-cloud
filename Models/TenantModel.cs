// TenantModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models {
  public class TenantModel : BaseEntity {
	[Required]
	public String UserId { get; set; } = null!;

	[ForeignKey("UserId")]
	public UserModel? User { get; set; }

	public Guid? CurrentPropertyId { get; set; }
	[ForeignKey("CurrentPropertyId")]
	public PropertyModel? CurrentProperty { get; set; }

	// Navigation properties
	public ICollection<RentalApplicationModel>? Applications { get; set; }
	public ICollection<RentPaymentModel>? RentPayments { get; set; }
	public ICollection<MaintenanceRequestModel>? MaintenanceRequests { get; set; }
	public ICollection<LeaseModel>? Leases { get; set; }
  }
}