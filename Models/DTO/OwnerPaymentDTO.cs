using System.ComponentModel.DataAnnotations;

namespace Cloud.Models.DTO
{
	public class OwnerPaymentDto
	{
		public Guid Id { get; set; }
		public Guid OwnerId { get; set; }
		public Guid PropertyId { get; set; }
		public decimal Amount { get; set; }
		public OwnerPaymentStatus Status { get; set; }
		public DateTime PaymentDate { get; set; }
		public string StripePaymentIntentId { get; set; }

		public OwnerPaymentDto(OwnerPaymentModel payment)
		{
			Id = payment.Id;
			OwnerId = payment.OwnerId;
			PropertyId = payment.PropertyId;
			Amount = payment.Amount;
			Status = payment.Status;
			PaymentDate = payment.PaymentDate;
			StripePaymentIntentId = payment.StripePaymentIntentId;
		}
	}

	public class CreateOwnerPaymentDto
	{
		[Required]
		public Guid OwnerId { get; set; }

		[Required]
		public Guid PropertyId { get; set; }

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
		public decimal Amount { get; set; }
	}

	public class UpdateOwnerPaymentDto
	{
		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
		public decimal Amount { get; set; }

		[Required]
		public OwnerPaymentStatus Status { get; set; }
	}
}