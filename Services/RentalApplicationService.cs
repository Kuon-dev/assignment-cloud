using Microsoft.EntityFrameworkCore;
using Cloud.Models;
using Cloud.Models.DTO;

namespace Cloud.Services {

  public interface IRentalApplicationService {
	Task<CustomPaginatedResult<RentalApplicationModel>> GetApplicationsAsync(int page, int size);
	Task<RentalApplicationModel> GetApplicationByIdAsync(Guid id);
	Task<RentalApplicationModel> CreateApplicationAsync(CreateRentalApplicationDto applicationDto);
	Task<bool> UpdateApplicationAsync(Guid id, UpdateRentalApplicationDto applicationDto);
	Task<bool> DeleteApplicationAsync(Guid id);
	Task<bool> UploadDocumentsAsync(Guid id, IFormFileCollection files);
	Task<CustomPaginatedResult<RentalApplicationModel>> GetApplicationsByStatusAsync(ApplicationStatus status, int page, int size);
  }

  public class RentalApplicationService : IRentalApplicationService {
	private readonly ApplicationDbContext _context;
	private readonly S3Service _s3Service;

	public RentalApplicationService(ApplicationDbContext context, S3Service s3Service) {
	  _context = context;
	  _s3Service = s3Service;
	}

	public async Task<CustomPaginatedResult<RentalApplicationModel>> GetApplicationsAsync(int page, int size) {
	  var query = _context.RentalApplications.AsNoTracking();
	  var totalCount = await query.CountAsync();

	  var applications = await query
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  return new CustomPaginatedResult<RentalApplicationModel> {
		Items = applications,
		TotalCount = totalCount,
		PageNumber = page,
		PageSize = size
	  };
	}

	public async Task<RentalApplicationModel> GetApplicationByIdAsync(Guid id) {
	  var result = await _context.RentalApplications.FindAsync(id);
	  if (result == null) throw new NotFoundException("Rental application not found.");
	  return result;
	}

	public async Task<RentalApplicationModel> CreateApplicationAsync(CreateRentalApplicationDto applicationDto) {
	  var application = new RentalApplicationModel {
		TenantId = applicationDto.TenantId,
		ListingId = applicationDto.ListingId,
		Status = ApplicationStatus.Pending,
		ApplicationDate = DateTime.UtcNow,
		EmploymentInfo = applicationDto.EmploymentInfo,
		References = applicationDto.References,
		AdditionalNotes = applicationDto.AdditionalNotes
	  };

	  _context.RentalApplications.Add(application);
	  await _context.SaveChangesAsync();

	  return application;
	}

	public async Task<bool> UpdateApplicationAsync(Guid id, UpdateRentalApplicationDto applicationDto) {
	  var application = await _context.RentalApplications.FindAsync(id);

	  if (application == null) {
		return false;
	  }

	  application.Status = applicationDto.Status ?? application.Status;
	  application.EmploymentInfo = applicationDto.EmploymentInfo ?? application.EmploymentInfo;
	  application.References = applicationDto.References ?? application.References;
	  application.AdditionalNotes = applicationDto.AdditionalNotes ?? application.AdditionalNotes;

	  await _context.SaveChangesAsync();
	  return true;
	}

	public async Task<bool> DeleteApplicationAsync(Guid id) {
	  var application = await _context.RentalApplications.FindAsync(id);

	  if (application == null) {
		return false;
	  }

	  _context.RentalApplications.Remove(application);
	  await _context.SaveChangesAsync();
	  return true;
	}

	public async Task<bool> UploadDocumentsAsync(Guid id, IFormFileCollection files) {
	  var application = await _context.RentalApplications.FindAsync(id);

	  if (application == null) {
		return false;
	  }

	  foreach (var file in files) {
		var fileName = $"applications/{id}/{Guid.NewGuid()}_{file.FileName}";
		string contentType = file.ContentType;

		using (var stream = file.OpenReadStream()) {
		  var fileUrl = await _s3Service.UploadFileAsync(stream, fileName, contentType);

		  var document = new ApplicationDocumentModel {
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

	public async Task<CustomPaginatedResult<RentalApplicationModel>> GetApplicationsByStatusAsync(ApplicationStatus status, int page, int size) {
	  var query = _context.RentalApplications
		  .AsNoTracking()
		  .Where(a => a.Status == status);

	  var totalCount = await query.CountAsync();

	  var applications = await query
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  return new CustomPaginatedResult<RentalApplicationModel> {
		Items = applications,
		TotalCount = totalCount,
		PageNumber = page,
		PageSize = size
	  };
	}
  }
}