using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public class ServiceRequestViewModel
    {
        [Required]
        public int ContractId { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        [Display(Name = "Amount (USD)")]
        public decimal AmountUSD { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        // For dropdown display
        public string ContractDisplayName { get; set; }
    }
}