using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10448420_TechMove_GLMS.Models
{
    public class ServiceRequest
    {
        [Key]
        public int RequestID { get; set; }

        [Required]
        public int ContractID { get; set; }
        public virtual Contract Contract { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostUSD { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostZAR { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // Requested, Processed, etc.
        public DateTime CreatedAt { get; internal set; }
    }
}
