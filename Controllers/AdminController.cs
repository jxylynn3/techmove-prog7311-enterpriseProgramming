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
            RoleManager<IdentityRole> roleManager)    //added to manage user roles within the admin panel
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public IActionResult Index(string search, string status, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Client.Name.Contains(search));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);
            // below is the Date range filtering logic, that basically  uses the query WHERE clause to filter contracts based on their StartDate and EndDate properties.
            //like in SQL 
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

// Consolidated ManageUsers showing roles + client
        // AdminController.cs
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var model = new List<UserWithRoleViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserWithRoleViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName, // adjust to your actual property
                    Role = roles.FirstOrDefault() ?? "No role"
                });
            }
            return View(model); // passes List<UserWithRoleViewModel>
        }

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
        public async Task<IActionResult> CreateUser()
        {
            var model = new GLMSUserManagementViewModel
            {
                Clients = await _context.Clients.ToListAsync()
            };
            return View(model);
        }

        // POST: Create User
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(GLMSUserManagementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Clients = await _context.Clients.ToListAsync();
                return View(model);
            }

            // If role is Client and no existing client was selected, create one automatically
            int? resolvedClientID = model.ClientID;

            if (model.Role == "Client" && resolvedClientID == null)
            {
                var newClient = new Client
                {
                    Name = model.Email, // use email as placeholder name
                    ContactDetails = model.Email,
                    Region = "Not specified"
                };
                _context.Clients.Add(newClient);
                await _context.SaveChangesAsync(); // saves and generates ClientID
                resolvedClientID = newClient.ClientID;
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                ClientID = resolvedClientID
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Role))
                    await _userManager.AddToRoleAsync(user, model.Role);

                TempData["Success"] = "User created successfully.";
                return RedirectToAction(nameof(ManageUsers));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            model.Clients = await _context.Clients.ToListAsync();
            return View(model);
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
                Email = user.Email!,
                Role = roles.FirstOrDefault() ?? "",
                ClientID = user.ClientID,
                Clients = _context.Clients.ToList()
            };
            return View(vm);
        }

        
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Clients = await _context.Clients.ToListAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.ClientID = model.ClientID; // if ApplicationUser has ClientID

            await _userManager.UpdateAsync(user);

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = "User updated.";
            return RedirectToAction(nameof(ManageUsers));
        }

       
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            await _userManager.DeleteAsync(user);
            TempData["Success"] = "User deleted.";
            return RedirectToAction(nameof(ManageUsers));
        }

        public IActionResult ServiceRequests()
        {
            var requests = _context.ServiceRequests
                .Include(r => r.Contract)
                .ThenInclude(c => c.Client)
                .ToList();
            return View(requests);
        }

        // this was missing entirely before — shows details of a service request, including contract and client info
        [HttpGet]
        public async Task<IActionResult> ServiceRequestDetails(int id)
        {
            var req = await _context.ServiceRequests
                .Include(r => r.Contract)
                    .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(r => r.RequestID == id);

            if (req == null) return NotFound();
            return View(req);
        }

        
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

        // the deletion logic for service requests was missing before — now added, with Active-contract rules
        // [the rules: Admin cannot delete a service request linked to an Active contract]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteServiceRequest(int id)
        {
            var req = await _context.ServiceRequests
                .Include(r => r.Contract)
                .FirstOrDefaultAsync(r => r.RequestID == id);

            if (req == null) return NotFound();

            // error handling Admin cannot delete a service request tied to an Active contract
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

        // the approvl and rejection logic for service requests was missing before — now added, with simple status update and redirect back to list
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