using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aleris.Models
{
    public class CompanyMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public Company? Company { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Viewer;

        [Required]
        public MemberStatus Status { get; set; } = MemberStatus.PendingInvite;
    }

    public enum UserRole
    {
        Admin,  // Създал фирмата или има пълни права
        Editor, // Може да редактира само неща като покупки, продажби и т.н.
        Viewer  // Само чете данните
    }

    public enum MemberStatus
    {
        PendingInvite, // Just invited, not accepted yet
        In             // Accepted and part of the team
    }
}
