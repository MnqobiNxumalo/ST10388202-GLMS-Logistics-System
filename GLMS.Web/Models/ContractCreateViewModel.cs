using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public class ContractCreateViewModel
    {
        public int ClientId { get; set; }

        [Required]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Required]
        public string ServiceLevel { get; set; } = string.Empty;

        public string? TermsAndConditions { get; set; }

        // File upload
        public IFormFile? SignedAgreement { get; set; }

        // For dropdown - REMOVE [Required] and make nullable
        public List<Client>? AvailableClients { get; set; }  // Added ? to make nullable
    }
}