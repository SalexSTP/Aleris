using System.ComponentModel.DataAnnotations;

namespace Aleris.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Length(9, 9)]
        public string Bulstat { get; set; }

        [MaxLength(12)]
        public string? VatNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string City { get; set; }

        [Required]
        [MaxLength(200)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string Manager { get; set; }

        [Required]
        [Length(9, 9)]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(50)]
        [EmailAddress]
        public string Email { get; set; }

        public CompanySettings CompanySettings { get; set; }

        public ICollection<CompanyMember> TeamMembers { get; set; } = new List<CompanyMember>();

        public Company()
        {
            CompanySettings = new CompanySettings();
        }
    }
}
