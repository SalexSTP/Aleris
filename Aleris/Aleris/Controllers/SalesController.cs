using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Aleris.Controllers
{
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sales
        public IActionResult Index(int companyId)
        {
            var company = _context.Companies
                .Include(c => c.Sales) // Load sales
                .FirstOrDefault(c => c.Id == companyId);

            if (company == null)
            {
                return NotFound();
            }

            return View(company.Sales);
        }

        [HttpGet]
        public IActionResult AddSale(int companyId)
        {
            var company = _context.Companies
                .Include(c => c.Storage)
                .FirstOrDefault(c => c.Id == companyId);

            if (company == null)
            {
                // Return a view with an error message or something similar instead of NotFound()
                ViewBag.ErrorMessage = "Company not found!";
                return View("Error"); // Assuming you have an Error view to display the message
            }

            ViewBag.StorageProducts = company.Storage.Select(p => p.ProductName).ToList();
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

                ViewBag.StorageProducts = company.Storage.Select(p => p.ProductName).ToList();
                ViewBag.CompanyId = companyId;

                return View(sale);
            }

            var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
            if (!companyExists)
            {
                // Handle the case when the company doesn't exist
                ModelState.AddModelError("CompanyNotFound", "The company does not exist.");
                return View(sale);  // Return view with error
            }

            // Edit Quantity by the Unit Type
            if (sale.UnitType == "Num.")
            {
                sale.Quantity = (int)Math.Ceiling(sale.Quantity);
            }

            // Check if a product with the same name already exists in this company's storage
            var existingProduct = await _context.CompanyStorages
                .FirstOrDefaultAsync(st => st.ProductName == sale.Name && st.CompanyId == companyId);

            if (existingProduct != null)
            {
                // You can decide whether to update the existing product or add a new entry
                existingProduct.UpdateStorageOnSale(sale.Quantity, sale.ProductPrice * sale.Quantity);
                _context.CompanyStorages.Update(existingProduct);
            }
            else
            {
                // If no existing product, create a new one
                var storage = new CompanyStorage
                {
                    CompanyId = companyId,  // Ensure CompanyId is set
                    ProductName = sale.Name,
                    Quantity = sale.Quantity,
                    ProductPrice = sale.ProductPrice,
                    UnitType = sale.UnitType
                };

                storage.CalculateTotalPrice();
                _context.CompanyStorages.Add(storage);
            }

            // Handle sale addition
            sale.CompanyId = companyId;
            sale.ProductId = existingProduct?.Id ?? _context.CompanyStorages.OrderByDescending(p => p.Id).FirstOrDefault()?.Id;
            sale.CalculateTotalPrice();

            _context.CompanySales.Add(sale);
            await _context.SaveChangesAsync();

            return RedirectToAction("Sales", "Company", new { id = companyId });
        }
    }
}
