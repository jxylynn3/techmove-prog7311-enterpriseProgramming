using System.ComponentModel.DataAnnotations;

namespace ST10448420_TechMove_GLMS.Models.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }

        public int? ClientID { get; set; }

        public List<Client>? Clients { get; set; }
    }
}