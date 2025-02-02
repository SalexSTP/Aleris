using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Aleris.Models.Company
{
    public class CompanyMember
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CompanyTeam")]
        public int CompanyTeamId { get; set; }
        public CompanyTeam CompanyTeam { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public Roles Role { get; set; } = Roles.Viewer; // Роля (Admin, Editor, Viewer)

        public enum Roles
        {
            [Display(Name = "Админ")]
            Admin,
            [Display(Name = "Редактор")]
            Editor,
            [Display(Name = "Зрител")]
            Viewer
        }
    }
}
