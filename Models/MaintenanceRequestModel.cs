// MaintenanceRequestModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models {

  public class MaintenanceRequestModel : BaseEntity {
	[Required]
	public Guid TenantId { get; set; }

	[ForeignKey("TenantId")]
	public TenantModel? Tenant { get; set; }

	[Required]
	public Guid PropertyId { get; set; }

	[ForeignKey("PropertyId")]
	public PropertyModel? Property { get; set; }

	[Required]
	public string Description { get; set; } = string.Empty;

	public MaintenanceStatus Status { get; set; }
	/*public List<String> */

	// Navigation properties
	public ICollection<MaintenanceTaskModel>? MaintenanceTasks { get; set; }
  }

  public enum MaintenanceStatus {
	Pending,
	InProgress,
	Completed,
	Cancelled
  }
}