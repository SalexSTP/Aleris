using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aleris.Controllers
{
    public class SalesController : BaseController
    {
        public SalesController(ApplicationDbContext context) : base(context)
        {
        }

        [HttpGet]
        public IActionResult AddSale(int companyId)
        {
            var company = _context.Companies
                .Include(c => c.Storage)
                .FirstOrDefault(c => c.Id == companyId);

            if (company == null)
            {
                ViewBag.ErrorMessage = "Company not found!";
                return View("Error");
            }

            ViewBag.StorageProducts = company.Storage
                .Select(p => new { p.Id, p.ProductName, p.UnitType, p.Quantity })
                .ToList();

            ViewBag.CompanyId = companyId;

            return View(new CompanySale { CompanyId = companyId });
        }

        [HttpPost]
        public async Task<IActionResult> AddSale(int companyId, CompanySale sale)
        {
            if (!ModelState.IsValid)
            {
                var company = _context.Companies
                    .Include(c => c.Storage)
                    .FirstOrDefault(c => c.Id == companyId);

                if (company == null)
                {
                    return NotFound();
                }

                ViewBag.StorageProducts = company.Storage
                    .Select(p => new { p.Id, p.ProductName, p.UnitType, p.Quantity })
                    .ToList();

                ViewBag.CompanyId = companyId;

                return View(sale);
            }

            var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
            if (!companyExists)
            {
                ModelState.AddModelError("CompanyNotFound", "The company does not exist.");
                return View(sale);
            }

            var product = await _context.CompanyStorages
                .FirstOrDefaultAsync(p => p.Id == sale.ProductId && p.CompanyId == companyId);

            if (product == null)
            {
                ModelState.AddModelError("ProductNotFound", "The selected product does not exist.");
                return View(sale);
            }

            if (sale.Quantity > product.Quantity)
            {
                ModelState.AddModelError("QuantityExceeded", "Not enough stock available.");
                return View(sale);
            }

            // Assign unit type from storage
            sale.UnitType = product.UnitType;
            sale.Company = _context.Companies.First(c => c.Id.Equals(companyId));
            sale.Storage = _context.CompanyStorages.First(c => c.Id.Equals(product.Id));

            // Calculate total price
            sale.CalculateTotalPrice();

            // Update storage quantity and recalculate total price
            product.Quantity -= sale.Quantity;
            product.CalculateTotalPrice();
            product.CalculateAveragePrice();

            _context.CompanyStorages.Update(product);
            _context.CompanySales.Add(sale);
            await _context.SaveChangesAsync();

            return RedirectToAction("Sales", "Company", new { id = companyId });
        }
    }
}
