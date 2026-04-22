using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448420_TechMove_GLMS.Data;
using ST10448420_TechMove_GLMS.Models;
using ST10448420_TechMove_GLMS.Models.ViewModels;
using ST10448420_TechMove_GLMS.UtilsServices;

namespace ST10448420_TechMove_GLMS.Controllers
{
    [Authorize(Roles = "Admin,LogisticsManager,Client")]
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        //DI injection of the PDF service and UserManager for user-related operations
        private readonly PDFManagementService _pdfService;      
        private readonly UserManager<ApplicationUser> _userManager; 

        public ContractController(
            ApplicationDbContext context,
            PDFManagementService pdfService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _pdfService = pdfService;
            _userManager = userManager;
        }

        //  Clients only see their own contracts; Admin/Manager see all
        public async Task<IActionResult> Index()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Client)
                .ToListAsync();
            return View(contracts);
        }

        // GET: Create — Admin/Logistics only
        [Authorize(Roles = "Admin,LogisticsManager")]
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new ContractViewModel
            {
                Clients = _context.Clients.ToList(),
                Status = "Draft",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };
            return View(vm);
        }


        [Authorize(Roles = "Admin,LogisticsManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractViewModel model)
        {
            // a must to upload a PDF when creating a new contract; for edits, it's optional
            if (model.SignedAgreementFile == null)
                ModelState.AddModelError("SignedAgreementFile", "A contract PDF is required.");

            if (!ModelState.IsValid)
            {
                model.Clients = _context.Clients.ToList();
                return View(model);
            }

            string filePath;
            try
            {
                filePath = await _pdfService.SaveFileAsync(model.SignedAgreementFile!);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"File upload failed: {ex.Message}");
                model.Clients = _context.Clients.ToList();
                return View(model);
            }

            var contract = new Contract
            {
                ClientID = model.ClientID,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Status = model.Status,
                ServiceLevel = model.ServiceLevel,
                SignedAgreementFilePath = filePath
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Contract created successfully.";
            return RedirectToAction("Index", "Admin");
        }


        [Authorize(Roles = "Admin,LogisticsManager")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            var vm = new ContractViewModel
            {
                ContractID = contract.ContractID,
                ClientID = contract.ClientID,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status,
                ServiceLevel = contract.ServiceLevel,
                ExistingFilePath = contract.SignedAgreementFilePath,
                Clients = _context.Clients.ToList()
            };
            return View(vm);
        }

        [Authorize(Roles = "Admin,LogisticsManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContractViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Clients = _context.Clients.ToList();
                return View(model);
            }

            var contract = await _context.Contracts.FindAsync(model.ContractID);
            if (contract == null) return NotFound();

            contract.ClientID = model.ClientID;
            contract.StartDate = model.StartDate;
            contract.EndDate = model.EndDate;
            contract.Status = model.Status;
            contract.ServiceLevel = model.ServiceLevel;

            if (model.SignedAgreementFile != null)
            {
                try
                {
                    contract.SignedAgreementFilePath = await _pdfService.SaveFileAsync(model.SignedAgreementFile);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"File upload failed: {ex.Message}");
                    model.Clients = _context.Clients.ToList();
                    return View(model);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Contract updated.";
            return RedirectToAction("Index", "Admin");
        }

        // (changed from GET — deleting on GET is dangerous)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
                return NotFound();

            // the handling: Active contracts cannot be deleted
            if (contract.Status == "Active")
            {
                TempData["Error"] = "Active contracts cannot be deleted. Change the status to Draft or Expired first.";
                return RedirectToAction(nameof(Index));
            }

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Contract #{id} deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        // Details — All authorised roles
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.ContractID == id);

            if (contract == null) return NotFound();

            //access control Clients can only view their own contracts
            if (User.IsInRole("Client"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (contract.ClientID != user.ClientID) return Forbid();
            }

            return View(contract);
        }

        // GET: Client reuploads PDF on an existing contract
        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> ReuploadPdf(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || contract.ClientID != user.ClientID) return NotFound();
            return View(contract);
        }

        // POST: Client reuploads PDF
        [Authorize(Roles = "Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReuploadPdf(int id, IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || contract.ClientID != user.ClientID) return NotFound();

            if (file == null)
            {
                ModelState.AddModelError("", "Please select a PDF to upload.");
                return View(contract);
            }

            try
            {
                contract.SignedAgreementFilePath = await _pdfService.SaveFileAsync(file);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contract PDF updated successfully.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(contract);
            }

            return RedirectToAction("Details", new { id });
        }
    }
}