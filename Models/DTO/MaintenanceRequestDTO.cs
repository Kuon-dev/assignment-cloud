using System.ComponentModel.DataAnnotations;

namespace Cloud.Models.DTO
{
	/// <summary>
	/// Data Transfer Object for creating a new maintenance request.
	/// </summary>
	public class CreateMaintenanceRequestDto
	{
		/// <summary>
		/// The ID of the property associated with the maintenance request.
		/// </summary>
		[Required]
		public Guid PropertyId { get; set; }

		/// <summary>
		/// The description of the maintenance request.
		/// </summary>
		[Required]
		[StringLength(500, MinimumLength = 10)]
		public string Description { get; set; } = string.Empty;
	}

	/// <summary>
	/// Data Transfer Object for updating an existing maintenance request.
	/// </summary>
	public class UpdateMaintenanceRequestDto
	{
		/// <summary>
		/// The updated description of the maintenance request.
		/// </summary>
		[StringLength(500, MinimumLength = 10)]
		public string? Description { get; set; }

		/// <summary>
		/// The updated status of the maintenance request.
		/// </summary>
		public MaintenanceStatus? Status { get; set; }
	}
}