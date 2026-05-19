using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Web.Models
{
    public enum ContractStatus
    {
        Draft,
        Active,
        Expired,
        OnHold
    }

    public class Contract
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }

        [Required]
        [StringLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Required]
        [StringLength(50)]
        public string ServiceLevel { get; set; } = string.Empty;

        [StringLength(500)]
        public string TermsAndConditions { get; set; } = string.Empty;

        // MAKE THIS NULLABLE - add the ?
        public string? PdfFilePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        public bool IsActive()
        {
            return Status == ContractStatus.Active &&
                   StartDate <= DateTime.Today &&
                   EndDate >= DateTime.Today;
        }

        public bool CanCreateServiceRequest()
        {
            return Status == ContractStatus.Active || Status == ContractStatus.Draft;
        }
    }
}