using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using BackendServices.Models.Payments;

namespace Infrastructure.Models
{
    public class PeachPayoutRequest
    {
        private readonly XDocument _root;
        public PeachPayoutRequest(PayOutModel recods, PeachHeaderModel header)
        {
            _root = new XDocument( 
                new XElement( "APIPaymentsRequest",
                    PayoutHeader(header, recods.Reference),
                    PayoutPaymentsContent(recods.Payments, recods.Reference),
                    PayoutTotals(recods.Records, recods.TotalAmount, 
                        recods.BranchHash, recods.AccountHash)
                ) 
            );

        }

        private XElement PayoutHeader(PeachHeaderModel header, string reference)
        {
            XElement element = new XElement( "Header", 
                new XElement( "PsVer", header.ApiVersion ), 
                new XElement( "Client", header.Client ),
                new XElement( "Service", header.Service ),
                new XElement( "ServiceType", header.ServiceType ),
                new XElement( "DueDate", header.DueDate ),
                new XElement( "Reference", reference ),
                new XElement( "CallBackUrl", header.CallBackUrl ),
                new XElement( "BankAccount", header.BankAccount )
            );

            return element;
        }

        private XElement PayoutPaymentsContent(IEnumerable<PayOutPaymentModel> payments, string reference)
        {
            XElement element = new XElement( "Payments");

            foreach (var item in payments)
            {
                var content = new XElement( "FileContents", 
                    new XElement( "Initials", item.Initials ), 
                    new XElement( "FirstNames", item.FirstNames ),
                    new XElement( "Surname", item.Surname ),
                    new XElement( "BranchCode", item.BranchCode ),
                    new XElement( "AccountNumber", item.AccountNumber ),
                    new XElement( "FileAmount", item.FileAmount ),
                    new XElement( "AccountType", item.AccountType),
                    new XElement( "CustomerCode", item.CustomerCode ),
                    new XElement( "AmountMultiplier", item.AmountMultiplier ),
                    new XElement( "Reference", reference )
                );

                element.Add(content);
            };

            return element;
        }

        private XElement PayoutTotals(int recods, decimal amount, long branchHash, long acoountHash)
        {
            XElement element = new XElement( "Totals", 
                new XElement( "Records", recods ), 
                new XElement( "Amount", amount ),
                new XElement( "BranchHash", branchHash ),
                new XElement( "AccountHash", acoountHash )
            );

            return element;
        }
    
        public override string ToString()
        {
            return _root.ToString();
        }

        public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            await _root.SaveAsync(stream, SaveOptions.None, cancellationToken).ConfigureAwait(false);
        }
    }
}