// PropertyModel.cs
// MediaModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models
{
	public class MediaModel : BaseEntity
	{
		[Required]
		public String UserId { get; set; } = null!;

		[ForeignKey("UserId")]
		public UserModel? User { get; set; }

		[Required]
		public string FileName { get; set; } = string.Empty;

		[Required]
		public string FilePath { get; set; } = string.Empty;

		[Required]
		public string FileType { get; set; } = string.Empty;

		[Required]
		public long FileSize { get; set; }

		[Required]
		public DateTime UploadedAt { get; set; }
	}
}