using System.ComponentModel.DataAnnotations;

namespace ST10448420_TechMove_GLMS.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        public int? ClientID { get; set; }

        public List<Client>? Clients { get; set; }
    }
}