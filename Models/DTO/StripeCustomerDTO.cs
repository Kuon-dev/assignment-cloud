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


	public class OnboardingStatusDto
	{
		public bool IsVerified { get; set; }
		public StripeAccountStatusDto StripeAccountStatus { get; set; } = null!;
	}

	public class StripeAccountStatusDto
	{
		public bool DetailsSubmitted { get; set; }
		public bool PayoutsEnabled { get; set; }
		public List<string> RequiredVerification { get; set; } = new List<string>();
	}

}