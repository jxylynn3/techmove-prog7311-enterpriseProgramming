using Microsoft.AspNetCore.Identity;

namespace ST10448420_TechMove_GLMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? ClientID { get; set; }
        // virtual is used to help with loading to Client model
        public virtual Client Client { get; set; }
    }
}
