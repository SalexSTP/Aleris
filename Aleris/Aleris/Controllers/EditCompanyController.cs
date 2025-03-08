using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aleris.Controllers
{
    public class EditCompanyController : BaseController
    {
        public EditCompanyController(ApplicationDbContext context) : base(context)
        {
        }

        [HttpGet]
        public async Task<IActionResult> EditCompanyAsync(int id)
        {
            // Check if the user is logged in and has access to the company
            if (!await UserHasAccessToCompany(id))
            {
                return RedirectToAction("Index", "Home"); // User is either not logged in or doesn't have access
            }

            // Check if the user has the required role (Admin)
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var hasAccess = await _context.CompanyMembers
                .AnyAsync(cm => cm.CompanyId == id && cm.User.Email == userEmail &&
                                (cm.Role == UserRole.Admin || cm.Role == UserRole.Editor));

            if (!hasAccess)
            {
                return RedirectToAction("Index", "Home"); // User doesn't have admin or editor access
            }

            // Retrieve the company and settings from the database
            var company = _context.Companies
                                  .Include(c => c.CompanySettings) // Ensure settings are loaded too
                                  .FirstOrDefault(c => c.Id == id);

            if (company == null)
            {
                return NotFound(); // Return 404 if company doesn't exist
            }

            // Pass the company object to the view
            return View(company);
        }

        [HttpPost]
        public async Task<IActionResult> EditCompanyAsync(Company model)
        {
            if (!await UserHasAccessToCompany(model.Id))
            {
                return RedirectToAction("Index", "Home"); // User is either not logged in or doesn't have access
            }

            // Check if the user has the required role (Admin)
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var hasAccess = await _context.CompanyMembers
                .AnyAsync(cm => cm.CompanyId == model.Id && cm.User.Email == userEmail &&
                                (cm.Role == UserRole.Admin || cm.Role == UserRole.Editor));

            if (!hasAccess)
            {
                return RedirectToAction("Index", "Home"); // User doesn't have admin or editor access
            }

            if (ModelState.IsValid)
            {
                // Retrieve the existing company and its settings from the database
                var existingCompany = _context.Companies
                                              .Include(c => c.CompanySettings)
                                              .FirstOrDefault(c => c.Id == model.Id);

                if (existingCompany == null)
                {
                    return NotFound();
                }

                // Update company information (excluding Bulstat, as per your request)
                existingCompany.Name = model.Name;
                existingCompany.Bulstat = model.Bulstat;
                existingCompany.VatNumber = model.VatNumber;
                existingCompany.Address = model.Address;
                existingCompany.Manager = model.Manager;
                existingCompany.PhoneNumber = model.PhoneNumber;
                existingCompany.Email = model.Email;

                // Update company settings with all properties from the model
                if (model.CompanySettings != null)
                {
                    existingCompany.CompanySettings.IsVatRegistered = model.CompanySettings.IsVatRegistered;
                    existingCompany.CompanySettings.IsVatIncludedInPrices = model.CompanySettings.IsVatIncludedInPrices;
                    existingCompany.CompanySettings.PricePrecision = model.CompanySettings.PricePrecision;
                    existingCompany.CompanySettings.QuantityPrecision = model.CompanySettings.QuantityPrecision;
                    existingCompany.CompanySettings.MethodOfRevision = model.CompanySettings.MethodOfRevision;
                    existingCompany.CompanySettings.AutoProduction = model.CompanySettings.AutoProduction;
                    existingCompany.CompanySettings.WorkWithTraders = model.CompanySettings.WorkWithTraders;
                }

                // Update the company and settings in the database
                _context.Update(existingCompany);
                _context.SaveChanges();

                // Redirect to the settings page of the updated company
                return RedirectToAction("Settings", "Company", new { id = model.Id });
            }

            // If the model is not valid, return the same view with validation errors
            return View("EditCompany", model);
        }
    }
}
