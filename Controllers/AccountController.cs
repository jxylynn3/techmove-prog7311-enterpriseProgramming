using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ST10448420_TechMove_GLMS.ApiServices;
using ST10448420_TechMove_GLMS.Models.ViewModels;
using System.Security.Claims;

namespace ST10448420_TechMove_GLMS.Controllers
//jayy from part 03: previously UserManager + SignInManager had direct DB Identity access, but
//now we call the API's /api/auth/login endpoint for JWT, then create
//a cookie-based identity so MVC [Authorize] attributes still work.
{
    public class AccountController : Controller
    {
        private readonly ApiAuthService _authService;

        //commenting out from Part 02!!
        // DI that allows us to manage users, sign in and out, and manage roles
        /*
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        */

        public AccountController(ApiAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            //commenting out from Part 02!!
            // var user = await _userManager.FindByEmailAsync(model.Email);
            // var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            var (token, roles, error) = await _authService.LoginAsync(model.Email, model.Password);

            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View(model);
            }

            // Guard: ensure roles is never null
            var userRoles = roles ?? new List<string>();

            // Store JWT in session so ApiServices can attach it to future API calls
            HttpContext.Session.SetString("JwtToken", token!);

            // Build a claims identity using Identity's own application scheme.
            // ── KEY FIX: IdentityConstants.ApplicationScheme = "Identity.Application"
            // This is the scheme AddIdentity registered. Using "Cookies" fails because
            // AddIdentity does not register a handler for that name.
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name,  model.Email),
    new Claim(ClaimTypes.Email, model.Email),
};
            claims.AddRange(userRoles.Select(r => new Claim(ClaimTypes.Role, r)));
            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            var principal = new ClaimsPrincipal(identity);

            // SignInAsync uses "Identity.Application" — the scheme that IS registered
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

            // Role-based redirect — same logic as Part 2
            if (userRoles.Contains("Admin") || userRoles.Contains("LogisticsManager"))
                return RedirectToAction("Index", "Admin");

            if (userRoles.Contains("Client"))
                return RedirectToAction("Index", "ClientDashboard");

            return RedirectToAction("Index", "Home");
        }

        /*[HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        */ // replaced by the new Logout method below, which also clears the JWT from session

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JwtToken");
            // Sign out of Identity's application scheme, not "Cookies"
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index() => View(); // this is here so that if a user tries to access /Account, they get a nice page instead of an error

        public IActionResult AccessDenied() => View(); // this is here so that if a user tries to access a page they don't have permission for, they get a nice message instead of an error
    }
}