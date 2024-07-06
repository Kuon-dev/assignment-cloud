// OwnerModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models
{
	public class OwnerModel : BaseEntity
	{
		[Required]
		public String UserId { get; set; } = null!;

		[ForeignKey("UserId")]
		public UserModel? User { get; set; }

		// Navigation properties
		public ICollection<PropertyModel>? Properties { get; set; }
	}
}