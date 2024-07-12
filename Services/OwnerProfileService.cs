using Microsoft.EntityFrameworkCore;
/*using Microsoft.Extensions.Logging;*/
using Cloud.Models;

namespace Cloud.Services
{
	public interface IOwnerProfileService
	{
		Task<OwnerModel> CreateOwnerProfileAsync(OwnerModel owner);
		Task<OwnerModel?> GetOwnerProfileAsync(Guid id);
		Task<IEnumerable<OwnerModel>> GetAllOwnerProfilesAsync();
		Task<OwnerModel> UpdateOwnerProfileAsync(OwnerModel owner);
		Task<bool> DeleteOwnerProfileAsync(Guid id);
		Task<OwnerModel> VerifyOwnerAsync(Guid id, string verifiedBy);
	}

	public class OwnerProfileService : IOwnerProfileService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<OwnerProfileService> _logger;

		public OwnerProfileService(ApplicationDbContext context, ILogger<OwnerProfileService> logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<OwnerModel> CreateOwnerProfileAsync(OwnerModel owner)
		{
			try
			{
				_context.Owners.Add(owner);
				await _context.SaveChangesAsync();
				return owner;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while creating owner profile");
				throw;
			}
		}

		public async Task<OwnerModel?> GetOwnerProfileAsync(Guid id)
		{
			try
			{
				return await _context.Owners
					.Include(o => o.User)
					.FirstOrDefaultAsync(o => o.Id == id);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while retrieving owner profile with ID {Id}", id);
				throw;
			}
		}

		public async Task<IEnumerable<OwnerModel>> GetAllOwnerProfilesAsync()
		{
			try
			{
				return await _context.Owners
					.Include(o => o.User)
					.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while retrieving all owner profiles");
				throw;
			}
		}

		public async Task<OwnerModel> UpdateOwnerProfileAsync(OwnerModel owner)
		{
			try
			{
				_context.Entry(owner).State = EntityState.Modified;
				await _context.SaveChangesAsync();
				return owner;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating owner profile with ID {Id}", owner.Id);
				throw;
			}
		}

		public async Task<bool> DeleteOwnerProfileAsync(Guid id)
		{
			try
			{
				var owner = await _context.Owners.FindAsync(id);
				if (owner == null)
					return false;

				_context.Owners.Remove(owner);
				await _context.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting owner profile with ID {Id}", id);
				throw;
			}
		}

		public async Task<OwnerModel> VerifyOwnerAsync(Guid id, string verifiedBy)
		{
			try
			{
				var owner = await _context.Owners.FindAsync(id);
				if (owner == null)
					throw new ArgumentException("Owner profile not found", nameof(id));

				owner.VerificationStatus = OwnerVerificationStatus.Verified;
				owner.VerificationDate = DateTime.UtcNow;
				// You might want to add a VerifiedBy property to OwnerModel if you want to track who verified the owner
				// owner.VerifiedBy = verifiedBy;

				await _context.SaveChangesAsync();
				return owner;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while verifying owner profile with ID {Id}", id);
				throw;
			}
		}
	}
}