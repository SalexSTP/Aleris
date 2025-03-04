using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aleris.Data;
using Aleris.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Aleris.Controllers
{
    public class StatisticsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context) : base (context)
        {
        }

        public async Task<IActionResult> Index(int companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
            {
                return NotFound();
            }

            var purchases = await _context.CompanyPurchases
                .Where(p => p.CompanyId == companyId)
                .OrderBy(p => p.Date)
                .ToListAsync();

            var sales = await _context.CompanySales
                .Where(s => s.CompanyId == companyId)
                .OrderBy(s => s.Date)
                .ToListAsync();

            var dates = purchases.Select(p => p.Date)
                .Union(sales.Select(s => s.Date))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var purchasesData = dates.Select(date => purchases
                .Where(p => p.Date.Date == date.Date)
                .Sum(p => p.Quantity)).ToList();

            var salesData = dates.Select(date => sales
                .Where(s => s.Date.Date == date.Date)
                .Sum(s => s.Quantity)).ToList();

            var model = new CompanyStatistics(purchases, sales);


            return View(model);
        }
    }
}
