using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448420_TechMove_GLMS.ApiServices;
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
        private readonly PDFManagementService _pdfService;
        private readonly UserManager<ApplicationUser> _userManager;

        // Part 3 addition: API service so Create POST writes to the API database,
        // ensuring newly created contracts appear in the Admin dashboard which
        // also reads from the API. Without this, contracts saved via _context go
        // to the MVC database and never show in the admin table.
        private readonly ApiContractService _contractApiService;

        public ContractController(
            ApplicationDbContext context,
            PDFManagementService pdfService,
            UserManager<ApplicationUser> userManager,
            ApiContractService contractApiService)
        {
            _context = context;
            _pdfService = pdfService;
            _userManager = userManager;
            _contractApiService = contractApiService;
        }

        // Part 3: Fetch contract list from the API so the Contract/Index view shows
        // the same data as the Admin Dashboard (which also reads from the API).
        // The old code queried _context.Contracts directly (MVC database) which is
        // separate from the API database — those contracts would never match.
        //
        // Clients only see their own contracts; Admin/Manager see all.
        public async Task<IActionResult> Index()
        {
            // commented out from Part 02 — direct DB query replaced by API call
            // var contracts = await _context.Contracts
            //     .Include(c => c.Client)
            //     .ToListAsync();
            // return View(contracts);

            try
            {
                var allContracts = await _contractApiService.GetAllContractsAsync();

                // Clients only see their own contracts
                if (User.IsInRole("Client"))
                {
                    // Part 3 fix: FindByEmailAsync instead of GetUserAsync
                    // (GetUserAsync needs NameIdentifier claim which our cookie doesn't have)
                    var currentUser = await _userManager.FindByEmailAsync(User.Identity!.Name!);
                    if (currentUser?.ClientID != null)
                    {
                        allContracts = allContracts
                            .Where(c => c.ClientID == currentUser.ClientID!.Value)
                            .ToList();
                    }
                    else
                    {
                        allContracts = new List<ContractApiDTO>();
                    }
                }

                return View(allContracts);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Could not load contracts: {ex.Message}";
                return View(new List<ContractApiDTO>());
            }
        }

        // GET: Create — Admin/Logistics only
        [Authorize(Roles = "Admin,LogisticsManager")]
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new ContractViewModel
            {
                Clients = _context.Clients.ToList(), // Clients list still from MVC Identity DB
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

            // Part 3: Send the new contract to the API instead of saving directly to
            // the MVC database. The API stores it in ST10448420_TechMove_GLMS_API_DB,
            // which is the same database the Admin Dashboard reads from.
            //
            // commented out from Part 02 — direct DB save replaced by API call below:
            // var contract = new Contract
            // {
            //     ClientID = model.ClientID,
            //     StartDate = model.StartDate,
            //     EndDate = model.EndDate,
            //     Status = model.Status,
            //     ServiceLevel = model.ServiceLevel,
            //     SignedAgreementFilePath = filePath
            // };
            // _context.Contracts.Add(contract);
            // await _context.SaveChangesAsync();

            var dto = new CreateContractApiDTO
            {
                ClientID = model.ClientID,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Status = model.Status,
                ServiceLevel = model.ServiceLevel,
                SignedAgreementFilePath = filePath
            };

            var (created, error) = await _contractApiService.CreateContractAsync(dto);

            if (error != null)
            {
                ModelState.AddModelError("", $"Could not save contract: {error}");
                model.Clients = _context.Clients.ToList();
                return View(model);
            }

            TempData["Success"] = "Contract created successfully.";
            return RedirectToAction("Index", "Admin");
        }

        [Authorize(Roles = "Admin,LogisticsManager")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Part 3: Fetch from API for consistent data source
            var contractDto = await _contractApiService.GetContractByIdAsync(id);
            if (contractDto == null) return NotFound();

            // commented out from Part 02:
            // var contract = await _context.Contracts.FindAsync(id);
            // if (contract == null) return NotFound();

            var vm = new ContractViewModel
            {
                ContractID = contractDto.ContractID,
                ClientID = contractDto.ClientID,
                StartDate = contractDto.StartDate,
                EndDate = contractDto.EndDate,
                Status = contractDto.Status,
                ServiceLevel = contractDto.ServiceLevel,
                ExistingFilePath = contractDto.SignedAgreementFilePath,
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

            // If a new PDF was uploaded, save it; otherwise keep the existing path
            string filePath = model.ExistingFilePath ?? string.Empty;
            if (model.SignedAgreementFile != null)
            {
                try
                {
                    filePath = await _pdfService.SaveFileAsync(model.SignedAgreementFile);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"File upload failed: {ex.Message}");
                    model.Clients = _context.Clients.ToList();
                    return View(model);
                }
            }

            // Part 3: Update status via the API PATCH endpoint.
            //
            // commented out from Part 02 — direct EF update replaced by API call:
            // var contract = await _context.Contracts.FindAsync(model.ContractID);
            // if (contract == null) return NotFound();
            // contract.ClientID = model.ClientID; ...
            // await _context.SaveChangesAsync();

            var (success, error) = await _contractApiService.UpdateStatusAsync(model.ContractID, model.Status);
            if (!success)
            {
                ModelState.AddModelError("", $"Could not update contract: {error}");
                model.Clients = _context.Clients.ToList();
                return View(model);
            }

            TempData["Success"] = "Contract updated.";
            return RedirectToAction("Index", "Admin");
        }

        // (changed from GET — deleting on GET is dangerous)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Delete is still via the MVC context since the API does not expose
            // DELETE /api/contracts. Kept as-is from Part 02.
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
            // Part 3: Fetch from the API for consistency with the Admin Dashboard.
            var contractDto = await _contractApiService.GetContractByIdAsync(id);
            if (contractDto == null) return NotFound();

            // Access control: Clients can only view their own contracts
            if (User.IsInRole("Client"))
            {
                // Part 3 fix: FindByEmailAsync instead of GetUserAsync.
                // GetUserAsync requires NameIdentifier claim which is not present
                // in our cookie identity — it always returns null, causing NullReferenceException.
                // commented out from Part 02: var user = await _userManager.GetUserAsync(User);
                var user = await _userManager.FindByEmailAsync(User.Identity!.Name!);
                if (user == null || contractDto.ClientID != user.ClientID)
                    return Forbid();
            }

            // Build a Contract entity from the DTO so the existing Details view
            // (which still uses the Contract model) continues to work without changes.
            var contract = new Contract
            {
                ContractID = contractDto.ContractID,
                ClientID = contractDto.ClientID,
                StartDate = contractDto.StartDate,
                EndDate = contractDto.EndDate,
                Status = contractDto.Status,
                ServiceLevel = contractDto.ServiceLevel,
                SignedAgreementFilePath = contractDto.SignedAgreementFilePath,
                Client = new Client { Name = contractDto.ClientName }
            };

            return View(contract);
        }

        // GET: Client reuploads PDF on an existing contract
        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> ReuploadPdf(int id)
        {
            // Part 3 fix: FindByEmailAsync instead of GetUserAsync (same reason as Details)
            // commented out from Part 02: var user = await _userManager.GetUserAsync(User);
            var user = await _userManager.FindByEmailAsync(User.Identity!.Name!);
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || contract.ClientID != user?.ClientID) return NotFound();
            return View(contract);
        }

        // POST: Client reuploads PDF
        [Authorize(Roles = "Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReuploadPdf(int id, IFormFile file)
        {
            // Part 3 fix: FindByEmailAsync instead of GetUserAsync
            // commented out from Part 02: var user = await _userManager.GetUserAsync(User);
            var user = await _userManager.FindByEmailAsync(User.Identity!.Name!);
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || contract.ClientID != user?.ClientID) return NotFound();

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