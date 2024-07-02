namespace Cloud.Models.DTO {
  public class CreateRentalApplicationDto {
	public Guid TenantId { get; set; }
	public Guid ListingId { get; set; }
	public string? EmploymentInfo { get; set; }
	public string? References { get; set; }
	public string? AdditionalNotes { get; set; }
  }

  public class UpdateRentalApplicationDto {
	public ApplicationStatus? Status { get; set; }
	public string? EmploymentInfo { get; set; }
	public string? References { get; set; }
	public string? AdditionalNotes { get; set; }
  }
}