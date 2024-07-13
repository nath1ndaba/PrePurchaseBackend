using BackendServices.Actions;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BackendServices
{
    public interface ICommon
    {
        Task<Company> ValidateCompany(string role, string companyId = null);
    }
}
