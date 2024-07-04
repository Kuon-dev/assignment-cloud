// ActivityLogModel.cs
using System.ComponentModel.DataAnnotations;
using Cloud.Models.Data;

namespace Cloud.Models {
  public class ActivityLogModel : BaseEntity {
	[Required]
	public Guid UserId { get; set; }

	[Required]
	public string Action { get; set; } = null!;

	[Required]
	public DateTime Timestamp { get; set; }

	public string Details { get; set; } = string.Empty;
  }
}