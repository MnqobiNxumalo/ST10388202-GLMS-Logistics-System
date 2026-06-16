using System.Linq.Expressions;
using GLMS.Shared.Models;

namespace GLMS.API.Services
{
    public interface IContractRepository
    {
        Task<Contract> GetByIdAsync(int id);
        Task<IEnumerable<Contract>> GetAllAsync();
        Task<IEnumerable<Contract>> FindByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<Contract>> FindByStatusAsync(ContractStatus status);
        Task<IEnumerable<Contract>> SearchContractsAsync(DateTime? startDate, DateTime? endDate, ContractStatus? status);
        Task<Contract> AddAsync(Contract contract);
        Task<Contract> UpdateAsync(Contract contract);
        Task<bool> DeleteAsync(int id);
        Task<bool> ContractExistsAsync(int id);
        Task<bool> IsContractActiveForRequestAsync(int contractId);
    }
}