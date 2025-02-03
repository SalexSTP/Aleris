using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aleris.Models
{
    public class CompanyMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int CompanyId { get; set; }
        public Company Company { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Viewer;
    }

    public enum UserRole
    {
        Admin,  // Създал фирмата или има пълни права
        Editor, // Може да редактира само неща като покупки, продажби и т.н.
        Viewer  // Само чете данните
    }
}
