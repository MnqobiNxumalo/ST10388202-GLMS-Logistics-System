using GLMS.Shared.Models;
using GLMS.Shared.ViewModels;

namespace GLMS.Web.Services
{
    public interface IApiService
    {
        // Authentication
        Task<string> LoginAsync(string username, string password);

        // Contract methods
        Task<List<Contract>> GetContractsAsync(DateTime? startDate = null, DateTime? endDate = null, ContractStatus? status = null);
        Task<Contract> GetContractByIdAsync(int id);
        Task<Contract> CreateContractAsync(Contract contract);
        Task<bool> UpdateContractAsync(Contract contract);  // ← ADD THIS
        Task<bool> UpdateContractStatusAsync(int id, ContractStatus status);

        // Service Request methods
        Task<List<ServiceRequest>> GetServiceRequestsAsync();
        Task<ServiceRequest> GetServiceRequestByIdAsync(int id);
        Task<ServiceRequest> CreateServiceRequestAsync(ServiceRequest request);

        // Client methods
        Task<List<Client>> GetClientsAsync();
        Task<Client> GetClientByIdAsync(int id);
        Task<Client> CreateClientAsync(Client client);

        // Currency
        Task<decimal> ConvertCurrencyAsync(decimal usdAmount);

        // PDF
        Task<byte[]?> DownloadPdfAsync(int contractId);  // ← ADD THIS

        // Health check
        Task<bool> IsApiHealthy();
    }
}