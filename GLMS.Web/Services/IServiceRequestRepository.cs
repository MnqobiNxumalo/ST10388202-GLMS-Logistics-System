using GLMS.Web.Models;

namespace GLMS.Web.Services
{
    public interface IServiceRequestRepository
    {
        Task<ServiceRequest> GetByIdAsync(int id);
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
        Task<IEnumerable<ServiceRequest>> GetByContractIdAsync(int contractId);
        Task<ServiceRequest> AddAsync(ServiceRequest request);
        Task<ServiceRequest> UpdateAsync(ServiceRequest request);
        Task<bool> DeleteAsync(int id);
    }
}