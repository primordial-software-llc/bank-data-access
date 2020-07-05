using System.Collections.Generic;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;

namespace PropertyRentalManagement.BusinessLogic
{
    public class SaleReport
    {
        public IList<SalesReceipt> SalesReceipts { get; set; }
        public IList<Invoice> Invoices { get; set; }
    }
}
