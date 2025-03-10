﻿using Aleris.Data;
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
                ViewBag.ErrorMessage = "Company not found!";
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
                ModelState.AddModelError("CompanyNotFound", "The company does not exist.");
                return View(purchase);  // Return view with error
            }

            // Edit Quantity by the Unit Type
            if (purchase.UnitType == "Num.")
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
    }
}
