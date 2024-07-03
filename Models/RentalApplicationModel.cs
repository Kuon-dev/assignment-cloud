// RentalApplicationModel.cs
/*using System;*/
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/*namespace Cloud.Models*/
namespace Cloud.Models {
  public class RentalApplicationModel {
	[Key]
	public Guid Id { get; set; }

	[Required]
	public Guid TenantId { get; set; }

	[ForeignKey("TenantId")]
	public TenantModel? Tenant { get; set; }

	[Required]
	public Guid ListingId { get; set; }

	[ForeignKey("ListingId")]
	public ListingModel? Listing { get; set; }

	public ApplicationStatus Status { get; set; }

	public DateTime ApplicationDate { get; set; }

	public string? EmploymentInfo { get; set; }

	public string? References { get; set; }

	public string? AdditionalNotes { get; set; }
  }

  public enum ApplicationStatus {
	Pending,
	Approved,
	Rejected
  }
}