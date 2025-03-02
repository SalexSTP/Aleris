using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Aleris.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        : base(context)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return View();
            }

            var userCompanies = _context.CompanyMembers
                .Where(cm => cm.User.Email == userEmail)
                .Select(cm => cm.Company)
                .ToList();

            return View(userCompanies);
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
