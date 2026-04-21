using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448420_TechMove_GLMS.Data;
using ST10448420_TechMove_GLMS.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ST10448420_TechMove_GLMS.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientDashboardController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var contracts = _context.Contracts
                .Include(c => c.ServiceRequests)
                .Where(c => c.ClientID == user.ClientID)
                .ToList();

            return View(contracts);
        }
    }

}

