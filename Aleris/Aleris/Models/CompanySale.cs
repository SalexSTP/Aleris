using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aleris.Models
{
    public class CompanySale
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public Company? Company { get; set; }

        [Required]
        [ForeignKey("CompanyStorage")]
        public int ProductId { get; set; }
        public CompanyStorage? Storage { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public decimal ProductPrice { get; set; }

        public decimal TotalPrice => ProductPrice * Quantity; // Computed dynamically

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; private set; } = DateTime.Now;
    }
}
