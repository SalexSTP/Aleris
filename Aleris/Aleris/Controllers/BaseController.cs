using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Aleris.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected void PopulateCompaniesInViewBag()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(userEmail))
            {
                var userCompanies = _context.CompanyMembers
                    .Where(cm => cm.User.Email == userEmail)
                    .Select(cm => cm.Company)
                    .ToList();

                ViewBag.Companies = userCompanies;
            }
            else
            {
                ViewBag.Companies = new List<Company>();
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            PopulateCompaniesInViewBag();
            base.OnActionExecuting(context);
        }
    }
}
