using System.Collections.Generic;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using PropertyRentalManagement.QuickBooksOnline.Models.Payments;

namespace PropertyRentalManagement.BusinessLogic
{
    public class SalesReport
    {
        public IList<SalesReceipt> SalesReceipts { get; set; }
        public IList<Invoice> Invoices { get; set; }
        public IList<Payment> Payments { get; set; }
    }
}
