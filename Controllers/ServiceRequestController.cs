using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448420_TechMove_GLMS.Data;
using ST10448420_TechMove_GLMS.Models;
using ST10448420_TechMove_GLMS.Models.ViewModels;
using ST10448420_TechMove_GLMS.Patterns.Builder;
using ST10448420_TechMove_GLMS.UtilsServices;

namespace ST10448420_TechMove_GLMS.Controllers
{
    [Authorize(Roles = "Client")]
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CurrencyApiService _currencyService;
        private readonly PDFManagementService _pdfService;
        private readonly UserManager<ApplicationUser> _userManager;  

        public ServiceRequestController(
            ApplicationDbContext context,
            CurrencyApiService currencyService,
            PDFManagementService pdfService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _currencyService = currencyService;
            _pdfService = pdfService;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var requests = await _context.ServiceRequests
                .Include(r => r.Contract)
                .Where(r => r.Contract.ClientID == user.ClientID)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int contractId)
        {
            var user = await _userManager.GetUserAsync(User);
            var contract = await _context.Contracts.FindAsync(contractId);

            if (contract == null || contract.ClientID != user.ClientID)
                return NotFound();

            // the state pattern is used to check that the contract created is in the right status to create a service request
            if (!contract.CurrentState.contractCanRaiseServiceRequest())
            {
                TempData["Error"] = $"Cannot create a service request. Your contract status is '{contract.Status}'. It must be Active.";
                return RedirectToAction("Index", "ClientDashboard");
            }

            return View(new ServiceRequestViewModel { ContractID = contractId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var contract = await _context.Contracts.FindAsync(model.ContractID);
            if (contract == null) return NotFound();

            // ✅ FIX #6 — Double-check state on POST as well
            if (!contract.CurrentState.contractCanRaiseServiceRequest())
            {
                ModelState.AddModelError("", $"Contract status is '{contract.Status}'. Only Active contracts can have service requests.");
                return View(model);
            }

            var rate = await _currencyService.GetRateAsync();
            var costZAR = model.CostUSD * rate;

            string? filePath = null;
            if (model.File != null)
            {
                try { filePath = await _pdfService.SaveFileAsync(model.File); }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"File upload failed: {ex.Message}");
                    return View(model);
                }
            }

            var builder = new ServiceRequestBuilder();
            var director = new ServiceRequestDirector();

            var request = director.Construct(
                builder,
                model.ContractID,
                model.Description,
                model.CostUSD,
                costZAR,
                filePath!
            );

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service request submitted with status 'Draft'.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var req = await _context.ServiceRequests
                .Include(r => r.Contract)
                .FirstOrDefaultAsync(r => r.RequestID == id);

            if (req == null || req.Contract.ClientID != user.ClientID)
                return NotFound();

            var vm = new ServiceRequestViewModel
            {
                RequestID = req.RequestID,
                ContractID = req.ContractID,
                Description = req.Description,
                CostUSD = req.CostUSD
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceRequestViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);

            var req = await _context.ServiceRequests
                .Include(r => r.Contract)
                .FirstOrDefaultAsync(r => r.RequestID == model.RequestID);

            if (req == null || req.Contract.ClientID != user.ClientID)
                return NotFound();

            req.Description = model.Description;
            req.CostUSD = model.CostUSD;

            var rate = await _currencyService.GetRateAsync();
            req.CostZAR = model.CostUSD * rate;

            if (model.File != null)
            {
                try { req.DocumentPath = await _pdfService.SaveFileAsync(model.File); }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Service request updated.";
            return RedirectToAction("Index");
        }
    }
}