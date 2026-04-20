using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448420_TechMove_GLMS.Data;

namespace ST10448420_TechMove_GLMS.Controllers
{
    [Authorize(Roles = "Client")]
    public class ContractController : Controller
    {
    private readonly ApplicationDbContext _context;

        public ContractController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            var contracts = _context.Contracts
                .Include(c => c.Client)
                .ToList();

            return View(contracts);
        }

        public IActionResult Details(int id)
        {
            var contract = _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefault(c => c.ContractID == id);
            if (contract == null)
                return NotFound();

            return View(contract);
        }

        public IActionResult ChangeStatus(int id, string status)
        {
            var contract = _context.Contracts.Find(id);
            contract.Status = status;
            _context.SaveChanges();

            return RedirectToAction("Details", new { id });
        }
    }
}
