using System;
using System.Collections.Generic;
using System.Linq;

namespace Aleris.Models
{
    public class CompanyStatistics
    {
        public string CompanyName { get; set; } = string.Empty;
        public List<DateTime> Dates { get; set; } = new();
        public List<decimal> Purchases { get; set; } = new();
        public List<decimal> Orders { get; set; } = new();

        public CompanyStatistics(List<CompanyPurchase> purchases, List<CompanySale> sales)
        {
            if (purchases.Any())
                CompanyName = purchases.First().Company?.Name ?? "Unknown";
            else if (sales.Any())
                CompanyName = sales.First().Company?.Name ?? "Unknown";

            var allDates = purchases.Select(p => p.Date).Concat(sales.Select(s => s.Date)).Distinct().OrderBy(d => d).ToList();
            Dates = allDates;

            foreach (var date in allDates)
            {
                Purchases.Add(purchases.Where(p => p.Date.Date == date.Date).Sum(p => p.TotalPrice));
                Orders.Add(sales.Where(s => s.Date.Date == date.Date).Sum(s => s.TotalPrice));
            }
        }

        
    }
}
