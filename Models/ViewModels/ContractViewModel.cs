using System.ComponentModel.DataAnnotations;

namespace ST10448420_TechMove_GLMS.Models.ViewModels
{
    public class ContractViewModel
    {
        public int ContractID { get; set; }

        [Required(ErrorMessage = "Please select a client.")]
        [Display(Name = "Client")]
        public int ClientID { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; } = "Draft";

        [Required]
        [Display(Name = "Service Level")]
        public string ServiceLevel { get; set; }

        // The uploaded PDF file — required on Create, optional on Edit
        [Display(Name = "Contract PDF")]
        public IFormFile? SignedAgreementFile { get; set; }

        // Used by Edit view to show the currently stored file
        public string? ExistingFilePath { get; set; }

        // Populated for the dropdowns
        public List<Client>? Clients { get; set; }
    }
}