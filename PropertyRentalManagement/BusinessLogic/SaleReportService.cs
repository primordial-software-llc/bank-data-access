using System;
using System.Collections.Generic;
using System.Linq;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using PropertyRentalManagement.QuickBooksOnline.Models.Payments;

namespace PropertyRentalManagement.BusinessLogic
{
    public class SaleReportService
    {
        public SaleReport GetSales(QuickBooksOnlineClient client, DateTime start, DateTime end, int? customerId, List<int> ignoredCustomerIds)
        {
            string salesReceiptQuery = $"select * from SalesReceipt Where TxnDate >= '{start:yyyy-MM-dd}' and TxnDate <= '{end:yyyy-MM-dd}'";
            if (customerId.GetValueOrDefault() > 0)
            {
                salesReceiptQuery += $" and CustomerRef = '{customerId}'";
            }
            var salesReceipts = client.QueryAll<SalesReceipt>(salesReceiptQuery)
                .Where(x => !ignoredCustomerIds.Contains(int.Parse(x.CustomerRef.Value)))
                .ToList();

            var paymentQuery = $"select * from Payment Where TxnDate >= '{start:yyyy-MM-dd}' and TxnDate <= '{end:yyyy-MM-dd}'";
            if (customerId.GetValueOrDefault() > 0)
            {
                paymentQuery += $" and CustomerRef = '{customerId}'";
            }
            var payments = client.QueryAll<Payment>(paymentQuery)
                .Where(x => !ignoredCustomerIds.Contains(int.Parse(x.CustomerRef.Value)))
                .ToList();

            var invoiceQuery = $"select * from Invoice Where TxnDate >= '{start:yyyy-MM-dd}' and TxnDate <= '{end:yyyy-MM-dd}'";
            if (customerId.GetValueOrDefault() > 0)
            {
                invoiceQuery += $" and CustomerRef = '{customerId}'";
            }
            var invoices = client.QueryAll<Invoice>(invoiceQuery)
                .Where(x => !ignoredCustomerIds.Contains(int.Parse(x.CustomerRef.Value)))
                .ToList();

            return new SaleReport
            {
                SalesReceipts = salesReceipts,
                Invoices = invoices,
                Payments = payments
            };
        }

    }
}
