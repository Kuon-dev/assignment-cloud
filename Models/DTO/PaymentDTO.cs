using System.ComponentModel.DataAnnotations;

namespace Cloud.Models.DTO
{
	public class CreatePaymentIntentDto
	{
		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
		public int Amount { get; set; }
	}

	public class CreatePaymentIntentResponseDto
	{
		public string ClientSecret { get; set; } = string.Empty;
	}

	public class ProcessPaymentDto
	{
		[Required]
		public string PaymentIntentId { get; set; } = string.Empty;
	}

	public class CancelPaymentDto
	{
		[Required]
		public string PaymentIntentId { get; set; } = string.Empty;
	}

	public class RentPaymentDto
	{
		public Guid Id { get; set; }
		public int Amount { get; set; }
		public string Currency { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
	}
}