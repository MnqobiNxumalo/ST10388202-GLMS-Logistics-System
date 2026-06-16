using GLMS.Shared.Models;
using Microsoft.AspNetCore.Http;

namespace GLMS.Shared.ViewModels
{
    public class ContractCreateViewModel
    {
        public int ClientId { get; set; }

        public string ContractNumber { get; set; } = string.Empty;

        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);

        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        public string ServiceLevel { get; set; } = string.Empty;

        public string? TermsAndConditions { get; set; }

        // File upload (Microsoft.AspNetCore.Http is needed)
        public IFormFile? SignedAgreement { get; set; }

        // For dropdown
        public List<Client>? AvailableClients { get; set; }
    }
}