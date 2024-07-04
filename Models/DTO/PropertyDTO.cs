using System.ComponentModel.DataAnnotations;

namespace Cloud.Models.DTO {
  public class PropertyDto
  {
	  public int Id { get; set; }
	  public string City { get; set; }
	  public string State { get; set; }
	  public string ZipCode { get; set; }
	  public int Bedrooms { get; set; }
	  public string Amenities { get; set; }
	  public decimal RentAmount { get; set; }
  }
}
