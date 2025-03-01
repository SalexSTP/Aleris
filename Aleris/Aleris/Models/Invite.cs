using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aleris.Models
{
    public class Invite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; } // The invited user

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public Company? Company { get; set; } // The company sending the invite

        [Required]
        public InviteStatus Status { get; set; } = InviteStatus.Pending;

        [Required]
        public DateTime DateSent { get; set; } = DateTime.Now;

        [Required]
        public UserRole Role { get; set; } = UserRole.Viewer;
    }

    public enum InviteStatus
    {
        Pending  // Awaiting user response
    }
}
