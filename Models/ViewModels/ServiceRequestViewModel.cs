using System.ComponentModel.DataAnnotations;

namespace ST10448420_TechMove_GLMS.Models.ViewModels
{
    public class ServiceRequestViewModel
    {
        public int ContractID { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal CostUSD { get; set; }
        public IFormFile File { get; set; }

    }
}
