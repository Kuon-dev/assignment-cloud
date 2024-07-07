using System.ComponentModel.DataAnnotations;

namespace Cloud.Models.DTO
{
	/// <summary>
	/// Data Transfer Object for Media
	/// </summary>
	public class MediaDto
	{
		/// <summary>
		/// Unique identifier for the media
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Name of the file
		/// </summary>
		[Required]
		public string FileName { get; set; } = string.Empty;

		/// <summary>
		/// Path where the file is stored
		/// </summary>
		[Required]
		public string FilePath { get; set; } = string.Empty;

		/// <summary>
		/// Type of the file (e.g., image/jpeg, application/pdf)
		/// </summary>
		[Required]
		public string FileType { get; set; } = string.Empty;

		/// <summary>
		/// Size of the file in bytes
		/// </summary>
		[Required]
		public long FileSize { get; set; }

		/// <summary>
		/// Date and time when the file was uploaded
		/// </summary>
		[Required]
		public DateTime UploadedAt { get; set; }
	}

	/// <summary>
	/// Data Transfer Object for creating a new Media
	/// </summary>
	public class CreateMediaDto
	{
		/// <summary>
		/// The file to be uploaded
		/// </summary>
		[Required(ErrorMessage = "File is required")]
		public IFormFile File { get; set; } = null!;

		/// <summary>
		/// Optional custom file name. If not provided, the original file name will be used.
		/// </summary>
		public string? CustomFileName { get; set; }
	}
}