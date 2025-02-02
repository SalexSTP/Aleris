using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Aleris.Models.Company
{
    public class CompanySettings
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        public VatRegistered IsVatRegistered { get; set; } = VatRegistered.Yes; // Фирмата регистрирана ли е по ЗДДС?
        public IsVatIncluded IsVatIncludedInPrices { get; set; } = IsVatIncluded.Yes; // Продавачните цени са с включен ДДС
        public PrecisionOfPrice PricePrecision { get; set; } = PrecisionOfPrice.TwoDecimals; // Точност на цените    
        public PrecisionOfQuantity QuantityPrecision { get; set; } = PrecisionOfQuantity.OneDecimal; // Точност на количествата
        public NegativeQuantities AllowNegativeQuantities { get; set; } = NegativeQuantities.Yes; // Разрешаване на отрицателни количества
        public RevisionMethod MethodOfRevision { get; set; } = RevisionMethod.DeliveryPrice; // Метод на ревизиране
        public IsAutoProduction AutoProduction { get; set; } = IsAutoProduction.No; // Автоматично производство
        public Traders WorkWithTraders { get; set; } = Traders.No; // Работа с търговци

        public enum VatRegistered
        {
            [Display(Name = "Да")]
            Yes = 1,
            [Display(Name = "Не")]
            No = 0
        }

        public enum IsVatIncluded
        {
            [Display(Name = "Да")]
            Yes = 1,
            [Display(Name = "Не")]
            No = 0
        }

        public enum PrecisionOfPrice
        {
            [Display(Name = "0.")]
            ZeroDecimal = 0,

            [Display(Name = "0.0")]
            OneDecimal = 1,

            [Display(Name = "0.00")]
            TwoDecimals = 2,

            [Display(Name = "0.000")]
            ThreeDecimals = 3,

            [Display(Name = "0.0000")]
            FourDecimals = 4,

            [Display(Name = "0.00000")]
            FiveDecimals = 5
        }

        public enum PrecisionOfQuantity
        {
            [Display(Name = "0.")]
            ZeroDecimal = 0,

            [Display(Name = "0.0")]
            OneDecimal = 1,

            [Display(Name = "0.00")]
            TwoDecimals = 2,

            [Display(Name = "0.000")]
            ThreeDecimals = 3,

            [Display(Name = "0.0000")]
            FourDecimals = 4,

            [Display(Name = "0.00000")]
            FiveDecimals = 5
        }

        public enum NegativeQuantities
        {
            [Display(Name = "Да")]
            Yes = 1,
            [Display(Name = "Не")]
            No = 0
        }

        public enum RevisionMethod
        {
            [Display(Name = "Доставна цена")]
            DeliveryPrice,
            [Display(Name = "Продажна цена")]
            SellingPrice
        }

        public enum IsAutoProduction
        {
            [Display(Name = "Да")]
            Yes = 1,
            [Display(Name = "Не")]
            No = 0
        }

        public enum Traders
        {
            [Display(Name = "Да")]
            Yes = 1,
            [Display(Name = "Не")]
            No = 0
        }
    }
}
