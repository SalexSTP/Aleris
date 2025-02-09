using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Aleris.Controllers
{
    public class CompanyController : Controller
    {
        protected readonly ApplicationDbContext _context;

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreateCompany()
        {
            return View(new Company());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCompany(Company company)
        {
            if (ModelState.IsValid)
            {
                
                var existingCompany = await _context.Companies
                    .FirstOrDefaultAsync(c => c.Bulstat == company.Bulstat || c.Name == company.Name);

                if (existingCompany != null)
                {
                    if (existingCompany.Bulstat == company.Bulstat)
                        ModelState.AddModelError("Bulstat", "A company with this BULSTAT already exists.");
                    if (existingCompany.Name == company.Name)
                        ModelState.AddModelError("Name", "A company with this Name already exists.");

                    return View(company); 
                }

                // Add the company to the database
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                // Add the current user as a CompanyMember with Admin role
                string? userEmail = HttpContext.Session.GetString("UserEmail");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user != null)
                {
                    var companyMember = new CompanyMember
                    {
                        UserId = user.Id,
                        CompanyId = company.Id,
                        Role = UserRole.Admin // Assigning Admin role
                    };

                    _context.CompanyMembers.Add(companyMember);
                    await _context.SaveChangesAsync();
                }

                // Store the company in TempData and keep it for next request
                TempData["Company"] = JsonConvert.SerializeObject(company);
                TempData.Keep("Company");

                // Redirect to ConfigureSettings page
                return RedirectToAction("ConfigureSettings", "Company");
            }

            return View(company);
        }


        [HttpGet]
        public IActionResult ConfigureSettings(int companyId)
        {
            // Retrieve company from TempData
            var companyJson = TempData["Company"]?.ToString();
            if (companyJson == null)
            {
                return NotFound();
            }

            var company = JsonConvert.DeserializeObject<Company>(companyJson);
            TempData["Company"] = JsonConvert.SerializeObject(company);
            TempData.Keep("Company");

            var settings = new CompanySettings { Company = company };
            return View("ConfigureSettings", settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSettingsAsync(CompanySettings settings)
        {
            if (!ModelState.IsValid)
            {
                // Handle invalid settings
                return View(settings);
            }

            // Retrieve the company object from TempData and assign settings to it
            var companyJson = TempData["Company"].ToString();
            var company = JsonConvert.DeserializeObject<Company>(companyJson);
            company.CompanySettings = settings;

            // Add the company with the configured settings to the database
            _context.Companies.Add(company);
            _context.SaveChanges();

            // Redirect to the next step, e.g., add team or finish
            return RedirectToAction("Index", "Home");
        }
    }
}
