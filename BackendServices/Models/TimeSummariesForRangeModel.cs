using System;
using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models
{
    public record TimeSummariesForRangeModel(
        [Required] DateTime StartDate, 
        [Required] DateTime EndDate, 
        string CompanyId,
        int Page = 0, 
        int Limit = 100)
    {
        internal int Skip => Page * Limit;
    }
}
