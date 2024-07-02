// Services/IUserService.cs
using Cloud.Models;

namespace Cloud.Services
{
    public interface IUserService
    {
        Task<PropertyModel> GetRentedPropertyAsync(string userId);
        Task<IEnumerable<RentPaymentModel>> GetPaymentHistoryAsync(string userId, int page, int size);
        Task<IEnumerable<MaintenanceRequestModel>> GetMaintenanceRequestsAsync(string userId, int page, int size);
        Task<IEnumerable<RentalApplicationModel>> GetApplicationsAsync(string userId, int page, int size);
    }
}
