using System.ComponentModel.DataAnnotations;

namespace ST10448420_TechMove_GLMS.Models.ViewModels
{
    public class AdminServiceRequestEditViewModel
    {
        public int RequestID { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        [Display(Name = "Cost (USD)")]
        public decimal CostUSD { get; set; }
    }
}