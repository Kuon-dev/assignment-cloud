// AdminModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models
{
	public class AdminModel : BaseEntity
	{
		[Required]
		public string UserId { get; set; } = null!;

		[ForeignKey("UserId")]
		public UserModel? User { get; set; }
	}
}