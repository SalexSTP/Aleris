using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Aleris.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Register
        public IActionResult Register()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError(string.Empty, "Потребител с този имейл вече съществува.");
                    return View(user);
                }

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }
        public IActionResult Login()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(User user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);

                if (existingUser == null || existingUser.Password != user.Password)
                {
                    ModelState.AddModelError(string.Empty, "Грешен имейл или парола.");
                    return View(user); // Return the same view to show validation messages
                }

                // Store user details in session
                HttpContext.Session.SetString("UserEmail", existingUser.Email);
                HttpContext.Session.SetString("UserName", existingUser.Name);
                HttpContext.Session.SetString("UserId", existingUser.Id.ToString());

                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }


    }
}
