/*using System;*/
/*using System.Collections.Generic;*/
/*using System.Linq;*/
/*using System.Threading.Tasks;*/
using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Models.DTO
{
	/// <summary>
	/// Data Transfer Object for Owner information, including user details
	/// </summary>
	public class OwnerDto
	{
		public Guid Id { get; set; }
		public string UserId { get; set; } = string.Empty;
		public UserInfoDto UserInfo { get; set; } = null!;
		public ICollection<PropertyDto>? Properties { get; set; }
	}

	/// <summary>
	/// Data Transfer Object for creating or updating an Owner
	/// </summary>
	public class OwnerCreateUpdateDto
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? ProfilePictureUrl { get; set; }
	}
}

namespace Cloud.Services
{
	/// <summary>
	/// Interface for Owner-related operations
	/// </summary>
	public interface IOwnerService
	{
		Task<CustomPaginatedResult<OwnerDto>> GetOwnersAsync(PaginationParams paginationParams);
		Task<OwnerDto?> GetOwnerByIdAsync(Guid id);
		Task<OwnerDto> CreateOwnerAsync(OwnerCreateUpdateDto ownerDto);
		Task<OwnerDto> UpdateOwnerAsync(Guid id, OwnerCreateUpdateDto ownerDto);
		Task<bool> SoftDeleteOwnerAsync(Guid id);
		Task<IEnumerable<PropertyModel>> GetOwnerPropertiesAsync(Guid ownerId);
	}

	/// <summary>
	/// Service implementation for Owner-related operations
	/// </summary>
	public class OwnerService : IOwnerService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<OwnerService> _logger;

		public OwnerService(ApplicationDbContext context, ILogger<OwnerService> logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Retrieves a paginated list of owners
		/// </summary>
		/// <param name="paginationParams">Pagination parameters</param>
		/// <returns>A paginated result of OwnerDto objects</returns>
		public async Task<CustomPaginatedResult<OwnerDto>> GetOwnersAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.Owners
				.AsNoTracking()
				.Where(o => !o.IsDeleted)
				.Include(o => o.User)
				.Select(o => new OwnerDto
				{
					Id = o.Id,
					UserId = o.UserId,
					UserInfo = new UserInfoDto
					{
						Id = Guid.Parse(o.UserId),
						FirstName = o.User!.FirstName,
						LastName = o.User!.LastName,
						Role = o.User!.Role,
						IsVerified = o.User!.IsVerified,
						ProfilePictureUrl = o.User!.ProfilePictureUrl,
						Owner = new OwnerInfoDto { Id = o.Id }
					},
				});

			var totalCount = await query.CountAsync();
			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.ToListAsync();

			return new CustomPaginatedResult<OwnerDto>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}

		/// <summary>
		/// Retrieves an owner by their ID
		/// </summary>
		/// <param name="id">The ID of the owner</param>
		/// <returns>An OwnerDto object if found, null otherwise</returns>
		public async Task<OwnerDto?> GetOwnerByIdAsync(Guid id)
		{
			var owner = await _context.Owners
				.Include(o => o.User)
				.Include(o => o.Properties)
				.FirstOrDefaultAsync(o => o.Id == id);

			if (owner == null)
			{
				return null;
			}

			return new OwnerDto
			{
				Id = owner.Id,
				UserId = owner.UserId,
				UserInfo = new UserInfoDto
				{
					Id = Guid.Parse(owner.UserId),
					FirstName = owner.User!.FirstName,
					LastName = owner.User!.LastName,
					Role = owner.User!.Role,
					IsVerified = owner.User!.IsVerified,
					ProfilePictureUrl = owner.User!.ProfilePictureUrl,
					Owner = new OwnerInfoDto { Id = owner.Id }
				},
				Properties = owner.Properties == null ? new List<PropertyDto>() : owner.Properties.Select(p => new PropertyDto
				{
					Id = p.Id,
					Description = p.Description,
					OwnerId = p.OwnerId,
					ImageUrls = p.ImageUrls,
					Address = p.Address,
					City = p.City,
					State = p.State,
					ZipCode = p.ZipCode,
					PropertyType = p.PropertyType,
					Bedrooms = p.Bedrooms,
					Bathrooms = p.Bathrooms,
					RentAmount = p.RentAmount,
					Amenities = p.Amenities,
					IsAvailable = p.IsAvailable,
					RoomType = p.RoomType,
					CreatedAt = p.CreatedAt,
					UpdatedAt = p.UpdatedAt
				}).ToList()

			};
		}

