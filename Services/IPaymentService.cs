
// IPaymentService.cs
using Cloud.Models;

namespace Cloud.Services {
  /// <summary>
  /// Interface for payment-related operations.
  /// </summary>
  public interface IPaymentService {
	/// <summary>
	/// Get all payments with pagination.
	/// </summary>
	/// <param name="page">The page number.</param>
	/// <param name="size">The number of items per page.</param>
	/// <returns>A paginated list of payments.</returns>
	Task<(IEnumerable<RentPaymentModel> Payments, int TotalCount)> GetAllPaymentsAsync(int page, int size);

	/// <summary>
	/// Get a specific payment by ID.
	/// </summary>
	/// <param name="id">The ID of the payment.</param>
	/// <returns>The payment if found, null otherwise.</returns>
	Task<RentPaymentModel?> GetPaymentByIdAsync(Guid id);

	/// <summary>
	/// Create a new payment.
	/// </summary>
	/// <param name="payment">The payment to create.</param>
	/// <returns>The created payment.</returns>
	Task<RentPaymentModel> CreatePaymentAsync(RentPaymentModel payment);

	/// <summary>
	/// Update an existing payment.
	/// </summary>
	/// <param name="id">The ID of the payment to update.</param>
	/// <param name="payment">The updated payment information.</param>
	/// <returns>The updated payment if found, null otherwise.</returns>
	Task<RentPaymentModel?> UpdatePaymentAsync(Guid id, RentPaymentModel payment);

	/// <summary>
	/// Delete a payment.
	/// </summary>
	/// <param name="id">The ID of the payment to delete.</param>
	/// <returns>True if the payment was deleted, false otherwise.</returns>
	Task<bool> DeletePaymentAsync(Guid id);

	/// <summary>
	/// Get all payments for a specific user with pagination.
	/// </summary>
	/// <param name="userId">The ID of the user.</param>
	/// <param name="page">The page number.</param>
	/// <param name="size">The number of items per page.</param>
	/// <returns>A paginated list of payments for the specified user.</returns>
	Task<(IEnumerable<RentPaymentModel> Payments, int TotalCount)> GetPaymentsByUserIdAsync(string userId, int page, int size);

	/// <summary>
	/// Get all payments for a specific property with pagination.
	/// </summary>
	/// <param name="propertyId">The ID of the property.</param>
	/// <param name="page">The page number.</param>
	/// <param name="size">The number of items per page.</param>
	/// <returns>A paginated list of payments for the specified property.</returns>
	Task<(IEnumerable<RentPaymentModel> Payments, int TotalCount)> GetPaymentsByPropertyIdAsync(Guid propertyId, int page, int size);

	/// <summary>
	/// Handle Stripe webhook events.
	/// </summary>
	/// <param name="json">The JSON payload from Stripe.</param>
	/// <param name="signature">The Stripe signature header.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task HandleStripeWebhookAsync(string json, string signature);
  }
}