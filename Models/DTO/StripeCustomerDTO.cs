namespace Cloud.Models.DTO
{
	public class CreateStripeCustomerDto
	{
		public string UserId { get; set; } = null!;
		public string StripeCustomerId { get; set; } = string.Empty;
	}

	public class UpdateStripeCustomerDto
	{
		public string? StripeCustomerId { get; set; }
	}
}