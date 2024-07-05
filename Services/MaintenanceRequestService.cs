using Cloud.Models.DTO;
using Cloud.Models;
using Microsoft.EntityFrameworkCore;
using Cloud.Factories;

namespace Cloud.Services {
  /// <summary>
  /// Interface for maintenance request operations.
  /// </summary>
  public interface IMaintenanceRequestService {
	/// <summary>
	/// Gets all maintenance requests with pagination.
	/// </summary>
	/// <param name="page">The page number.</param>
	/// <param name="size">The page size.</param>
	/// <returns>A paginated list of maintenance requests.</returns>
	Task<(IEnumerable<MaintenanceRequestModel> Requests, int TotalCount)> GetAllMaintenanceRequestsAsync(int page, int size);

	/// <summary>
	/// Gets a specific maintenance request by ID.
	/// </summary>
	/// <param name="id">The ID of the maintenance request.</param>
	/// <param name="userId">The ID of the user making the request.</param>
	/// <returns>The maintenance request if found; otherwise, null.</returns>
	Task<MaintenanceRequestModel?> GetMaintenanceRequestByIdAsync(Guid id, string userId);

	/// <summary>
	/// Creates a new maintenance request.
	/// </summary>
	/// <param name="dto">The DTO containing the maintenance request details.</param>
	/// <param name="userId">The ID of the user creating the request.</param>
	/// <returns>The created maintenance request.</returns>
	Task<MaintenanceRequestModel> CreateMaintenanceRequestAsync(CreateMaintenanceRequestDto dto, string userId);

	/// <summary>
	/// Updates an existing maintenance request.
	/// </summary>
	/// <param name="id">The ID of the maintenance request to update.</param>
	/// <param name="dto">The DTO containing the updated maintenance request details.</param>
	/// <param name="userId">The ID of the user updating the request.</param>
	Task UpdateMaintenanceRequestAsync(Guid id, UpdateMaintenanceRequestDto dto, string userId);

	/// <summary>
	/// Deletes a maintenance request.
	/// </summary>
	/// <param name="id">The ID of the maintenance request to delete.</param>
	Task DeleteMaintenanceRequestAsync(Guid id);

	/// <summary>
	/// Uploads images for a maintenance request.
	/// </summary>
	/// <param name="id">The ID of the maintenance request.</param>
	/// <param name="images">The list of images to upload.</param>
	/// <param name="userId">The ID of the user uploading the images.</param>
	/*Task UploadImagesAsync(Guid id, List<IFormFile> images, string userId);*/

	/// <summary>
	/// Gets maintenance requests by status with pagination.
	/// </summary>
	/// <param name="status">The status to filter by.</param>
	/// <param name="page">The page number.</param>
	/// <param name="size">The page size.</param>
	/// <returns>A paginated list of maintenance requests with the specified status.</returns>
	Task<(IEnumerable<MaintenanceRequestModel> Requests, int TotalCount)> GetMaintenanceRequestsByStatusAsync(MaintenanceStatus status, int page, int size);
  }
}

namespace Cloud.Services {
  /// <summary>
  /// Service for handling maintenance request operations.
  /// </summary>
  public class MaintenanceRequestService : IMaintenanceRequestService {
	private readonly ApplicationDbContext _context;
	private readonly MaintenanceRequestFactory _factory;
	private readonly S3Service _s3Service;

	public MaintenanceRequestService(ApplicationDbContext context, MaintenanceRequestFactory factory, S3Service s3Service) {
	  _context = context ?? throw new ArgumentNullException(nameof(context));
	  _factory = factory ?? throw new ArgumentNullException(nameof(factory));
	  _s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
	}

	/// <inheritdoc/>
	public async Task<(IEnumerable<MaintenanceRequestModel> Requests, int TotalCount)> GetAllMaintenanceRequestsAsync(int page, int size) {
	  var requests = await _context.MaintenanceRequests
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  var totalCount = await _context.MaintenanceRequests.CountAsync();

	  return (requests, totalCount);
	}

	/// <inheritdoc/>
	public async Task<MaintenanceRequestModel?> GetMaintenanceRequestByIdAsync(Guid id, string userId) {
	  var request = await _context.MaintenanceRequests
		  .Include(r => r.Tenant)
		  .FirstOrDefaultAsync(r => r.Id == id);

	  if (request == null) {
		return null;
	  }

	  var user = await _context.Users.FindAsync(userId);
	  if (user == null) {
		throw new InvalidOperationException("User not found.");
	  }

	  // Check if the user is the tenant who created the request or an admin/owner
	  if (request.TenantId.ToString() != userId && user.Role != UserRole.Admin && user.Role != UserRole.Owner) {
		throw new UnauthorizedAccessException("You do not have permission to view this maintenance request.");
	  }

	  return request;
	}

	/// <inheritdoc/>
	public async Task<MaintenanceRequestModel> CreateMaintenanceRequestAsync(CreateMaintenanceRequestDto dto, string userId) {
	  if (_context.Tenants == null || _context.Properties == null || _context.MaintenanceRequests == null) {
		throw new InvalidOperationException("ApplicationDbContext is null.");
	  }
	  var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.UserId == userId);
	  if (tenant == null) {
		throw new InvalidOperationException("User is not a tenant.");
	  }

