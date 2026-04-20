using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST10448420_TechMove_GLMS.Data;
using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Controllers
{
[Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly ApplicationDbContext _context;
        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalClients = await _context.Clients.CountAsync();
            var totalContracts = await _context.Contracts.CountAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalClients = totalClients;
            ViewBag.TotalContracts = totalContracts;

            return View();
        }
        public IActionResult CreateGLMSUser()
        {
            ViewBag.Clients = _context.Clients.ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateGLMSUser(string email, string password, string role, int? clientId)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                ClientID = clientId
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                return RedirectToAction("ManageGLMSUsers");
            }

            return View();
        }

        // Admin view to manage GLMS users, showing their associated client companies
        public async Task<IActionResult> ManageGLMSUsers()
        {
            var users = await _context.Users
                .Include(u => u.Client)
                .ToListAsync();

            return View(users);
        }

    }
}
