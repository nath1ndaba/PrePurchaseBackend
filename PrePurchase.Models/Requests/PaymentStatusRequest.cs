using System;
using System.Collections.Generic;
using System.Text;
using PrePurchase.Models.Payments;

namespace PrePurchase.Models.Requests
{
    public record PaymentStatusRequest(
        string id, 
        string batchCode, 
        Service Service,
        ServiceType ServiceType
    );
}
