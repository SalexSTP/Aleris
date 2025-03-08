using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aleris.Controllers
{
    public class MembersController : BaseController
    {
        public MembersController(ApplicationDbContext context) : base(context)
        {
        }

        [HttpGet]
        public async Task<IActionResult> EditMember(int id)
        {
            var member = await _context.CompanyMembers.FindAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            if (!IsUserLoggedIn() || !await UserHasAccessToCompany(member.CompanyId))
            {
                return RedirectToAction("Index", "Home");
            }

            var currentUserEmail = HttpContext.Session.GetString("UserEmail");
            var isAdmin = await _context.CompanyMembers
                .AnyAsync(m => m.CompanyId == member.CompanyId && m.User.Email == currentUserEmail && m.Role == UserRole.Admin);

            if (!isAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            // Pass available roles to the view
            ViewBag.Roles = new List<string> { "Admin", "Editor", "Viewer" };

            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMember(int id, CompanyMember model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var member = await _context.CompanyMembers.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            if (!IsUserLoggedIn() || !await UserHasAccessToCompany(member.CompanyId))
            {
                return RedirectToAction("Index", "Home");
            }

            // Check if the logged-in user is an admin of the company
            var currentUserEmail = HttpContext.Session.GetString("UserEmail");
            var isAdmin = await _context.CompanyMembers
                .AnyAsync(m => m.CompanyId == member.CompanyId && m.User.Email == currentUserEmail && m.Role == UserRole.Admin);

            if (!isAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            // Don't modify CompanyId, make sure it's preserved
            model.CompanyId = member.CompanyId;

            // Update Role
            member.Role = model.Role;

            try
            {
                _context.Update(member);
                await _context.SaveChangesAsync();
                return RedirectToAction("Members", "Company", new { id = member.CompanyId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeamMemberExists(member.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool TeamMemberExists(int id)
        {
            return _context.CompanyMembers.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMember(int id)
        {
            // Retrieve the logged-in user's ID from the session
            var userIdStr = HttpContext.Session.GetInt32("UserId");
            if (userIdStr == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect if the user is not logged in
            }

            var userId = userIdStr; // Convert to integer

            // Get the member that needs to be deleted
            var member = _context.CompanyMembers.FirstOrDefault(m => m.Id == id);
            if (member == null)
            {
                return NotFound(); // Return NotFound if the member doesn't exist
            }

            if (!IsUserLoggedIn() || !UserHasAccessToCompany(member.CompanyId).Result)
            {
                return RedirectToAction("Index", "Home");
            }

            // Check if the logged-in user is an admin of the company
            var isAdmin = _context.CompanyMembers.Any(m => m.CompanyId == member.CompanyId && m.User.Id == userId && m.Role == UserRole.Admin);

            if (!isAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            // Check if the logged-in user is allowed to delete the member (e.g., they are not the member themselves)
            if (member.UserId == userId)
            {
                // You can handle a situation where the user is trying to delete their own account
                TempData["ErrorMessage"] = "You cannot delete your own profile.";
                return RedirectToAction("Members", "Company", new { id = member.CompanyId }); // Redirect back to the members page
            }

            // Otherwise, delete the member
            _context.CompanyMembers.Remove(member);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Member successfully deleted.";
            return RedirectToAction("Members", "Company", new { id = member.CompanyId });
        }

        [HttpGet]
        public IActionResult InviteMember(int companyId)
        {
            var company = _context.Companies.Include(c => c.TeamMembers).FirstOrDefault(c => c.Id == companyId);

            if (company == null)
            {
                return NotFound();
            }

            if (!IsUserLoggedIn() || !UserHasAccessToCompany(companyId).Result)
            {
                return RedirectToAction("Index", "Home");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = company.TeamMembers.Any(m => m.UserId == currentUserId && m.Role == UserRole.Admin);

            if (!isAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            // Pass roles to the view
            ViewBag.Roles = new List<string> { "Admin", "Editor", "Viewer" };

            return View(companyId);
        }

        [HttpPost]
        public IActionResult SendInvite(CompanyMember model, string role)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrEmpty(role))
            {
                TempData["ErrorMessage"] = "Всички полета са задължителни.";
                return RedirectToAction("Members", "Company", new { companyId = model.CompanyId });
            }

            var company = _context.Companies.Include(c => c.TeamMembers)
                                            .FirstOrDefault(c => c.Id == model.CompanyId);
            if (company == null)
            {
                return NotFound();
            }

            if (!IsUserLoggedIn() || !UserHasAccessToCompany(model.CompanyId).Result)
            {
                return RedirectToAction("Index", "Home");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = company.TeamMembers.Any(m => m.UserId == currentUserId && m.Role == UserRole.Admin);

            if (!isAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                if (company.TeamMembers.Any(m => m.UserId == existingUser.Id))
                {
                    TempData["ErrorMessage"] = "Този потребител вече е член на екипа.";
                    return RedirectToAction("Members", "Company", new { id = model.CompanyId });
                }

                var existingInvite = _context.Invites.FirstOrDefault(i => i.UserId == existingUser.Id && i.CompanyId == model.CompanyId);
                if (existingInvite != null)
                {
                    TempData["ErrorMessage"] = "Вече има изпратена покана към този потребител.";
                    return RedirectToAction("Members", "Company", new { id = model.CompanyId });
                }

                var invite = new Invite
                {
                    UserId = existingUser.Id,
                    CompanyId = model.CompanyId,
                    Status = InviteStatus.Pending,
                    Role = Enum.Parse<UserRole>(role) // Set role from the form
                };

                _context.Invites.Add(invite);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Поканата е изпратена успешно!";
            }
            else
            {
                TempData["ErrorMessage"] = "Не съществува потребител с този имейл.";
            }

            return RedirectToAction("Members", "Company", new { id = model.CompanyId });
        }
    }
}
