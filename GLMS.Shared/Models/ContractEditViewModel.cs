using GLMS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLMS.Shared.Models
{
    public class ContractEditViewModel
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; }
        public string ServiceLevel { get; set; } = string.Empty;
        public string? TermsAndConditions { get; set; }
        public List<Client>? AvailableClients { get; set; }
    }
}
