using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ST10448420_TechMove_GLMS.ApiServices;
using ST10448420_TechMove_GLMS.Data;
using ST10448420_TechMove_GLMS.Models;
using ST10448420_TechMove_GLMS.Models.ViewModels;

//
// using Microsoft.EntityFrameworkCore;
// using ST10448420_TechMove_GLMS.Data;
// using ST10448420_TechMove_GLMS.Models;
// using ST10448420_TechMove_GLMS.Models.ViewModels;

namespace ST10448420_TechMove_GLMS.Controllers
{
    [Authorize(Roles = "Admin,LogisticsManager")]
    public class AdminController : Controller
    {
        private readonly ApiContractService _contractApiService;
        private readonly ApiServiceRequestService _serviceRequestApiService;

        // NOTE: ManageUsers, CreateUser, EditUser, DeleteUser still use Identity
        // via UserManager — those are kept as-is since Identity lives on the API side.
        // In a full SOA refactor you would add a /api/users endpoint too, but for
        // the scope of this submission the user management pages remain direct-Identity.
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        // private readonly ApplicationDbContext _context;        // commented out from Part 02
        // private readonly UserManager<ApplicationUser> _userManager;   // commented out from Part 02
        // private readonly RoleManager<IdentityRole> _roleManager;      // commented out from Part 02

        public AdminController(
            ApiContractService contractApiService,
            ApiServiceRequestService serviceRequestApiService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _contractApiService = contractApiService;
            _serviceRequestApiService = serviceRequestApiService;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ── CONTRACTS (via API) ────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? search, string? status)
        {
            // var query = _context.Contracts.Include(c => c.Client).AsQueryable();
            // if (!string.IsNullOrEmpty(search)) query = query.Where(c => c.Client.Name.Contains(search));
            // if (!string.IsNullOrEmpty(status)) query = query.Where(c => c.Status == status);
            // return View(query.ToList());

            try
            {
                var contracts = await _contractApiService.GetAllContractsAsync();

                // Apply filtering client-side (since we got all data from API)
                if (!string.IsNullOrEmpty(search))
                    contracts = contracts.Where(c => c.ClientName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.IsNullOrEmpty(status))
                    contracts = contracts.Where(c => c.Status == status).ToList();

                ViewBag.Search = search;
                ViewBag.Status = status;
                return View(contracts);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Could not load contracts from API: {ex.Message}";
                return View(new List<ContractApiDTO>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            // var contract = await _context.Contracts.FindAsync(id);
            // if (contract == null) return NotFound();
            // contract.Status = status;
            // await _context.SaveChangesAsync();

            var (success, error) = await _contractApiService.UpdateStatusAsync(id, status);
            if (!success)
            {
                TempData["Error"] = error ?? "Failed to update status.";
                return RedirectToAction("Index");
            }

            TempData["Success"] = $"Contract #{id} status changed to '{status}'.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ServiceRequests()
        {
            // var requests = _context.ServiceRequests.Include(r => r.Contract).ThenInclude(c => c.Client).ToList();
            // return View(requests)

            try
            {
                var requests = await _serviceRequestApiService.GetAllAsync();
                return View(requests);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Could not load service requests: {ex.Message}";
                return View(new List<ServiceRequestApiDTO>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            await _serviceRequestApiService.UpdateStatusAsync(id, "Approved");
            return RedirectToAction("ServiceRequests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            await _serviceRequestApiService.UpdateStatusAsync(id, "Rejected");
            return RedirectToAction("ServiceRequests");
        }

        // ── USER MANAGEMENT (direct Identity — kept from Part 2) ──────────────────────────────
        // These actions were omitted during the Part 3 refactor but the views and nav links
        // still exist. Restoring them here using the MVC's own UserManager/RoleManager.

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUsers()
        {
            
        var users = _userManager.Users.ToList();
            var model = new List<UserWithRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var clientName = string.Empty;

                if (user.ClientID.HasValue)
                {
                    var client = await _context.Clients.FindAsync(user.ClientID.Value);
                    clientName = client?.Name ?? string.Empty;
                }

                model.Add(new UserWithRoleViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "No Role",
                    ClientName = clientName
                });
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult CreateUser()
        {
            var vm = new CreateUserViewModel
            {
                Clients = _context.Clients.ToList()
            };
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
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

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                model.Clients = _context.Clients.ToList();
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.Role))
                await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = $"User {model.Email} created successfully.";
            return RedirectToAction("ManageUsers");
        }

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
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? string.Empty,
                ClientID = user.ClientID,
                Clients = _context.Clients.ToList()
            };

            return View(vm);
        }

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

            // Replace role
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!string.IsNullOrEmpty(model.Role))
                await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = $"User {model.Email} updated successfully.";
            return RedirectToAction("ManageUsers");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "User deleted.";
            return RedirectToAction("ManageUsers");
        }
    }
}