	  var property = await _context.Properties.FindAsync(dto.PropertyId);
	  if (property == null) {
		throw new InvalidOperationException("Property not found.");
	  }

	  var request = await _factory.CreateRequestAsync(tenant.Id, dto.PropertyId, dto.Description, MaintenanceStatus.Pending);

	  return request;
	}

	/// <inheritdoc/>
	public async Task UpdateMaintenanceRequestAsync(Guid id, UpdateMaintenanceRequestDto dto, string userId) {
	  if (_context.MaintenanceRequests == null) {
		throw new InvalidOperationException("ApplicationDbContext is null.");
	  }

	  var request = await _context.MaintenanceRequests
		  .Include(r => r.Tenant)
		  .FirstOrDefaultAsync(r => r.Id == id);

	  if (request == null) {
		throw new NotFoundException("Maintenance request not found.");
	  }

	  var user = await _context.Users.FindAsync(userId);
	  if (user == null) {
		throw new InvalidOperationException("User not found.");
	  }

	  // Check if the user is the tenant who created the request or an admin/owner
	  if (request.TenantId.ToString() != userId && user.Role != UserRole.Admin && user.Role != UserRole.Owner) {
		throw new UnauthorizedAccessException("You do not have permission to update this maintenance request.");
	  }

	  // Update fields if provided
	  if (!string.IsNullOrEmpty(dto.Description)) {
		request.Description = dto.Description;
	  }

	  if (dto.Status.HasValue) {
		// Only allow status updates by admin or owner
		if (user.Role != UserRole.Admin && user.Role != UserRole.Owner) {
		  throw new UnauthorizedAccessException("Only admins or owners can update the status of a maintenance request.");
		}
		request.Status = dto.Status.Value;
	  }

	  request.UpdateModifiedProperties(DateTime.UtcNow);
	  await _context.SaveChangesAsync();
	}

	/// <inheritdoc/>
	public async Task DeleteMaintenanceRequestAsync(Guid id) {
	  if (_context.Tenants == null || _context.Properties == null || _context.MaintenanceRequests == null) {
		throw new InvalidOperationException("ApplicationDbContext is null.");
	  }
	  var request = await _context.MaintenanceRequests.FindAsync(id);
	  if (request == null) {
		throw new NotFoundException("Maintenance request not found.");
	  }

	  _context.MaintenanceRequests.Remove(request);
	  await _context.SaveChangesAsync();
	}

	/// <inheritdoc/>
	/*public async Task UploadImagesAsync(Guid id, List<IFormFile> images, string userId)*/
	/*{*/
	/*    var request = await _context.MaintenanceRequests*/
	/*        .Include(r => r.Tenant)*/
	/*        .FirstOrDefaultAsync(r => r.Id == id);*/
	/**/
	/*    if (request == null)*/
	/*    {*/
	/*        throw new NotFoundException("Maintenance request not found.");*/
	/*    }*/
	/**/
	/*    if (request.TenantId.ToString() != userId)*/
	/*    {*/
	/*        throw new UnauthorizedAccessException("You do not have permission to upload images for this maintenance request.");*/
	/*    }*/
	/**/
	/*    var uploadedImageUrls = new List<string>();*/
	/**/
	/*    foreach (var image in images)*/
	/*    {*/
	/*        if (image.Length > 0)*/
	/*        {*/
	/*            using (var stream = image.OpenReadStream())*/
	/*            {*/
	/*                var fileName = $"maintenance_requests/{id}/{Guid.NewGuid()}_{image.FileName}";*/
	/*                var imageUrl = await _s3Service.UploadFileAsync(stream, fileName, image.ContentType);*/
	/*                uploadedImageUrls.Add(imageUrl);*/
	/*            }*/
	/*        }*/
	/*    }*/
	/**/
	/*    // Update the MaintenanceRequestModel with the new image URLs*/
	/*    if (request.ImageUrls == null)*/
	/*    {*/
	/*        request.ImageUrls = new List<string>();*/
	/*    }*/
	/*    request.ImageUrls.AddRange(uploadedImageUrls);*/
	/**/
	/*    request.UpdateModifiedProperties(DateTime.UtcNow);*/
	/*    await _context.SaveChangesAsync();*/
	/*}*/
	/// <inheritdoc/>
	public async Task<(IEnumerable<MaintenanceRequestModel> Requests, int TotalCount)> GetMaintenanceRequestsByStatusAsync(MaintenanceStatus status, int page, int size) {
	  var requests = await _context.MaintenanceRequests
		  .Where(r => r.Status == status)
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  var totalCount = await _context.MaintenanceRequests.CountAsync(r => r.Status == status);

	  return (requests, totalCount);
	}
  }

  /// <summary>
  /// Exception thrown when a requested resource is not found.
  /// </summary>
  public class NotFoundException : Exception {
	public NotFoundException(string message) : base(message) { }
  }
}