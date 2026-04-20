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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Authenticate user
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {

                var user = await _userManager.FindByEmailAsync(model.Email);

                // redirection based on role
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                if (await _userManager.IsInRoleAsync(user, "Client"))
                {
                    return RedirectToAction("Index", "ClientDashboard");
                }

                if (await _userManager.IsInRoleAsync(user, "LogisticsManager"))
                {
                    return RedirectToAction("Index", "Admin"); // or Manager dashboard if you add one
                }

                // fallback
                return RedirectToAction("Login", "Account");
            }

            // Failed login
            ModelState.AddModelError("", "Invalid login attempt");
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
