using ST10448420_TechMove_GLMS.Patterns.State;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ST10448420_TechMove_GLMS.Models
{
    public class Contract
    {
        [Key]
        public int ContractID { get; set; }

        [Required]
        public int ClientID { get; set; }
        public virtual Client Client { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        [Required]
        [StringLength(50)]
        public string Status { get; set; }
        [Required]
        [StringLength(50)]
        public string ServiceLevel { get; set; }

        // Path to the PDF stored on the server (UUID renamed)
        [Required]
        public string SignedAgreementFilePath { get; set; }

        // Logic hook for the State Pattern (Not mapped to DB)
        [NotMapped]
        public IContractState CurrentState { get; set; }

        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; }
    }
}

