namespace Cloud.Models.DTO
{
	public class PropertyDto
	{
		public int Id { get; set; }
		public string City { get; set; } = string.Empty;
		public string State { get; set; } = string.Empty;
		public string ZipCode { get; set; } = string.Empty;
		public int Bedrooms { get; set; }
		public string Amenities { get; set; } = string.Empty;
		public decimal RentAmount { get; set; }
	}
}