using System.ComponentModel.DataAnnotations;

namespace ST10448420_TechMove_GLMS.Models.ViewModels
{
    public class AuthenticationViewModel
    {
        // this class is empty because we will use the LoginViewModel and RegisterViewModel separately in our views.
        //But it can be used in the future if we want to have a combined view for login and registration... idk bro
    }
    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Role { get; set; } // Admin, LogisticsManager, or Client

        public int? ClientID { get; set; } // Optional: Link to a Client company
    }
}
