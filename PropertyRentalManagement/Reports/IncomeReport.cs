using System;
using System.Collections.Generic;
using System.Linq;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Payments;

namespace PropertyRentalManagement.Reports
{
    public class IncomeReport
    {
        public static CashBasisIncomeReport RunReport(
            DateTimeOffset start,
            DateTimeOffset end,
            QuickBooksOnlineClient qboClient,
            List<int> ignoredCustomerIds)
        {
            var salesStart = start.AddDays(-365);
            var salesEnd = end.AddDays(365);
            string salesReceiptQuery = $"select * from SalesReceipt Where TxnDate >= '{salesStart:yyyy-MM-dd}' and TxnDate <= '{salesEnd:yyyy-MM-dd}'";
            var salesReceipts = qboClient.QueryAll<SalesReceipt>(salesReceiptQuery);

            var paymentQuery = $"select * from Payment Where TxnDate >= '{salesStart:yyyy-MM-dd}' and TxnDate <= '{salesEnd:yyyy-MM-dd}'";
            var payments = qboClient.QueryAll<Payment>(paymentQuery);

            payments = payments
                .Where(x =>
                    x.MetaData.CreateTime >= start && x.MetaData.CreateTime < end.AddDays(1) &&
                    !ignoredCustomerIds.Select(y => y.ToString()).Contains(x.CustomerRef.Value)
                )
                .ToList();
            salesReceipts = salesReceipts
                .Where(x =>
                    x.MetaData.CreateTime >= start && x.MetaData.CreateTime < end.AddDays(1) &&
                    !ignoredCustomerIds.Select(y => y.ToString()).Contains(x.CustomerRef.Value)
                )
                .ToList();

            return new CashBasisIncomeReport
            {
                Payments = payments,
                SalesReceipts = salesReceipts
            };
        }

    }
}
