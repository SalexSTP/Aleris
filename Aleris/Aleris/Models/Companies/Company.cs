using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Aleris.Models.Company
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(9)]
        public string Bulstat { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? VatNumber { get; set; } // Номер по ДДС (не е задължителен)

        [Required]
        [MaxLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Manager { get; set; } // М.О.Л (препоръчителен)

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public ICollection<CompanyMember> CompanyMembers { get; set; } = new List<CompanyMember>();
        public CompanySettings CompanySettings { get; set; }
    }
}
