using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aleris.Controllers
{
    public class PurchasesController : BaseController
    {
        public PurchasesController(ApplicationDbContext context) : base(context)
        {
        }

        [HttpGet]
        public async Task<IActionResult> AddPurchaseAsync(int companyId)
        {
            if (!IsUserLoggedIn() || !UserHasAccessToCompany(companyId).Result)
            {
                return RedirectToAction("Index", "Home");
            }

            var company = _context.Companies
                .Include(c => c.Storage)
                .FirstOrDefault(c => c.Id == companyId);

            if (company == null)
            {
                // Return a view with an error message or something similar instead of NotFound()
                ViewBag.ErrorMessage = "Компанията не беше намерена!";
                return View("Error"); // Assuming you have an Error view to display the message
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
                ModelState.AddModelError("CompanyNotFound", "Компанията не съществува.");
                return View(purchase);  // Return view with error
            }

            // Edit Quantity by the Unit Type
            if (purchase.UnitType == "бр.")
            {
                purchase.Quantity = (int)Math.Ceiling(purchase.Quantity);
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
                    Company = _context.Companies.First(c => c.Id.Equals(companyId)),
                    ProductName = purchase.Name,
                    Quantity = purchase.Quantity,
                    ProductPrice = purchase.ProductPrice,
                    UnitType = purchase.UnitType
                };

                storage.CalculateTotalPrice();
                _context.CompanyStorages.Add(storage);
                await _context.SaveChangesAsync();
            }

            // Handle purchase addition
            purchase.CompanyId = companyId;
            purchase.Company = _context.Companies.First(c => c.Id.Equals(companyId));

            if (existingProduct == null)
            {
                purchase.ProductId = _context.CompanyStorages.OrderByDescending(p => p.Id).FirstOrDefault().Id;
            }
            else
            {
                purchase.ProductId = existingProduct.Id;
            }

            purchase.CalculateTotalPrice();

            _context.CompanyPurchases.Add(purchase);
            await _context.SaveChangesAsync();

            return RedirectToAction("Purchases", "Company", new { id = companyId });
        }

        [HttpGet]
        public async Task<IActionResult> EditPurchaseAsync(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            var purchase = await _context.CompanyPurchases
                .Include(p => p.Company)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                ViewBag.ErrorMessage = "Покупката не беше намерена.";
                return View("Error");
            }

            var companyId = purchase.CompanyId;

            if (!await UserHasAccessToCompany(companyId))
            {
                return RedirectToAction("Index", "Home");
            }

            var storageProducts = await _context.CompanyStorages
                .Where(p => p.CompanyId == companyId)
                .Select(p => p.ProductName)
                .ToListAsync();

            ViewBag.StorageProducts = storageProducts;
            ViewBag.CompanyId = companyId;

            return View("EditPurchase", purchase);
        }

        [HttpPost]
        public async Task<IActionResult> EditPurchase(CompanyPurchase purchase)
        {
            if (!ModelState.IsValid)
            {
                var storageProducts = await _context.CompanyStorages
                    .Where(s => s.CompanyId == purchase.CompanyId)
                    .Select(s => s.ProductName)
                    .ToListAsync();

                ViewBag.StorageProducts = storageProducts;
                ViewBag.CompanyId = purchase.CompanyId;

                return View("EditPurchase", purchase);
            }

            var existingPurchase = await _context.CompanyPurchases
                .FirstOrDefaultAsync(p => p.Id == purchase.Id);

            if (existingPurchase == null)
            {
                ViewBag.ErrorMessage = "Покупката не беше намерена.";
                return View("Error");
            }

            if (!await UserHasAccessToCompany(existingPurchase.CompanyId))
            {
                return RedirectToAction("Index", "Home");
            }

            var product = await _context.CompanyStorages
                .FirstOrDefaultAsync(p => p.Id == existingPurchase.ProductId && p.CompanyId == existingPurchase.CompanyId);

            if (product == null)
            {
                ViewBag.ErrorMessage = "Продуктът в склада не беше намерен.";
                return View("Error");
            }

            // STEP 1: Rollback the old purchase
            product.UpdateStorageOnPurchase(-existingPurchase.Quantity, -existingPurchase.TotalPrice);

            // STEP 2: Calculate new purchase values
            existingPurchase.UnitType = purchase.UnitType;
            existingPurchase.ProductPrice = purchase.ProductPrice;

            // Apply unit-based quantity formatting
            existingPurchase.Quantity = purchase.UnitType == "бр."
                ? (int)Math.Ceiling(purchase.Quantity)
                : purchase.Quantity;

            existingPurchase.CalculateTotalPrice();

            // STEP 3: Check if applying new purchase will result in negative quantity
            decimal tempQuantity = product.Quantity + existingPurchase.Quantity;
            if (tempQuantity < 0)
            {
                // Rollback the rollback to restore original storage state
                product.UpdateStorageOnPurchase(existingPurchase.Quantity, existingPurchase.TotalPrice);

                ViewBag.StorageProducts = await _context.CompanyStorages
                    .Where(s => s.CompanyId == purchase.CompanyId)
                    .Select(s => s.ProductName)
                    .ToListAsync();

                ViewBag.CompanyId = purchase.CompanyId;
                ModelState.AddModelError("", "Редакцията ще доведе до отрицателно количество в склада.");
                return View("EditPurchase", existingPurchase);
            }

            // STEP 4: Apply new values
            product.UpdateStorageOnPurchase(existingPurchase.Quantity, existingPurchase.TotalPrice);

            _context.CompanyStorages.Update(product);
            _context.CompanyPurchases.Update(existingPurchase);
            await _context.SaveChangesAsync();

            return RedirectToAction("Purchases", "Company", new { id = existingPurchase.CompanyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            var purchase = await _context.CompanyPurchases
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null || !await UserHasAccessToCompany(purchase.CompanyId))
            {
                return RedirectToAction("Index", "Home");
            }

            var storage = await _context.CompanyStorages.FirstOrDefaultAsync(s => s.Id == purchase.ProductId);

            if (storage == null)
            {
                TempData["Error"] = "Продуктът не е намерен в склада.";
                return RedirectToAction("Purchases", "Company", new { id = purchase.CompanyId });
            }

            // Check for negative quantity
            if (storage.Quantity - purchase.Quantity < 0)
            {
                TempData["Error"] = "Не може да изтриете покупката, защото това ще доведе до отрицателно количество в склада.";
                return RedirectToAction("Purchases", "Company", new { id = purchase.CompanyId });
            }

            // Apply update
            storage.UpdateStorageOnPurchase(-purchase.Quantity, -purchase.TotalPrice);

            _context.CompanyStorages.Update(storage);
            _context.CompanyPurchases.Remove(purchase);

            await _context.SaveChangesAsync();

            return RedirectToAction("Purchases", "Company", new { id = purchase.CompanyId });
        }
    }
}
