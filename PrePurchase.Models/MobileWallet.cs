
using System.Collections.Generic;

namespace PrePurchase.Models;

public class MobileWallet
{
    public IEnumerable<RateContent> RateContents { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal UifAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
}
public class RateContent
{
    public string RateType { get; set; }
    public string RateAmount { get; set; }

}
