using System;
using System.Linq;
using AwsTools;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Payments;

namespace PropertyRentalManagement.Reports
{
    public class IncomeReport
    {
        public static void PrintReport(DateTime start, DateTime end, ILogging logger, QuickBooksOnlineClient qboClient)
        {
            var salesStart = start.AddDays(-365);
            var salesEnd = end.AddDays(365);
            string salesReceiptQuery = $"select * from SalesReceipt Where TxnDate >= '{salesStart:yyyy-MM-dd}' and TxnDate <= '{salesEnd:yyyy-MM-dd}'";
            var salesReceipts = qboClient.QueryAll<SalesReceipt>(salesReceiptQuery);
            var paymentQuery = $"select * from Payment Where TxnDate >= '{salesStart:yyyy-MM-dd}' and TxnDate <= '{salesEnd:yyyy-MM-dd}'";
            var payments = qboClient.QueryAll<Payment>(paymentQuery);

            payments = payments
                .Where(x => x.MetaData.CreateTime >= start && x.MetaData.CreateTime < end.AddDays(1))
                .ToList();
            salesReceipts = salesReceipts
                .Where(x => x.MetaData.CreateTime >= start && x.MetaData.CreateTime < end.AddDays(1))
                .ToList();

            var total = payments.Sum(x => x.TotalAmount.GetValueOrDefault()) +
                        salesReceipts.Sum(x => x.TotalAmount.GetValueOrDefault());
            logger.Log($"Rental Income {total:C}");
        }

    }
}
