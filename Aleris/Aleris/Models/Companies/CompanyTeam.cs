using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aleris.Models.Company
{
    public class CompanyTeam
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Company")]
        [MaxLength(100)]
        public string TeamName { get; set; } = string.Empty;
        public Company Company { get; set; } = null!;

        public ICollection<CompanyMember> CompanyMembers { get; set; } = new List<CompanyMember>();
    }
}   
