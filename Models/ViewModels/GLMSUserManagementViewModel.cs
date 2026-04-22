using System.ComponentModel.DataAnnotations;

namespace ST10448420_TechMove_GLMS.Models.ViewModels
{
    public class GLMSUserManagementViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        public int? ClientID { get; set; }

        public List<Client>? Clients { get; set; }
    }
}// bro dont confuse GLMSUserManagementViewModel with CreateUserViewModel or EditUserViewModel.
 // this is a general view model for user management that can be used for both creating and editing users. 
 //the views of that use GLSMUserManagementViewModel will determine whether it's for creating or editing a user, and the controller will handle the logic .