using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10448420_TechMove_GLMS.ApiServices;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using ST10448420_TechMove_GLMS.Data;
// using ST10448420_TechMove_GLMS.Models;


namespace ST10448420_TechMove_GLMS.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientDashboardController : Controller
    {
        private readonly ApiContractService _contractApiService;
        // private readonly ApplicationDbContext _context;
        // private readonly UserManager<ApplicationUser> _userManager;

        public ClientDashboardController(ApiContractService contractApiService)
        {
            _contractApiService = contractApiService;
        }

        public async Task<IActionResult> Index()
        {
            // var user = await _userManager.GetUserAsync(User);
            // var contracts = _context.Contracts.Include(c => c.ServiceRequests)
            //     .Where(c => c.ClientID == user.ClientID).ToList();
            // return View(contracts);

            try
            {
                var contracts = await _contractApiService.GetAllContractsAsync();
                return View(contracts);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Could not load your contracts: {ex.Message}";
                return View(new List<ContractApiDTO>());
            }
        }
    }
}