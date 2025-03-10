﻿using System.ComponentModel.DataAnnotations;
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
        public int ProductId { get; set; }
        public CompanyStorage? Storage { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public decimal ProductPrice { get; set; }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            private set => _totalPrice = value;
        }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; private set; } = DateTime.Now;

        [Required]
        public string UnitType { get; set; }

        public void CalculateTotalPrice()
        {
            _totalPrice = ProductPrice * Quantity;
        }
    }
}
