using System.Collections.Generic;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Payments;

namespace PropertyRentalManagement.Reports
{
    public class CashBasisIncomeReport
    {
        public IList<SalesReceipt> SalesReceipts { get; set; }
        public IList<Payment> Payments { get; set; }
    }
}
