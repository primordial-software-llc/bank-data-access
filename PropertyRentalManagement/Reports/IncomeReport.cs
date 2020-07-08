using System;
using System.Collections.Generic;
using System.Linq;
using AwsTools;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;

namespace PropertyRentalManagement.Reports
{
    public class IncomeReport
    {
        public const int CUSTOMER_PARKING_A = 1859;
        public const int CUSTOMER_PARKING_B = 1861;
        public const int CUSTOMER_BAR_A = 1862;
        public const int CUSTOMER_BAR_B = 1863;
        public const int CUSTOMER_RESTAURANT = 1864;
        public const int CUSTOMER_VIRGIN_GUADALUPE = 1899;

        public static void PrintReport(DateTime start, DateTime end, ILogging logger, QuickBooksOnlineClient qboClient)
        {
            var payrollVendors = qboClient.QueryAll<Vendor>($"select * from Vendor where DisplayName LIKE 'Mi Pueblo%'");
            var expenses = qboClient.QueryAll<Purchase>($"select * from Purchase where TxnDate >= '{start:yyyy-MM-dd}' and TxnDate <= '{end:yyyy-MM-dd}'");
            List<Tuple<Vendor, decimal?>> vendorTotals = new List<Tuple<Vendor, decimal?>>();
            foreach (var vendor in payrollVendors)
            {
                var vendorExpenses = expenses
                    .Where(x => x.EntityRef != null &&
                                string.Equals(x.EntityRef.Type, "vendor", StringComparison.OrdinalIgnoreCase) && // Entity ref isn't queryable
                                x.EntityRef.Value == vendor.Id)
                    .ToList();
                var vendorTotal = vendorExpenses.Sum(x => x.TotalAmount);
                vendorTotals.Add(new Tuple<Vendor, decimal?>(vendor, vendorTotal));
            }

            var nonRentalCustomers = new List<int>
            {
                CUSTOMER_PARKING_A,
                CUSTOMER_PARKING_B,
                CUSTOMER_BAR_A,
                CUSTOMER_BAR_B,
                CUSTOMER_RESTAURANT,
                CUSTOMER_VIRGIN_GUADALUPE
            };

            var rentalSalesReport = new SaleReportService().GetSales(
                qboClient,
                start,
                end,
                null,
                nonRentalCustomers);
            rentalSalesReport.Payments = rentalSalesReport.Payments
                .Where(x => x.MetaData.CreateTime >= start && x.MetaData.CreateTime < end.AddDays(1))
                .ToList();

            List<Tuple<string, decimal?>> incomeTotals = new List<Tuple<string, decimal?>>();
            incomeTotals.Add(new Tuple<string, decimal?>("Rental Income", rentalSalesReport.Payments.Sum(x => x.TotalAmount)));
            foreach (var nonRentalCustomerId in nonRentalCustomers)
            {
                var customer = qboClient.Query<Customer>($"select * from Customer where Id = '{nonRentalCustomerId}'").First();
                var nonRentalSalesReport = new SaleReportService().GetSales(
                    qboClient,
                    start,
                    end,
                    nonRentalCustomerId.ToString(),
                    new List<int>());
                incomeTotals.Add(new Tuple<string, decimal?>(
                    customer.DisplayName,
                    nonRentalSalesReport.Payments.Sum(x => x.TotalAmount) + nonRentalSalesReport.SalesReceipts.Sum(x => x.TotalAmount)));
            }

            logger.Log($"Income and Cash Payroll for {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");
            logger.Log($"\nIncome");
            foreach (var incomeTotal in incomeTotals)
            {
                logger.Log($"{incomeTotal.Item1}: {incomeTotal.Item2:C}");
            }

            logger.Log($"\nCash Payroll");
            foreach (var vendorTotal in vendorTotals.Where(x => x.Item2 > 0))
            {
                logger.Log($"{vendorTotal.Item1.DisplayName}: {vendorTotal.Item2:C}");
            }

            logger.Log($"\nTotal income {incomeTotals.Sum(x => x.Item2):C}");
            logger.Log($"Total cash payroll {vendorTotals.Sum(x => x.Item2):C}");
            var netIncome = incomeTotals.Sum(x => x.Item2) - vendorTotals.Sum(x => x.Item2);
            logger.Log($"Net income {netIncome:C}");
            logger.Log("----------------------------------------------------");
        }

    }
}
