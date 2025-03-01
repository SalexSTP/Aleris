using System.ComponentModel.DataAnnotations;

namespace Aleris.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 9)]
        [Phone]
        public string PhoneNumber { get; set; }

        public ICollection<CompanyMember> CompanyMemberships { get; set; } = new List<CompanyMember>();

        public ICollection<Invite> Invites { get; set; } = new List<Invite>();
    }
}
