// IMaintenanceTaskService.cs
using Cloud.Models;

namespace Cloud.Services {
  public interface IMaintenanceTaskService {
	Task<IEnumerable<MaintenanceTaskModel>> GetAllTasksAsync(int page, int size);
	Task<MaintenanceTaskModel> GetTaskByIdAsync(Guid id);
	Task<MaintenanceTaskModel> CreateTaskAsync(MaintenanceTaskModel task);
	Task<MaintenanceTaskModel> UpdateTaskAsync(MaintenanceTaskModel task);
	Task<bool> DeleteTaskAsync(Guid id);
	Task<IEnumerable<MaintenanceTaskModel>> GetTasksByStaffIdAsync(Guid staffId);
  }
}