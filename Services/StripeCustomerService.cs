using Cloud.Models;
using Cloud.Models.DTO;

namespace Cloud.Services
{

	public interface IStripeCustomerService
	{
		Task<StripeCustomerModel> GetStripeCustomerByIdAsync(Guid id);
		Task<StripeCustomerModel> UpdateStripeCustomerAsync(Guid id, UpdateStripeCustomerDto customerDto);
		Task<bool> DeleteStripeCustomerAsync(Guid id);
	}

	public class StripeCustomerService : IStripeCustomerService
	{
		private readonly ApplicationDbContext _context;

		public StripeCustomerService(ApplicationDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<StripeCustomerModel> GetStripeCustomerByIdAsync(Guid id)
		{
			if (_context.StripeCustomers == null)
				throw new InvalidOperationException("StripeCustomers DbSet is not initialized.");

			var stripeCustomer = await _context.StripeCustomers.FindAsync(id);
			if (stripeCustomer == null)
				throw new InvalidOperationException("Stripe customer not found");

			return stripeCustomer;
		}

		public async Task<StripeCustomerModel> UpdateStripeCustomerAsync(Guid id, UpdateStripeCustomerDto stripeCustomerDto)
		{
			if (stripeCustomerDto == null)
				throw new ArgumentNullException(nameof(stripeCustomerDto));

			if (_context.StripeCustomers == null)
				throw new InvalidOperationException("StripeCustomers DbSet is not initialized.");

			var stripeCustomer = await _context.StripeCustomers.FindAsync(id);
			if (stripeCustomer == null)
				throw new InvalidOperationException("Stripe customer not found");

			if (!string.IsNullOrEmpty(stripeCustomerDto.StripeCustomerId))
				stripeCustomer.StripeCustomerId = stripeCustomerDto.StripeCustomerId;

			stripeCustomer.UpdateModifiedProperties(DateTime.UtcNow);
			await _context.SaveChangesAsync();

			return stripeCustomer;
		}

		public async Task<bool> DeleteStripeCustomerAsync(Guid id)
		{
			if (_context.StripeCustomers == null)
				throw new InvalidOperationException("StripeCustomers DbSet is not initialized.");

			var stripeCustomer = await _context.StripeCustomers.FindAsync(id);
			if (stripeCustomer == null)
				return false;

			stripeCustomer.UpdateIsDeleted(DateTime.UtcNow, true);
			await _context.SaveChangesAsync();

			return true;
		}

		private bool StripeCustomerExists(Guid id)
		{
			return _context.StripeCustomers?.Any(e => e.Id == id) ?? false;
		}
	}
}