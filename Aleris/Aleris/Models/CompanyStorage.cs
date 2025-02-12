using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aleris.Models
{
    public class CompanyStorage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public Company Company { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public decimal ProductPrice { get; set; }

        [Required]
        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            private set => _totalPrice = value;
        }

        public void UpdateStorageOnPurchase(decimal purchaseQuantity, decimal purchaseTotalPrice)
        {
            Quantity += purchaseQuantity;
            _totalPrice += purchaseTotalPrice;
            CalculateAveragePrice();
        }

        public void UpdateStorageOnSale(decimal saleQuantity)
        {
            if (saleQuantity > Quantity)
                throw new InvalidOperationException("Not enough stock available.");

            Quantity -= saleQuantity;
            CalculateTotalPrice();
        }

        public void CalculateTotalPrice()
        {
            _totalPrice = ProductPrice * Quantity;
        }

        public void CalculateAveragePrice()
        {
            ProductPrice = Quantity > 0 ? _totalPrice / Quantity : 0;
        }
    }
}
