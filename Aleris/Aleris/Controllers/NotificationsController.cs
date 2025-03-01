using Aleris.Data;
using Aleris.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace Aleris.Controllers
{
    public class NotificationsController : BaseController
    {
        public NotificationsController(ApplicationDbContext context) : base(context)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.Invites)  // Include the invites
                .ThenInclude(i => i.Company)  // Include the company related to each invite
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            var pendingInvites = user.Invites.Where(i => i.Status == InviteStatus.Pending).ToList();

            // Pass the pending invites to the view
            return View(pendingInvites);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptInvite(int id)
        {
            var invite = await _context.Invites
                .Include(i => i.Company)  // Ensure Company is loaded to access its details
                .Include(i => i.User) // Ensure User is loaded to access its details
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invite == null)
            {
                return NotFound();
            }

            // Check if the user is already a member of the company
            var existingMember = await _context.CompanyMembers
                .FirstOrDefaultAsync(cm => cm.UserId == invite.UserId && cm.CompanyId == invite.CompanyId);

            if (existingMember == null)
            {
                // If the user is not already a member, create a new company member
                var newMember = new CompanyMember
                {
                    UserId = invite.UserId,
                    User = _context.Users.First(c => c.Id.Equals(invite.UserId)),
                    CompanyId = invite.CompanyId,
                    Company = _context.Companies.First(c => c.Id.Equals(invite.CompanyId)),
                    Role = invite.Role,
                    Status = MemberStatus.In
                };

                newMember.Name = newMember.User.Name;
                newMember.Email = newMember.User.Email;

                _context.CompanyMembers.Add(newMember);
            }

            // Save changes to the database
            _context.Invites.Remove(invite);

            await _context.SaveChangesAsync();

            // Redirect the user back to the notifications page
            return RedirectToAction("Notifications");
        }


        [HttpPost]
        public async Task<IActionResult> RejectInvite(int id)
        {
            var invite = await _context.Invites.FindAsync(id);

            if (invite == null)
            {
                return NotFound();
            }

            // Remove the invitation from the database
            _context.Invites.Remove(invite);
            await _context.SaveChangesAsync();

            // Redirect to Notifications or any other page
            return RedirectToAction("Notifications");
        }
    }
}
