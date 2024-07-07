using Microsoft.EntityFrameworkCore;
using Cloud.Models;
using Cloud.Models.DTO;
using Cloud.Factories;
using Cloud.Models.Validator;

namespace Cloud.Services
{
	/// <summary>
	/// Service for managing rental applications.
	/// </summary>
	public interface IRentalApplicationService
	{
		Task<CustomPaginatedResult<RentalApplicationDto>> GetApplicationsAsync(int page, int size);
		Task<RentalApplicationDto> GetApplicationByIdAsync(Guid id);
		Task<RentalApplicationModel> CreateApplicationAsync(CreateRentalApplicationDto applicationDto);
		Task<bool> UpdateApplicationAsync(Guid id, UpdateRentalApplicationDto applicationDto);
		Task<bool> DeleteApplicationAsync(Guid id);
		Task<bool> UploadDocumentsAsync(Guid id, IFormFileCollection files);
		Task<CustomPaginatedResult<RentalApplicationModel>> GetApplicationsByStatusAsync(ApplicationStatus status, int page, int size);
	}

	/// <summary>
	/// Implementation of the rental application service.
	/// </summary>
	public class RentalApplicationService : IRentalApplicationService
	{
		private readonly ApplicationDbContext _context;
		private readonly S3Service _s3Service;
		private readonly RentalApplicationFactory _applicationFactory;
		private readonly RentalApplicationValidator _applicationValidator;

		/// <summary>
		/// Initializes a new instance of the RentalApplicationService class.
		/// </summary>
		/// <param name="context">The database context.</param>
		/// <param name="s3Service">The S3 service for file uploads.</param>
		/// <param name="applicationFactory">The factory for creating rental applications.</param>
		/// <param name="applicationValidator">The validator for rental applications.</param>
		public RentalApplicationService(
			ApplicationDbContext context,
			S3Service s3Service,
			RentalApplicationFactory applicationFactory,
			RentalApplicationValidator applicationValidator)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
			_applicationFactory = applicationFactory ?? throw new ArgumentNullException(nameof(applicationFactory));
			_applicationValidator = applicationValidator ?? throw new ArgumentNullException(nameof(applicationValidator));
		}

		/// <summary>
		/// Gets a paginated list of rental applications with associated user and tenant information.
		/// </summary>
		/// <param name="page">The page number.</param>
		/// <param name="size">The page size.</param>
		/// <returns>A paginated result of rental applications with user and tenant details.</returns>
		public async Task<CustomPaginatedResult<RentalApplicationDto>> GetApplicationsAsync(int page, int size)
		{
			/// <summary>
			/// Retrieves a paginated list of rental applications
			/// </summary>
			/// <param name="page">The page number</param>
			/// <param name="size">The number of items per page</param>
			/// <returns>A CustomPaginatedResult of RentalApplicationDto objects</returns>
			var query = _context.RentalApplications
				.AsNoTracking()
				.Select(a => new RentalApplicationDto
				{
					Id = a.Id,
					ApplicationDate = a.ApplicationDate,
					Status = a.Status,
					EmploymentInfo = a.EmploymentInfo,
					References = a.References,
					AdditionalNotes = a.AdditionalNotes,
					TenantId = a.TenantId,
					TenantEmail = a.Tenant != null && a.Tenant.User != null && a.Tenant.User.Email != null ? a.Tenant.User.Email : "",
					TenantFirstName = a.Tenant != null && a.Tenant.User != null && a.Tenant.User.FirstName != null ? a.Tenant.User.FirstName : "",
					TenantLastName = a.Tenant != null && a.Tenant.User != null && a.Tenant.User.LastName != null ? a.Tenant.User.LastName : ""
				})
				.OrderByDescending(a => a.ApplicationDate);

			var totalCount = await query.CountAsync();
			var applications = await query
				.Skip((page - 1) * size)
				.Take(size)
				.ToListAsync();

			return new CustomPaginatedResult<RentalApplicationDto>
			{
				Items = applications,
				TotalCount = totalCount,
				PageNumber = page,
				PageSize = size
			};
		}

		/// <summary>
		/// Gets a rental application by its ID, including associated user and tenant information.
		/// </summary>
		/// <param name="id">The ID of the rental application.</param>
		/// <returns>The rental application with user and tenant details.</returns>
		/// <exception cref="NotFoundException">Thrown when the application is not found.</exception>
		public async Task<RentalApplicationDto> GetApplicationByIdAsync(Guid id)
		{
			var query = _context.RentalApplications
				.AsNoTracking()
				// where tenant user email is not null
				.Select(a => new RentalApplicationDto
				{
					Id = a.Id,
					ApplicationDate = a.ApplicationDate,
					Status = a.Status,
					EmploymentInfo = a.EmploymentInfo,
					References = a.References,
					AdditionalNotes = a.AdditionalNotes,
					TenantId = a.TenantId,
					TenantEmail = a.Tenant != null && a.Tenant.User != null && a.Tenant.User.Email != null ? a.Tenant.User.Email : "",
					TenantFirstName = a.Tenant != null && a.Tenant.User != null && a.Tenant.User.FirstName != null ? a.Tenant.User.FirstName : "",
					TenantLastName = a.Tenant != null && a.Tenant.User != null && a.Tenant.User.LastName != null ? a.Tenant.User.LastName : ""
				})
				.FirstOrDefaultAsync(a => a.Id == id);

			var result = await query;
			if (result == null) throw new NotFoundException("Rental application not found.");
			return result;
		}

