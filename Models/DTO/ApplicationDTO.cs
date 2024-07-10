using System.ComponentModel.DataAnnotations;

namespace Cloud.Models.DTO
{
	/// <summary>
	/// DTO for creating a new rental application
	/// </summary>
	public class CreateRentalApplicationDto
	{
		[Required(ErrorMessage = "TenantId is required.")]
		public Guid TenantId { get; set; }

		[Required(ErrorMessage = "ListingId is required.")]
		public Guid ListingId { get; set; }

		[MaxLength(500, ErrorMessage = "Employment information cannot exceed 500 characters.")]
		public string? EmploymentInfo { get; set; }

		[MaxLength(500, ErrorMessage = "References cannot exceed 500 characters.")]
		public string? References { get; set; }

		[MaxLength(1000, ErrorMessage = "Additional notes cannot exceed 1000 characters.")]
		public string? AdditionalNotes { get; set; }
	}

	/// <summary>
	/// DTO for updating an existing rental application
	/// </summary>
	public class UpdateRentalApplicationDto
	{
		public ApplicationStatus? Status { get; set; }

		[MaxLength(500, ErrorMessage = "Employment information cannot exceed 500 characters.")]
		public string? EmploymentInfo { get; set; }

		[MaxLength(500, ErrorMessage = "References cannot exceed 500 characters.")]
		public string? References { get; set; }

		[MaxLength(1000, ErrorMessage = "Additional notes cannot exceed 1000 characters.")]
		public string? AdditionalNotes { get; set; }
	}

	/// <summary>
	/// DTO for rental application response
	/// </summary>
	public class RentalApplicationDto
	{
		public Guid Id { get; set; }
		public DateTime ApplicationDate { get; set; }
		public ApplicationStatus Status { get; set; }
		public string? EmploymentInfo { get; set; }
		public string? References { get; set; }
		public string? AdditionalNotes { get; set; }
		public Guid TenantId { get; set; }
		public string TenantFirstName { get; set; } = string.Empty;
		public string TenantLastName { get; set; } = string.Empty;
		public string TenantEmail { get; set; } = string.Empty;
		public string ListingAddress { get; set; } = string.Empty;
		public Guid PropertyId { get; set; } = Guid.Empty;
	}
}