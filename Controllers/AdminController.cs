using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10448420_TechMove_GLMS.ApiServices;
using ST10448420_TechMove_GLMS.APIServices;

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

        
        // private readonly ApplicationDbContext _context;
        // private readonly UserManager<ApplicationUser> _userManager;
        // private readonly RoleManager<IdentityRole> _roleManager;
     

        public AdminController(
            ApiContractService contractApiService,
            ApiServiceRequestService serviceRequestApiService)
        {
            _contractApiService = contractApiService;
            _serviceRequestApiService = serviceRequestApiService;
        }

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

        // NOTE: ManageUsers, CreateUser, EditUser, DeleteUser still use Identity
        // via UserManager — those are kept as-is since Identity lives on the API side.
        // In a full SOA refactor you would add a /api/users endpoint too, but for
        // the scope of this submission the user management pages can remain.
    }
}