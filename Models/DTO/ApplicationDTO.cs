namespace Cloud.Models.DTO
{
	public class CreateRentalApplicationDto
	{
		public Guid TenantId { get; set; }
		public Guid ListingId { get; set; }
		public string? EmploymentInfo { get; set; }
		public string? References { get; set; }
		public string? AdditionalNotes { get; set; }
	}

	public class UpdateRentalApplicationDto
	{
		public ApplicationStatus? Status { get; set; }
		public string? EmploymentInfo { get; set; }
		public string? References { get; set; }
		public string? AdditionalNotes { get; set; }
	}

	public class RentalApplicationValidator
	{
		public void ValidateApplication(RentalApplicationModel application)
		{
			if (application.TenantId == Guid.Empty)
			{
				throw new ArgumentException("Tenant ID cannot be empty.", nameof(application.TenantId));
			}

			if (application.ListingId == Guid.Empty)
			{
				throw new ArgumentException("Listing ID cannot be empty.", nameof(application.ListingId));
			}

			if (application.ApplicationDate == default)
			{
				throw new ArgumentException("Application date must be set.", nameof(application.ApplicationDate));
			}

			// Add more validation rules as necessary
		}
	}

}