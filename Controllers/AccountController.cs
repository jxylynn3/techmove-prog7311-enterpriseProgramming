using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ST10448420_TechMove_GLMS.Models;
using ST10448420_TechMove_GLMS.Models.ViewModels;

namespace ST10448420_TechMove_GLMS.Controllers
{
    public class AccountController : Controller
    {
        // DI that allows us to manage users, sign in and out, and manage roles
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Look up the user by email first — PasswordSignInAsync expects a username by default.
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }// you fucked the database configuration now nthing work.you better come fix this

            // Use the user's UserName (or use CheckPasswordSignInAsync + SignInAsync)
            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Admin", new { area = "" });

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    
        public IActionResult Index()
        {
            return View();
        }
    }
}
