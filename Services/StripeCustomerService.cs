using Cloud.Models;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Services {

  public interface IStripeCustomerService {
	Task<StripeCustomerModel> CreateStripeCustomerAsync(StripeCustomerModel customer);
	Task<StripeCustomerModel?> GetStripeCustomerAsync(Guid id);
	Task<StripeCustomerModel?> UpdateStripeCustomerAsync(StripeCustomerModel customer);
	Task<bool> DeleteStripeCustomerAsync(Guid id);
  }

  public class StripeCustomerService : IStripeCustomerService {
	private readonly ApplicationDbContext _context;

	public StripeCustomerService(ApplicationDbContext context) {
	  _context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async Task<StripeCustomerModel> CreateStripeCustomerAsync(StripeCustomerModel customer) {
	  if (customer == null)
		throw new ArgumentNullException(nameof(customer));

	  _context.StripeCustomers.Add(customer);
	  await _context.SaveChangesAsync();
	  return customer;
	}

	public async Task<StripeCustomerModel?> GetStripeCustomerAsync(Guid id) {
	  if (_context.StripeCustomers == null)
		throw new InvalidOperationException("StripeCustomers DbSet is not initialized.");

	  return await _context.StripeCustomers.FindAsync(id);
	}

	public async Task<StripeCustomerModel?> UpdateStripeCustomerAsync(StripeCustomerModel customer) {
	  if (customer == null)
		throw new ArgumentNullException(nameof(customer));

	  if (_context.StripeCustomers == null)
		throw new InvalidOperationException("StripeCustomers DbSet is not initialized.");

	  _context.Entry(customer).State = EntityState.Modified;
	  try {
		await _context.SaveChangesAsync();
	  }
	  catch (DbUpdateConcurrencyException) {
		if (!StripeCustomerExists(customer.Id))
		  return null;
		throw;
	  }
	  return customer;
	}

	public async Task<bool> DeleteStripeCustomerAsync(Guid id) {
	  if (_context.StripeCustomers == null)
		throw new InvalidOperationException("StripeCustomers DbSet is not initialized.");

	  var customer = await _context.StripeCustomers.FindAsync(id);
	  if (customer == null)
		return false;

	  _context.StripeCustomers.Remove(customer);
	  await _context.SaveChangesAsync();
	  return true;
	}

	private bool StripeCustomerExists(Guid id) {
	  return _context.StripeCustomers?.Any(e => e.Id == id) ?? false;
	}
  }
}