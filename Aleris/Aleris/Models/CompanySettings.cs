using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aleris.Models
{
    public class CompanySettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [InverseProperty("CompanySettings")]
        public Company? Company { get; set; }

        public VatRegistered IsVatRegistered { get; set; } = VatRegistered.Yes; // Фирмата регистрирана ли е по ЗДДС?
        public IsVatIncluded IsVatIncludedInPrices { get; set; } = IsVatIncluded.Yes; // Продавачните цени са с включен ДДС
        public PrecisionOfPrice PricePrecision { get; set; } = PrecisionOfPrice.TwoDecimals; // Точност на цените    
        public PrecisionOfQuantity QuantityPrecision { get; set; } = PrecisionOfQuantity.OneDecimal; // Точност на количествата
        public NegativeQuantities AllowNegativeQuantities { get; set; } = NegativeQuantities.Yes; // Разрешаване на отрицателни количества
        public RevisionMethod MethodOfRevision { get; set; } = RevisionMethod.Delivery; // Метод на ревизиране
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
            Delivery,
            [Display(Name = "Продажна цена")]
            Selling
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

        public CompanySettings()
        {
            IsVatRegistered = VatRegistered.Yes;
            IsVatIncludedInPrices = IsVatIncluded.Yes;
            PricePrecision = PrecisionOfPrice.TwoDecimals;
            QuantityPrecision = PrecisionOfQuantity.OneDecimal;
            AllowNegativeQuantities = NegativeQuantities.Yes;
            MethodOfRevision = RevisionMethod.Delivery;
            AutoProduction = IsAutoProduction.No;
            WorkWithTraders = Traders.No;
        }
    }
}