		/// <summary>
		/// Creates a new owner
		/// </summary>
		/// <param name="ownerDto">The DTO containing owner information</param>
		/// <returns>The created OwnerDto</returns>
		public async Task<OwnerDto> CreateOwnerAsync(OwnerCreateUpdateDto ownerDto)
		{
			var user = new UserModel
			{
				FirstName = ownerDto.FirstName,
				LastName = ownerDto.LastName,
				Email = ownerDto.Email,
				UserName = ownerDto.Email,
				Role = UserRole.Owner,
				ProfilePictureUrl = ownerDto.ProfilePictureUrl
			};

			var owner = new OwnerModel
			{
				User = user
			};

			_context.Users.Add(user);
			_context.Owners.Add(owner);
			await _context.SaveChangesAsync();

			return new OwnerDto
			{
				Id = owner.Id,
				UserId = owner.UserId,
				UserInfo = new UserInfoDto
				{
					Id = Guid.Parse(user.Id),
					FirstName = user.FirstName,
					LastName = user.LastName,
					Role = user.Role,
					IsVerified = user.IsVerified,
					ProfilePictureUrl = user.ProfilePictureUrl,
					Owner = new OwnerInfoDto { Id = owner.Id }
				},
				Properties = owner.Properties == null ? new List<PropertyDto>() : owner.Properties.Select(p => new PropertyDto
				{
					Id = p.Id,
					Description = p.Description,
					OwnerId = p.OwnerId,
					ImageUrls = p.ImageUrls,
					Address = p.Address,
					City = p.City,
					State = p.State,
					ZipCode = p.ZipCode,
					PropertyType = p.PropertyType,
					Bedrooms = p.Bedrooms,
					Bathrooms = p.Bathrooms,
					RentAmount = p.RentAmount,
					Amenities = p.Amenities,
					IsAvailable = p.IsAvailable,
					RoomType = p.RoomType,
					CreatedAt = p.CreatedAt,
					UpdatedAt = p.UpdatedAt
				}).ToList()
			};
		}

		/// <summary>
		/// Updates an existing owner
		/// </summary>
		/// <param name="id">The ID of the owner to update</param>
		/// <param name="ownerDto">The DTO containing updated owner information</param>
		/// <returns>The updated OwnerDto</returns>
		public async Task<OwnerDto> UpdateOwnerAsync(Guid id, OwnerCreateUpdateDto ownerDto)
		{
			var owner = await _context.Owners
				.Include(o => o.User)
				.FirstOrDefaultAsync(o => o.Id == id);

			if (owner == null)
			{
				throw new KeyNotFoundException($"Owner with ID {id} not found.");
			}

			owner.User!.FirstName = ownerDto.FirstName;
			owner.User!.LastName = ownerDto.LastName;
			owner.User!.Email = ownerDto.Email;
			owner.User!.UserName = ownerDto.Email;
			owner.User!.ProfilePictureUrl = ownerDto.ProfilePictureUrl;

			await _context.SaveChangesAsync();

			return new OwnerDto
			{
				Id = owner.Id,
				UserId = owner.UserId,
				UserInfo = new UserInfoDto
				{
					Id = Guid.Parse(owner.UserId),
					FirstName = owner.User!.FirstName,
					LastName = owner.User!.LastName,
					Role = owner.User!.Role,
					IsVerified = owner.User!.IsVerified,
					ProfilePictureUrl = owner.User!.ProfilePictureUrl,
					Owner = new OwnerInfoDto { Id = owner.Id }
				},
			};
		}

		/// <summary>
		/// Soft deletes an owner
		/// </summary>
		/// <param name="id">The ID of the owner to delete</param>
		/// <returns>True if the owner was successfully deleted, false otherwise</returns>
		public async Task<bool> SoftDeleteOwnerAsync(Guid id)
		{
			var owner = await _context.Owners.FindAsync(id);
			if (owner == null)
			{
				return false;
			}

			owner.UpdateIsDeleted(DateTime.UtcNow, true);
			await _context.SaveChangesAsync();
			return true;
		}

		/// <summary>
		/// Retrieves properties belonging to an owner
		/// </summary>
		/// <param name="ownerId">The ID of the owner</param>
		/// <returns>A collection of PropertyModel objects</returns>
		public async Task<IEnumerable<PropertyModel>> GetOwnerPropertiesAsync(Guid ownerId)
		{
			var owner = await _context.Owners
				.Include(o => o.Properties)
				.FirstOrDefaultAsync(o => o.Id == ownerId);

			if (owner == null || owner.Properties == null)
			{
				return Enumerable.Empty<PropertyModel>();
			}

			return owner.Properties;
		}
	}
}