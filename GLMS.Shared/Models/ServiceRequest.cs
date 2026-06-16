using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Shared.Models
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        InProgress,
        Completed,
        Cancelled
    }

    public class ServiceRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        [ForeignKey("ContractId")]
        public virtual Contract? Contract { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountUSD { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountZAR { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ExchangeRateUsed { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}