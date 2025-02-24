﻿using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Aleris.Controllers
{
    public class CompanyController : BaseController
    {
        public CompanyController(ApplicationDbContext context) : base(context)
        {
        }

        private async Task<bool> UserHasAccessToCompany(int companyId)
        {
            string? userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return false;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return false;

            return await _context.CompanyMembers
                .AnyAsync(cm => cm.UserId == user.Id && cm.CompanyId == companyId);
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

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                string? userEmail = HttpContext.Session.GetString("UserEmail");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user != null)
                {
                    var companyMember = new CompanyMember
                    {
                        UserId = user.Id,
                        CompanyId = company.Id,
                        Role = UserRole.Admin
                    };

                    _context.CompanyMembers.Add(companyMember);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("ConfigureSettings", "Company", new { companyId = company.Id });
            }

            return View(company);
        }

        [HttpGet]
        public async Task<IActionResult> ConfigureSettings(int companyId)
        {
            if (!await UserHasAccessToCompany(companyId))
            {
                return RedirectToAction("Index", "Home");
            }

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

            if (settings.CompanyId == 0)
            {
                return BadRequest("Company ID is missing.");
            }

            if (!await UserHasAccessToCompany(settings.CompanyId))
            {
                return RedirectToAction("Index", "Home");
            }

            var company = await _context.Companies
                .Include(c => c.CompanySettings)
                .FirstOrDefaultAsync(c => c.Id == settings.CompanyId);

            if (company == null)
            {
                return NotFound("Company not found.");
            }

            company.CompanySettings = settings;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Statistics(int id)
        {
            if (!await UserHasAccessToCompany(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            ViewData["IsCompanyPage"] = true;
            return View("Statistics", company);
        }

        [HttpGet]
        public async Task<IActionResult> Purchases(int id)
        {
            if (!await UserHasAccessToCompany(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var company = _context.Companies
                .Include(c => c.Purchases)  // Ensure Purchases are included
                .FirstOrDefault(c => c.Id == id);

            if (company == null)
            {
                return NotFound();
            }

            ViewData["IsCompanyPage"] = true;
            return View("Purchases", company);
        }

        [HttpGet]
        public async Task<IActionResult> Storage(int id)
        {
            if (!await UserHasAccessToCompany(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            ViewData["IsCompanyPage"] = true;
            return View("Storage", company);
        }

        [HttpGet]
        public async Task<IActionResult> Sales(int id)
        {
            if (!await UserHasAccessToCompany(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            ViewData["IsCompanyPage"] = true;
            return View("Sales", company);
        }

        [HttpGet]
        public async Task<IActionResult> Members(int id)
        {
            if (!await UserHasAccessToCompany(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            ViewData["IsCompanyPage"] = true;
            return View("Members", company);
        }

        [HttpGet]
        public async Task<IActionResult> Settings(int id)
        {
            if (!await UserHasAccessToCompany(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            ViewData["IsCompanyPage"] = true;
            return View("Settings", company);
        }
    }
}
