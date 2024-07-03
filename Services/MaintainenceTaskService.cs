// MaintenanceTaskService.cs
using Cloud.Models;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Services {
  public class MaintenanceTaskService : IMaintenanceTaskService {
	private readonly ApplicationDbContext _context;

	public MaintenanceTaskService(ApplicationDbContext context) {
	  _context = context;
	}

	public async Task<IEnumerable<MaintenanceTaskModel>> GetAllTasksAsync(int page, int size) {
	  return await _context.MaintenanceTasks
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();
	}

	public async Task<MaintenanceTaskModel> GetTaskByIdAsync(Guid id) {
	  return await _context.MaintenanceTasks.FindAsync(id);
	}

	public async Task<MaintenanceTaskModel> CreateTaskAsync(MaintenanceTaskModel task) {
	  _context.MaintenanceTasks.Add(task);
	  await _context.SaveChangesAsync();
	  return task;
	}

	public async Task<MaintenanceTaskModel> UpdateTaskAsync(MaintenanceTaskModel task) {
	  _context.Entry(task).State = EntityState.Modified;
	  try {
		await _context.SaveChangesAsync();
	  }
	  catch (DbUpdateConcurrencyException) {
		if (!await TaskExists(task.Id))
		  return null;
		throw;
	  }
	  return task;
	}

	public async Task<bool> DeleteTaskAsync(Guid id) {
	  var task = await _context.MaintenanceTasks.FindAsync(id);
	  if (task == null)
		return false;

	  _context.MaintenanceTasks.Remove(task);
	  await _context.SaveChangesAsync();
	  return true;
	}

	public async Task<IEnumerable<MaintenanceTaskModel>> GetTasksByStaffIdAsync(Guid staffId) {
	  return await _context.MaintenanceTasks
		  .Where(t => t.StaffId == staffId)
		  .ToListAsync();
	}

	private async Task<bool> TaskExists(Guid id) {
	  return await _context.MaintenanceTasks.AnyAsync(e => e.Id == id);
	}
  }
}