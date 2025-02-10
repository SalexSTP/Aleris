using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Aleris.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        // Inject the ApplicationDbContext in the constructor
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        private void PopulateCompaniesInViewBag()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(userEmail))
            {
                var userCompanies = _context.CompanyMembers
                    .Where(cm => cm.User.Email == userEmail)
                    .Select(cm => cm.Company)
                    .ToList();

                ViewBag.Companies = userCompanies;
            }
            else
            {
                ViewBag.Companies = new List<Company>();
            }
        }

        public IActionResult Index()
        {
            PopulateCompaniesInViewBag();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
