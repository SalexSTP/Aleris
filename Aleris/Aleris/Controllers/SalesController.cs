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
        public async Task<IActionResult> AddSaleAsync(int companyId)
        {
            // Check if the user is logged in and has access to the company
            if (!IsUserLoggedIn() || !await UserHasAccessToCompany(companyId))
            {
                return RedirectToAction("Index", "Home");
            }

            var company = _context.Companies
                .Include(c => c.Storage)
                .FirstOrDefault(c => c.Id == companyId);

            if (company == null)
            {
                ViewBag.ErrorMessage = "Company not found!";
                return View("Error");
            }

            // Check if the user has the required role (Admin or Editor) for the company
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var hasAccess = await _context.CompanyMembers
                .AnyAsync(cm => cm.CompanyId == companyId && cm.User.Email == userEmail &&
                                (cm.Role == UserRole.Admin || cm.Role == UserRole.Editor));

            if (!hasAccess)
            {
                return RedirectToAction("Index", "Home");
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

        [HttpGet]
        public async Task<IActionResult> EditSaleAsync(int id)
        {
            // Check if the user is logged in
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            // Fetch the sale details from the database
            var sale = await _context.CompanySales
                .Include(s => s.Company)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                ViewBag.ErrorMessage = "Продажбата не беше намерена.";
                return View("Error");
            }

            var companyId = sale.CompanyId;

            // Check if the user has access to the company
            if (!await UserHasAccessToCompany(companyId))
            {
                return RedirectToAction("Index", "Home");
            }

            // Fetch product details from storage for this company
            var storageProduct = await _context.CompanyStorages
                .Where(p => p.CompanyId == companyId && p.Id == sale.ProductId)
                .Select(p => new { p.ProductName, p.Quantity, p.Id })
                .FirstOrDefaultAsync();

            if (storageProduct == null)
            {
                ViewBag.ErrorMessage = "Продуктът не беше намерен в склада.";
                return View("Error");
            }

            // Calculate max quantity (storage quantity + current sale quantity)
            var maxQuantity = storageProduct.Quantity + sale.Quantity; // Total available quantity

            // Set necessary ViewBag values
            ViewBag.CompanyId = companyId;
            ViewBag.StorageProducts = new List<dynamic> { storageProduct };
            ViewBag.MaxQuantity = maxQuantity; // Total available quantity

            // Pass the sale to the view for editing
            return View("EditSale", sale);
        }

        [HttpPost]
        public async Task<IActionResult> EditSale(CompanySale sale)
        {
            if (!ModelState.IsValid)
            {
                var storageProducts = await _context.CompanyStorages
                    .Where(s => s.CompanyId == sale.CompanyId)
                    .Select(s => s.ProductName)
                    .ToListAsync();

                ViewBag.StorageProducts = storageProducts;
                ViewBag.CompanyId = sale.CompanyId;

                return View("EditSale", sale);
            }

            var existingSale = await _context.CompanySales
                .FirstOrDefaultAsync(s => s.Id == sale.Id);

            if (existingSale == null)
            {
                ViewBag.ErrorMessage = "Продажбата не беше намерена.";
                return View("Error");
            }

            if (!await UserHasAccessToCompany(existingSale.CompanyId))
            {
                return RedirectToAction("Index", "Home");
            }

            var product = await _context.CompanyStorages
                .FirstOrDefaultAsync(p => p.Id == existingSale.ProductId && p.CompanyId == existingSale.CompanyId);

            if (product == null)
            {
                ViewBag.ErrorMessage = "Продуктът в склада не беше намерен.";
                return View("Error");
            }

            // STEP 1: Rollback the old sale
            product.UpdateStorageOnSale(-existingSale.Quantity);

            // STEP 2: Calculate new sale values
            existingSale.UnitType = sale.UnitType;
            existingSale.ProductPrice = sale.ProductPrice;

            // Apply unit-based quantity formatting
            existingSale.Quantity = sale.UnitType == "бр."
                ? (int)Math.Ceiling(sale.Quantity)
                : sale.Quantity;

            existingSale.CalculateTotalPrice();

            // STEP 3: Check if applying new sale will result in negative quantity in storage
            decimal tempQuantity = product.Quantity - existingSale.Quantity;
            if (tempQuantity < 0)
            {
                // Rollback the rollback to restore original storage state
                product.UpdateStorageOnSale(-existingSale.Quantity);

                ViewBag.StorageProducts = await _context.CompanyStorages
                    .Where(s => s.CompanyId == existingSale.CompanyId)
                    .Select(s => s.ProductName)
                    .ToListAsync();

                ViewBag.CompanyId = existingSale.CompanyId;
                ModelState.AddModelError("", "Редакцията ще доведе до отрицателно количество в склада.");
                return View("EditSale", existingSale);
            }

            // STEP 4: Apply new values
            product.UpdateStorageOnSale(existingSale.Quantity);

            _context.CompanyStorages.Update(product);
            _context.CompanySales.Update(existingSale);
            await _context.SaveChangesAsync();

            return RedirectToAction("Sales", "Company", new { id = existingSale.CompanyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSale(int id)
        {
            var sale = await _context.CompanySales
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null || !await UserHasAccessToCompany(sale.CompanyId))
            {
                return RedirectToAction("Index", "Home");
            }

            var storage = await _context.CompanyStorages.FirstOrDefaultAsync(s => s.Id == sale.ProductId);

            if (storage == null)
            {
                TempData["Error"] = "Продуктът не е намерен в склада.";
                return RedirectToAction("Sales", "Company", new { id = sale.CompanyId });
            }

            // Restore the sold quantity and total price to storage
            storage.UpdateStorageOnSale(-sale.Quantity);

            _context.CompanyStorages.Update(storage);
            _context.CompanySales.Remove(sale);

            await _context.SaveChangesAsync();

            return RedirectToAction("Sales", "Company", new { id = sale.CompanyId });
        }

    }
}
