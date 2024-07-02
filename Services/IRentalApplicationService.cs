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
}