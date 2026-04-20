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
        //[NotMapped]
        //public IContractState CurrentState { get; set; }
        //replacing the abovve with switch casing to determine the state based on the Status property, which is stored in the db.
        public IContractState CurrentState
        {
            get
            {
                return Status switch
                {
                    "Active" => new Contract_ActiveState(),
                    "Expired" => new Contract_ExpiredState(),
                    "OnHold" => new Contract_OnHoldState(),
                    _ => new Contract_DraftState() // Default state since at that point the contract is still being created by the client and not yet active [typing and probably didnt save it]
                };
            }
        }

        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; }

    }
}

