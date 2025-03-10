﻿using System.ComponentModel.DataAnnotations;

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
        public string? Manager { get; set; }

        [Required]
        [StringLength(12, MinimumLength = 9)]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(50)]
        [EmailAddress]
        public string Email { get; set; }

        public CompanySettings CompanySettings { get; set; }

        public ICollection<CompanyMember> TeamMembers { get; set; } = new List<CompanyMember>();

        public ICollection<CompanyStorage> Storage { get; set; } = new List<CompanyStorage>();
        public ICollection<CompanyPurchase> Purchases { get; set; } = new List<CompanyPurchase>();
        public ICollection<CompanySale> Sales { get; set; } = new List<CompanySale>();

        public ICollection<Invite> Invites { get; set; } = new List<Invite>();

        public Company()
        {
            CompanySettings = new CompanySettings();
        }
    }
}
