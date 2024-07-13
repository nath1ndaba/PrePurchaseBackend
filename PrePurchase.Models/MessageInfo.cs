using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrePurchase.Models;

public class MessageInfo
{
    public string UserInput { get; set; } 
    public string Message { get; set; }
    public DateTime? TimeStamp { get; set; }
}
