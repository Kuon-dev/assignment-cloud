// media type model
namespace Cloud.Models{
    public class ApplicationDocumentModel
    {
        public Guid Id { get; set; }
        public Guid RentalApplicationId { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
    }

}
