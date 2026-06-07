using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ST10448420_TechMove_GLMS.ApiServices;
using ST10448420_TechMove_GLMS.Models;
using ST10448420_TechMove_GLMS.Models.ViewModels;
// using Microsoft.EntityFrameworkCore;
// using ST10448420_TechMove_GLMS.Data;
// using ST10448420_TechMove_GLMS.Models;
// using ST10448420_TechMove_GLMS.Patterns.Builder;
// using ST10448420_TechMove_GLMS.UtilsServices;

namespace ST10448420_TechMove_GLMS.Controllers
{
    [Authorize(Roles = "Client")]
    public class ServiceRequestController : Controller
    {
        private readonly ApiServiceRequestService _serviceRequestApiService;
        private readonly ApiContractService _contractApiService;
        private readonly UserManager<ApplicationUser> _userManager;
        // private readonly ApplicationDbContext _context;             // commented out from Part 02
        // private readonly CurrencyApiService _currencyService;       // commented out from Part 02
        // private readonly PDFManagementService _pdfService;          // commented out from Part 02
        // private readonly UserManager<ApplicationUser> _userManager; // commented out from Part 02

        public ServiceRequestController(
            ApiServiceRequestService serviceRequestApiService,
            ApiContractService contractApiService,
            UserManager<ApplicationUser> userManager)
        {
            _serviceRequestApiService = serviceRequestApiService;
            _contractApiService = contractApiService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Part 02 direct DB approach — commented out:
            // var user = await _userManager.GetUserAsync(User);
            // var requests = await _context.ServiceRequests
            //     .Include(r => r.Contract)
            //     .Where(r => r.Contract.ClientID == user.ClientID)
            //     .OrderByDescending(r => r.CreatedAt).ToListAsync();
            // return View(requests);

            try
            {
                // Part 3: Use FindByEmailAsync instead of GetUserAsync for the same reason as
                // ClientDashboardController — the cookie identity does not contain NameIdentifier,
                // so GetUserAsync always returns null. FindByEmailAsync uses User.Identity.Name
                // (the email address stored in ClaimTypes.Name) which IS present in the cookie.
                var currentUser = await _userManager.FindByEmailAsync(User.Identity!.Name!);

                if (currentUser?.ClientID == null)
                {
                    ViewBag.Error = "Your account is not linked to a client. Please contact the administrator.";
                    return View(new List<ServiceRequestApiDTO>());
                }

                // Step 1: Fetch all contracts from the API and collect the IDs belonging to
                // this client. This avoids adding a /api/servicerequests/byclient/{id} endpoint.
                var allContracts = await _contractApiService.GetAllContractsAsync();

                var myContractIds = allContracts
                    .Where(c => c.ClientID == currentUser.ClientID!.Value)
                    .Select(c => c.ContractID)
                    .ToHashSet();

                // Step 2: Fetch all service requests and filter to those on this client's contracts.
                var allRequests = await _serviceRequestApiService.GetAllAsync();

                var myRequests = allRequests
                    .Where(r => myContractIds.Contains(r.ContractID))
                    .ToList();

                return View(myRequests);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Could not load service requests: {ex.Message}";
                return View(new List<ServiceRequestApiDTO>());
            }
        }

        [HttpGet]
        public IActionResult Create(int contractId)
        {
            return View(new ServiceRequestViewModel { ContractID = contractId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // commenting out part 2
            // var contract = await _context.Contracts.FindAsync(model.ContractID);
            // if (!contract.CurrentState.contractCanRaiseServiceRequest()) { ... }
            // var rate = await _currencyService.GetRateAsync();
            // var builder = new ServiceRequestBuilder(); ...

            // PART 3: The API now handles the state check AND currency conversion.
            // We just send the DTO; the API returns 400 with a message if the
            // contract isn't Active.
            var dto = new CreateServiceRequestApiDTO
            {
                ContractID = model.ContractID,
                Description = model.Description,
                CostUSD = model.CostUSD
                // DocumentPath handled separately if needed
            };

            var (created, error) = await _serviceRequestApiService.CreateAsync(dto);

            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View(model);
            }

            TempData["Success"] = "Service request submitted with status 'Draft'.";
            return RedirectToAction("Index");
        }
    }
}