using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Aleris.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Purchases
        public IActionResult Index(int companyId)
        {
            var company = _context.Companies
                .Include(c => c.Purchases) // Load purchases
                .FirstOrDefault(c => c.Id == companyId);

            if (company == null)
            {
                return NotFound();
            }

            return View(company.Purchases);
        }

        [HttpGet]
        public IActionResult AddPurchase(int companyId)
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

            return View(new CompanyPurchase { CompanyId = companyId });
        }

        [HttpPost]
        public async Task<IActionResult> AddPurchase(int companyId, CompanyPurchase purchase)
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

                return View(purchase);
            }

            var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
            if (!companyExists)
            {
                // Handle the case when the company doesn't exist
                ModelState.AddModelError("CompanyNotFound", "The company does not exist.");
                return View(purchase);  // Return view with error
            }

            // Check if a product with the same name already exists in this company's storage
            var existingProduct = await _context.CompanyStorages
                .FirstOrDefaultAsync(st => st.ProductName == purchase.Name && st.CompanyId == companyId);

            if (existingProduct != null)
            {
                // You can decide whether to update the existing product or add a new entry
                existingProduct.UpdateStorageOnPurchase(purchase.Quantity, purchase.ProductPrice * purchase.Quantity);
                _context.CompanyStorages.Update(existingProduct);
            }
            else
            {
                // If no existing product, create a new one
                var storage = new CompanyStorage
                {
                    CompanyId = companyId,  // Ensure CompanyId is set
                    ProductName = purchase.Name,
                    Quantity = purchase.Quantity,
                    ProductPrice = purchase.ProductPrice
                };

                storage.CalculateTotalPrice();
                _context.CompanyStorages.Add(storage);
            }

            // Handle purchase addition
            purchase.CompanyId = companyId;
            purchase.ProductId = existingProduct?.Id ?? _context.CompanyStorages.OrderByDescending(p => p.Id).FirstOrDefault()?.Id;
            purchase.CalculateTotalPrice();

            _context.CompanyPurchases.Add(purchase);
            await _context.SaveChangesAsync();

            return RedirectToAction("Purchases", "Company", new { id = companyId });
        }
    }
}
