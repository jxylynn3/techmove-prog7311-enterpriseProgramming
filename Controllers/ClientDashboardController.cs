using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ST10448420_TechMove_GLMS.ApiServices;
using ST10448420_TechMove_GLMS.Models;
// using Microsoft.EntityFrameworkCore;
// using ST10448420_TechMove_GLMS.Data;
// using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientDashboardController : Controller
    {
        private readonly ApiContractService _contractApiService;
        private readonly UserManager<ApplicationUser> _userManager;
        // private readonly ApplicationDbContext _context;        // commented out from Part 02
        // private readonly UserManager<ApplicationUser> _userManager;  // commented out from Part 02

        public ClientDashboardController(
            ApiContractService contractApiService,
            UserManager<ApplicationUser> userManager)
        {
            _contractApiService = contractApiService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Part 02 direct DB approach — commented out:
            // var user = await _userManager.GetUserAsync(User);
            // var contracts = _context.Contracts.Include(c => c.ServiceRequests)
            //     .Where(c => c.ClientID == user.ClientID).ToList();
            // return View(contracts);

            try
            {
                // Part 3: We use FindByEmailAsync instead of GetUserAsync because our cookie
                // identity (built in AccountController.Login) does not include a NameIdentifier
                // claim — it only has ClaimTypes.Name (set to the user's email) and ClaimTypes.Role.
                // GetUserAsync relies on NameIdentifier to find the user and returns null without it.
                // FindByEmailAsync uses User.Identity.Name (the email) which IS present in the cookie.
                var currentUser = await _userManager.FindByEmailAsync(User.Identity!.Name!);

                if (currentUser?.ClientID == null)
                {
                    ViewBag.Error = "Your account is not linked to a client. Please contact the administrator.";
                    return View(new List<ContractApiDTO>());
                }

                // Fetch all contracts from the API and filter to only this client's contracts.
                // Client-side filtering is acceptable here since the number of contracts is
                // manageable, and adding a /api/contracts/byclient/{id} endpoint is out of scope.
                var allContracts = await _contractApiService.GetAllContractsAsync();

                var myContracts = allContracts
                    .Where(c => c.ClientID == currentUser.ClientID!.Value)
                    .ToList();

                return View(myContracts);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Could not load your contracts: {ex.Message}";
                return View(new List<ContractApiDTO>());
            }
        }
    }
}