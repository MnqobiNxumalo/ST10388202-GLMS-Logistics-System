using System.ComponentModel.DataAnnotations;

namespace GLMS.Shared.ViewModels
{
    public class ServiceRequestViewModel
    {
        [Required]
        public int ContractId { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999.99)]
        [Display(Name = "Amount (USD)")]
        public decimal AmountUSD { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        // For display only
        public string? ContractDisplayName { get; set; }
    }
}