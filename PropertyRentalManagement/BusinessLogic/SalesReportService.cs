using System;
using System.Collections.Generic;
using System.Linq;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using PropertyRentalManagement.QuickBooksOnline.Models.Payments;

namespace PropertyRentalManagement.BusinessLogic
{
    public class SalesReportService
    {
        public SalesReport GetSales(QuickBooksOnlineClient client, DateTime start, DateTime end, int? customerId = null, List<int> ignoredCustomerIds = null)
        {
            string salesReceiptQuery = $"select * from SalesReceipt Where TxnDate >= '{start:yyyy-MM-dd}' and TxnDate <= '{end:yyyy-MM-dd}'";
            if (customerId.GetValueOrDefault() > 0)
            {
                salesReceiptQuery += $" and CustomerRef = '{customerId}'";
            }
            var salesReceipts = client.QueryAll<SalesReceipt>(salesReceiptQuery)
                .Where(x => ignoredCustomerIds == null || !ignoredCustomerIds.Contains(int.Parse(x.CustomerRef.Value)))
                .ToList();

            var paymentQuery = $"select * from Payment Where TxnDate >= '{start:yyyy-MM-dd}' and TxnDate <= '{end:yyyy-MM-dd}'";
            if (customerId.GetValueOrDefault() > 0)
            {
                paymentQuery += $" and CustomerRef = '{customerId}'";
            }
            var payments = client.QueryAll<Payment>(paymentQuery)
                .Where(x => ignoredCustomerIds == null || !ignoredCustomerIds.Contains(int.Parse(x.CustomerRef.Value)))
                .ToList();
            return new SalesReport
            {
                SalesReceipts = salesReceipts,
                Invoices = GetInvoices(client, start, end, customerId, ignoredCustomerIds),
                Payments = payments
            };
        }

        public List<Invoice> GetInvoices(QuickBooksOnlineClient client, DateTime start, DateTime end, int? customerId = null, List<int> ignoredCustomerIds = null)
        {
            var invoiceQuery = $"select * from Invoice Where TxnDate >= '{start:yyyy-MM-dd}' and TxnDate <= '{end:yyyy-MM-dd}'";
            if (customerId.GetValueOrDefault() > 0)
            {
                invoiceQuery += $" and CustomerRef = '{customerId}'";
            }
            return client.QueryAll<Invoice>(invoiceQuery)
                .Where(x => ignoredCustomerIds == null || !ignoredCustomerIds.Contains(int.Parse(x.CustomerRef.Value)))
                .ToList();
        }

    }
}
