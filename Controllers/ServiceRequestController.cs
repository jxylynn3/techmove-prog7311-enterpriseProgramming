using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using ST10448420_TechMove_GLMS.Data;
using ST10448420_TechMove_GLMS.Patterns.Builder;

namespace ST10448420_TechMove_GLMS.Controllers
{
    public class ServiceRequestController : Controller
    {
    private readonly ApplicationDbContext _context;

        public ServiceRequestController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Create(int contractId)
        {
            ViewBag.Contract = _context.Contracts.Find(contractId);
            return View();
        }
        [HttpPost]
        public IActionResult Create(int contractId, string description, decimal costUSD)
        {
            var _contract = _context.Contracts.Find(contractId);

            var builder = new ServiceRequestBuilder();
            var director = new ServiceRequestDirector();

            try
            {
                var _sRequest = director.Construct(builder, _contract, description, costUSD);

                _context.ServiceRequests.Add(_sRequest);
                _context.SaveChanges();

                return RedirectToAction("Index", "ClientDashboard");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }
        public IActionResult Index()
        {
            return View();
        }

    }
}
