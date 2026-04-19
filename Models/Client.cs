using System.ComponentModel.DataAnnotations;

namespace ST10448420_TechMove_GLMS.Models
{
    public class Client
    {
        [Key]
        public int ClientID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string ContactDetails { get; set; }

        [Required]
        public string Region { get; set; }

        public virtual ICollection<Contract> Contracts { get; set; }
    }
}
