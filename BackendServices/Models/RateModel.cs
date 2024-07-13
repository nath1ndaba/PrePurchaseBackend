using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models
{
    public record RateModel(
        [Required] string NameOfPosition,
        [Required] decimal StandardDaysRate,
        [Required] decimal SaturdaysRate,
        [Required] decimal SundaysRate,
        [Required] decimal PublicHolidaysRate,
        [Required] decimal DailyBonus,
        [Required] decimal OverTimeRate)
    {
        public RateModel Sanitize()
         => this with { NameOfPosition = NameOfPosition.Trim() };
    }
}
