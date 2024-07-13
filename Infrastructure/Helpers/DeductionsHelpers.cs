using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public static class DeductionsHelpers
    {
        private const decimal taxBracket1 = 226_000m / 12;
        private const decimal taxBracket2 = 127_100m / 12;
        private const decimal taxBracket3 = 135_600m / 12;
        private const decimal taxBracket4 = 152_700m / 12;
        private const decimal taxBracket5 = 176_200m / 12;
        private const decimal taxBracket6 = 914_000m / 12;
        private const decimal taxBracket7 = decimal.MaxValue;
        private const decimal UIFPercentage = 0.01m;
        private const decimal MaxUIFPayable = 17_712m;

        public static decimal GetDeductionTaxnUIF(this decimal totalAmount)
        {
            var taxDeduction = totalAmount.GetTaxDeduction();
            var afterTax = totalAmount - taxDeduction;
            return taxDeduction + afterTax.GetUIFDeduction();
        }

        public static decimal GetTaxDeduction(this decimal amount)
        {
            //return TaxBracket(amount, taxBracket1, 0.18m)
            //        .Apply(TaxBracket, taxBracket2, 0.26m)
            //        .Apply(TaxBracket, taxBracket3, 0.31m)
            //        .Apply(TaxBracket, taxBracket4, 0.36m)
            //        .Apply(TaxBracket, taxBracket5, 0.39m)
            //        .Apply(TaxBracket, taxBracket6, 0.41m)
            //        .Apply(TaxBracket, taxBracket7, 0.45m)
            //        .Value;
            return TaxBracket(amount, taxBracket1, 0)
                   .Apply(TaxBracket, taxBracket2, 0)
                   .Apply(TaxBracket, taxBracket3, 0)
                   .Apply(TaxBracket, taxBracket4, 0)
                   .Apply(TaxBracket, taxBracket5, 0)
                   .Apply(TaxBracket, taxBracket6, 0)
                   .Apply(TaxBracket, taxBracket7, 0)
                   .Value;
        }

        public static decimal GetUIFDeduction(this decimal totalAmountAfterTax)
        {
            if(totalAmountAfterTax >= MaxUIFPayable)
                return MaxUIFPayable*UIFPercentage;

            return totalAmountAfterTax * UIFPercentage;
        }

        internal static DeductionLevel TaxBracket(DeductionLevel level, decimal maxTaxableAmount, decimal percentage)
        {
            if (level.Amount > maxTaxableAmount)
                return new() { Value = maxTaxableAmount * percentage + level.Value, Amount = level.Amount - maxTaxableAmount };

            return new() { Value = level.Amount * percentage + level.Value, Amount = 0m };
        }

        internal static DeductionLevel TaxBracket(decimal amount, decimal maxTaxableAmount, decimal percentage)
            => TaxBracket(new DeductionLevel() { Amount = amount }, maxTaxableAmount, percentage);
    }

    internal struct DeductionLevel
    {
        public decimal Value { get; init; }
        public decimal Amount { get; set; }

        public DeductionLevel Apply(Func<DeductionLevel, decimal, decimal, DeductionLevel> level, decimal maxTaxableAmount, decimal percentage)
        {
            if (Amount <= 0)
                return this;

            return level(this, maxTaxableAmount, percentage);
        }
    }
}
