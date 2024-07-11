namespace Cloud.Models.DTO
{
	public class LeaseDto
	{
		public Guid TenantId { get; set; }
		public Guid PropertyId { get; set; }
		public PropertyDto? Property { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public decimal RentAmount { get; set; }
		public decimal SecurityDeposit { get; set; }
		public bool IsActive { get; set; }
	}

	public class LeaseValidator
	{
		/// <summary>
		/// Validates the lease model.
		/// </summary>
		/// <param name="lease">The lease model to validate.</param>
		public void ValidateLease(LeaseModel lease)
		{
			if (lease.TenantId == Guid.Empty)
			{
				throw new ArgumentException("Tenant ID cannot be empty.", nameof(lease.TenantId));
			}

			if (lease.StartDate >= lease.EndDate)
			{
				throw new ArgumentException("Start date must be before end date.", nameof(lease.StartDate));
			}

			if (lease.StartDate < DateTime.UtcNow.Date)
			{
				throw new ArgumentException("Start date cannot be in the past.", nameof(lease.StartDate));
			}

			if ((lease.EndDate - lease.StartDate).TotalDays < 30)
			{
				throw new ArgumentException("Lease duration must be at least 30 days.", nameof(lease.EndDate));
			}

			if (lease.RentAmount <= 0)
			{
				throw new ArgumentException("Rent amount must be greater than zero.", nameof(lease.RentAmount));
			}

			if (lease.SecurityDeposit < 0)
			{
				throw new ArgumentException("Security deposit cannot be negative.", nameof(lease.SecurityDeposit));
			}
		}
	}

	public class LeaseWithTenantNameDto : LeaseDto
	{
		public string TenantFirstName { get; set; } = string.Empty;
		public string TenantLastName { get; set; } = string.Empty;
		public string TenantEmail { get; set; } = string.Empty;
	}
}