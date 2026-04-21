using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448420_TechMove_GLMS.Data;
using ST10448420_TechMove_GLMS.Models;
using ST10448420_TechMove_GLMS.Models.ViewModels;

namespace ST10448420_TechMove_GLMS.Controllers
{
    [Authorize(Roles = "Admin,LogisticsManager")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)      // ✅ Added RoleManager
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ===== DASHBOARD =====
        // Admin dashboard: lists contracts with search/filter
        public IActionResult Index(string search, string status, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Client.Name.Contains(search));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            // ✅ FIX #10 — Date range filter added
            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(query.ToList());
        }

        // ===== CONTRACT STATUS =====
        // ✅ FIX #9 — Now POST only. Status passed via form, not query string.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            contract.Status = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Contract #{id} status changed to '{status}'.";
            return RedirectToAction("Index");
        }

        // ===== USER MANAGEMENT =====
        // ✅ FIX #8 — Consolidated ManageUsers showing roles + client
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.Include(u => u.Client).ToListAsync();

            var viewModels = new List<UserWithRoleViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                viewModels.Add(new UserWithRoleViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    Role = roles.FirstOrDefault() ?? "No Role",
                    ClientName = user.Client?.Name ?? "—"
                });
            }

            return View(viewModels);
        }

        // ✅ FIX #8 — User details
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserDetails(string id)
        {
            var user = await _context.Users.Include(u => u.Client)
                                           .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var vm = new UserWithRoleViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                Role = roles.FirstOrDefault() ?? "No Role",
                ClientName = user.Client?.Name ?? "—"
            };
            return View(vm);
        }

        // GET: Create User
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult CreateUser()
        {
            var model = new GLMSUserManagementViewModel
            {
                Clients = _context.Clients.ToList()
            };
            return View(model);
        }

        // POST: Create User
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(GLMSUserManagementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Clients = _context.Clients.ToList();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                ClientID = model.ClientID
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                TempData["Success"] = "User created.";
                return RedirectToAction("ManageUsers");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            model.Clients = _context.Clients.ToList();
            return View(model);
        }

        // ✅ FIX #8 — Edit User GET
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var vm = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                Role = roles.FirstOrDefault() ?? "",
                ClientID = user.ClientID,
                Clients = _context.Clients.ToList()
            };
            return View(vm);
        }

        // ✅ FIX #8 — Edit User POST
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Clients = _context.Clients.ToList();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;
            user.ClientID = model.ClientID;
            await _userManager.UpdateAsync(user);

            // Swap roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = "User updated.";
            return RedirectToAction("ManageUsers");
        }

        // ✅ FIX #8 — Delete User POST (was missing entirely)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "User deleted.";
            return RedirectToAction("ManageUsers");
        }

        // ===== SERVICE REQUEST MANAGEMENT =====
        public IActionResult ServiceRequests()
        {
            var requests = _context.ServiceRequests
                .Include(r => r.Contract)
                .ThenInclude(c => c.Client)
                .ToList();
            return View(requests);
        }

        // ✅ FIX #8 — Full details for a service request
        public async Task<IActionResult> ServiceRequestDetails(int id)
        {
            var req = await _context.ServiceRequests
                .Include(r => r.Contract)
                .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(r => r.RequestID == id);

            if (req == null) return NotFound();
            return View(req);
        }

        // ✅ FIX #8 — Edit Service Request GET
        [HttpGet]
        public async Task<IActionResult> EditServiceRequest(int id)
        {
            var req = await _context.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();

            var vm = new AdminServiceRequestEditViewModel
            {
                RequestID = req.RequestID,
                Description = req.Description,
                Status = req.Status,
                CostUSD = req.CostUSD
            };
            return View(vm);
        }

        // ✅ FIX #8 — Edit Service Request POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditServiceRequest(AdminServiceRequestEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var req = await _context.ServiceRequests.FindAsync(model.RequestID);
            if (req == null) return NotFound();

            req.Description = model.Description;
            req.Status = model.Status;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Service request updated.";
            return RedirectToAction("ServiceRequests");
        }

        // ✅ FIX #8 — Delete Service Request POST, with Active-contract guard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteServiceRequest(int id)
        {
            var req = await _context.ServiceRequests
                .Include(r => r.Contract)
                .FirstOrDefaultAsync(r => r.RequestID == id);

            if (req == null) return NotFound();

            // Guard: Admin cannot delete a service request tied to an Active contract
            if (req.Contract?.Status == "Active")
            {
                TempData["Error"] = "Cannot delete a service request linked to an Active contract.";
                return RedirectToAction("ServiceRequests");
            }

            _context.ServiceRequests.Remove(req);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Service request deleted.";
            return RedirectToAction("ServiceRequests");
        }

        // ✅ FIX #10 — Approve/Reject changed to POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var req = await _context.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();
            req.Status = "Approved";
            await _context.SaveChangesAsync();
            return RedirectToAction("ServiceRequests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var req = await _context.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();
            req.Status = "Rejected";
            await _context.SaveChangesAsync();
            return RedirectToAction("ServiceRequests");
        }
    }
}