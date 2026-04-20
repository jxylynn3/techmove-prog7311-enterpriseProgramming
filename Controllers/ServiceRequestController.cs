using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10448420_TechMove_GLMS.Data;
using ST10448420_TechMove_GLMS.Models;
using ST10448420_TechMove_GLMS.Models.ViewModels;
using ST10448420_TechMove_GLMS.Patterns.Builder;
using ST10448420_TechMove_GLMS.Patterns.Observer;
using ST10448420_TechMove_GLMS.UtilsServices;

namespace ST10448420_TechMove_GLMS.Controllers
{
    [Authorize(Roles = "Client")]
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PDFManagementService _pdfService;
        private readonly CurrencyApiService _currencyService;

        public ServiceRequestController(
            ApplicationDbContext context,
            PDFManagementService pdfService,
            CurrencyApiService currencyService)
        {
            _context = context;
            _pdfService = pdfService;
            _currencyService = currencyService;
        }

        [HttpGet]
        public IActionResult Create(int contractId)
        {
            var contract = _context.Contracts.Find(contractId);
            if (contract == null) return NotFound();

            // Passing the ID to the ViewModel for the form
            return View(new ServiceRequestViewModel
            {
                ContractID = contractId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var contract = _context.Contracts.Find(model.ContractID);
            if (contract == null) return NotFound();

            // 🛑 STATE PATTERN CHECK: Using the logic from your Details page
            // We check the Status string directly or call the State method
            if (contract.Status != "Active")
            {
                ModelState.AddModelError("", "This contract is no longer active. Service requests cannot be raised.");
                return View(model);
            }

            try
            {
                // 💰 CURRENCY: Fetch rate and calculate ZAR
                var rate = await _currencyService.GetRateAsync();
                decimal costZAR = model.CostUSD * (decimal)rate;

                // 📁 FILE UPLOAD: Use the utility service to save the PDF securely
                string filePath = await _pdfService.SaveFileAsync(model.File);

                // 🧱 BUILDER PATTERN: Constructing the final object
                // Note: We use the Director here to manage the complex building steps
                var builder = new ServiceRequestBuilder();
                var director = new ServiceRequestDirector();

                // Constructing the request through the Director ensures all parts are validated
                var serviceRequest = director.Construct(
                    builder,
                    model.ContractID,
                    model.Description,
                    model.CostUSD,
                    costZAR,
                    filePath
                );

                // 🔔 OBSERVER GUARD: Final safety check before DB save
                var guard = new ServiceRequestGuard();
                if (!guard.Validate(contract))
                {
                    ModelState.AddModelError("", "Security Check Failed: Contract status changed during the upload process.");
                    return View(model);
                }

                _context.ServiceRequests.Add(serviceRequest);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "ClientDashboard");
            }
            catch (Exception ex)
            {
                // Catching errors from the API or File Service
                ModelState.AddModelError("", "An error occurred: " + ex.Message);
                return View(model);
            }
        }

        public IActionResult Index()
        {
            // Logic for the Client to view their specific service request history
            return View();
        }
    }
}