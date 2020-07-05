using System;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;

namespace PropertyRentalManagement.BusinessLogic
{
    public class SaleReportService
    {
        public SaleReport GetSales(QuickBooksOnlineClient qboClient, string customerId, DateTime startDate, DateTime endDate)
        {
            var salesReceipts = qboClient.QueryAll<SalesReceipt>(
                $"select * from SalesReceipt Where TxnDate >= '{startDate:yyyy-MM-dd}' and TxnDate <= '{endDate:yyyy-MM-dd}' and CustomerRef = '{customerId}'");
            var invoices = qboClient.QueryAll<Invoice>(
                $"select * from Invoice Where TxnDate >= '{startDate:yyyy-MM-dd}' and TxnDate <= '{endDate:yyyy-MM-dd}' and CustomerRef = '{customerId}'");

            return new SaleReport
            {
                SalesReceipts = salesReceipts,
                Invoices = invoices
            };
        }
    }
}
