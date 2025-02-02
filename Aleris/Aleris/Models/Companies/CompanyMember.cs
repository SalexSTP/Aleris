using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Aleris.Models.Company
{
    public class CompanyMember : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Roles Role { get; set; } = Roles.Viewer;

        public enum Roles
        {
            [Display(Name = "Админ")]
            Admin,
            [Display(Name = "Редактор")]
            Editor,
            [Display(Name = "Зрител")]
            Viewer
        }

        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;
    }
}


