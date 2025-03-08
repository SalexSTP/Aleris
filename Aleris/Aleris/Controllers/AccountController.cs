using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Aleris.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(ApplicationDbContext context) : base(context)
        {
        }

        public bool isLogged = false;

        // GET: Register
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Index", "Home"); // Redirect if already logged in
            }

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

                user.Password = HashPassword(user.Password);

                _context.Users.Add(user);
                _context.SaveChanges();

                // Store user details in session
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetInt32("UserId", user.Id);
                isLogged = true;

                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Index", "Home"); // Redirect if already logged in
            }

            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);

            if (existingUser == null || existingUser.Password != HashPassword(user.Password))
            {
                ModelState.AddModelError(string.Empty, "Грешен имейл или парола.");
                return View(user); // Return the same view to show validation messages
            }

            // Store user details in session
            HttpContext.Session.SetString("UserEmail", existingUser.Email);
            HttpContext.Session.SetString("UserName", existingUser.Name);
            HttpContext.Session.SetInt32("UserId", existingUser.Id);
            isLogged = true;

            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            isLogged = false;
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