		/// <summary>
		/// Creates a new rental application.
		/// </summary>
		/// <param name="applicationDto">The DTO containing the application details.</param>
		/// <returns>The created rental application.</returns>
		public async Task<RentalApplicationModel> CreateApplicationAsync(CreateRentalApplicationDto applicationDto)
		{
			try
			{
				// Validate the DTO
				_applicationValidator.ValidateCreateDto(applicationDto);

				// Create the application using the factory
				var application = await _applicationFactory.CreateApplicationAsync(
					applicationDto.TenantId,
					applicationDto.ListingId,
					ApplicationStatus.Pending,
					DateTime.UtcNow,
					applicationDto.EmploymentInfo,
					applicationDto.References,
					applicationDto.AdditionalNotes
				);

				// Validate the created application
				_applicationValidator.ValidateApplication(application);

				return application;
			}
			catch (ValidationException)
			{
				// Log the validation errors
				/*_logger.LogError("Validation failed: {@Errors}", ex.Errors);*/
				throw;
			}
		}
		/// <summary>
		/// Updates an existing rental application.
		/// </summary>
		/// <param name="id">The ID of the application to update.</param>
		/// <param name="applicationDto">The DTO containing the updated details.</param>
		/// <returns>True if the update was successful, false otherwise.</returns>
		public async Task<bool> UpdateApplicationAsync(Guid id, UpdateRentalApplicationDto applicationDto)
		{
			var application = await _context.RentalApplications.FindAsync(id);

			if (application == null)
			{
				return false;
			}

			application.Status = applicationDto.Status ?? application.Status;
			application.EmploymentInfo = applicationDto.EmploymentInfo ?? application.EmploymentInfo;
			application.References = applicationDto.References ?? application.References;
			application.AdditionalNotes = applicationDto.AdditionalNotes ?? application.AdditionalNotes;

			await _context.SaveChangesAsync();
			return true;
		}

		/// <summary>
		/// Deletes a rental application.
		/// </summary>
		/// <param name="id">The ID of the application to delete.</param>
		/// <returns>True if the deletion was successful, false otherwise.</returns>
		public async Task<bool> DeleteApplicationAsync(Guid id)
		{
			var application = await _context.RentalApplications.FindAsync(id);

			if (application == null)
			{
				return false;
			}

			_context.RentalApplications.Remove(application);
			await _context.SaveChangesAsync();
			return true;
		}

		/// <summary>
		/// Uploads documents for a rental application.
		/// </summary>
		/// <param name="id">The ID of the application.</param>
		/// <param name="files">The collection of files to upload.</param>
		/// <returns>True if the upload was successful, false otherwise.</returns>
		public async Task<bool> UploadDocumentsAsync(Guid id, IFormFileCollection files)
		{
			var application = await _context.RentalApplications.FindAsync(id);

			if (application == null)
			{
				return false;
			}

			foreach (var file in files)
			{
				var fileName = $"applications/{id}/{Guid.NewGuid()}_{file.FileName}";
				string contentType = file.ContentType;

				using (var stream = file.OpenReadStream())
				{
					var fileUrl = await _s3Service.UploadFileAsync(stream, fileName, contentType);

					var document = new ApplicationDocumentModel
					{
						RentalApplicationId = id,
						FileName = file.FileName,
						FilePath = fileUrl,
					};

					document.UpdateCreationProperties(DateTime.UtcNow);
					_context.ApplicationDocuments.Add(document);
				}
			}

			await _context.SaveChangesAsync();
			return true;
		}

		/// <summary>
		/// Gets a paginated list of rental applications by status.
		/// </summary>
		/// <param name="status">The status to filter by.</param>
		/// <param name="page">The page number.</param>
		/// <param name="size">The page size.</param>
		/// <returns>A paginated result of rental applications.</returns>
		public async Task<CustomPaginatedResult<RentalApplicationModel>> GetApplicationsByStatusAsync(ApplicationStatus status, int page, int size)
		{
			var query = _context.RentalApplications
				.AsNoTracking()
				.Where(a => a.Status == status);

			var totalCount = await query.CountAsync();

			var applications = await query
				.Skip((page - 1) * size)
				.Take(size)
				.ToListAsync();

			return new CustomPaginatedResult<RentalApplicationModel>
			{
				Items = applications,
				TotalCount = totalCount,
				PageNumber = page,
				PageSize = size
			};
		}
	}
}