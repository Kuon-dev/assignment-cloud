using Cloud.Models;

namespace Cloud.Factories
{
	/// <summary>
	/// Factory for creating RentPaymentModel instances
	/// </summary>
	public interface IRentPaymentFactory
	{
		/// <summary>
		/// Creates a new RentPaymentModel instance
		/// </summary>
		/// <param name="tenantId">The ID of the tenant making the payment</param>
		/// <param name="amount">The amount to be paid in cents</param>
		/// <param name="currency">The currency of the payment</param>
		/// <param name="paymentIntentId">The Stripe PaymentIntent ID</param>
		/// <param name="status">The initial status of the payment</param>
		/// <returns>A new RentPaymentModel instance</returns>
		RentPaymentModel Create(Guid tenantId, int amount, string currency, string paymentIntentId, PaymentStatus status);
	}

	public class RentPaymentFactory : IRentPaymentFactory
	{
		public RentPaymentModel Create(Guid tenantId, int amount, string currency, string paymentIntentId, PaymentStatus status)
		{
			return new RentPaymentModel
			{
				TenantId = tenantId,
				Amount = amount,
				Currency = currency,
				PaymentIntentId = paymentIntentId,
				Status = status
			};
		}
	}
}