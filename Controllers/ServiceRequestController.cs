using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10448420_TechMove_GLMS.ApiServices;
using ST10448420_TechMove_GLMS.APIServices;
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
        // private readonly ApplicationDbContext _context;
        // private readonly CurrencyApiService _currencyService;
        // private readonly PDFManagementService _pdfService;
        // private readonly UserManager<ApplicationUser> _userManager;

        public ServiceRequestController(
            ApiServiceRequestService serviceRequestApiService,
            ApiContractService contractApiService)
        {
            _serviceRequestApiService = serviceRequestApiService;
            _contractApiService = contractApiService;
        }

        public async Task<IActionResult> Index()
        {
            // var user = await _userManager.GetUserAsync(User);
            // var requests = await _context.ServiceRequests
            //     .Include(r => r.Contract)
            //     .Where(r => r.Contract.ClientID == user.ClientID)
            //     .OrderByDescending(r => r.CreatedAt).ToListAsync();
            // return View(requests);

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
            //commenting out part 2
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