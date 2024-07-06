using Cloud.Models.Data;

namespace Cloud.Models
{
	public class ApplicationDocumentModel : BaseEntity
	{
		public Guid RentalApplicationId { get; set; }
		public string FileName { get; set; } = null!;
		public string FilePath { get; set; } = null!;
	}

}