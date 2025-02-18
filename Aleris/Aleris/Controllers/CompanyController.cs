using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

                // Redirect to ConfigureSettings page, passing companyId as a parameter
                return RedirectToAction("ConfigureSettings", "Company", new { companyId = company.Id });
            }

            return View(company);
        }
         
        [HttpGet]
        public async Task<IActionResult> ConfigureSettings(int companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
            {
                return NotFound();
            }

            var settings = new CompanySettings { Company = company };
            return View("ConfigureSettings", settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSettings(CompanySettings settings)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfigureSettings", settings);
            }

            // Check if CompanyId is present
            if (settings.CompanyId == 0)
            {
                return BadRequest("Company ID is missing.");
            }

            // Retrieve the company from the database
            var company = await _context.Companies
                .Include(c => c.CompanySettings)
                .FirstOrDefaultAsync(c => c.Id == settings.CompanyId);

            if (company == null)
            {
                return NotFound("Company not found.");
            }

            // Assign the settings to the company
            company.CompanySettings = settings;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> CompanyMain(int id)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }

            return View("CompanyMain", company);
        }
    }
}
