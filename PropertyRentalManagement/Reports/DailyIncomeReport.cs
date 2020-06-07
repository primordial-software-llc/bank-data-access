using System;
using System.Collections.Generic;
using System.Linq;
using AwsTools;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;

namespace PropertyRentalManagement.Reports
{
    public class DailyIncomeReport
    {
        public const int CUSTOMER_PARKING_A = 1859;
        public const int CUSTOMER_PARKING_B = 1861;
        public const int CUSTOMER_BAR_A = 1862;
        public const int CUSTOMER_BAR_B = 1863;
        public const int CUSTOMER_RESTAURANT = 1864;
        public const int CUSTOMER_VIRGIN_GUADALUPE = 1899;

        public static void PrintReport(string reportDate, ILogging logger, QuickBooksOnlineClient qboClient)
        {
            var rentalCustomerIds = new List<int> { CUSTOMER_PARKING_A, CUSTOMER_PARKING_B, CUSTOMER_BAR_A, CUSTOMER_BAR_B, CUSTOMER_RESTAURANT };
            var payrollVendors = qboClient.QueryAll<QuickBooksOnline.Models.Vendor>($"select * from Vendor where DisplayName LIKE 'Mi Pueblo%'");
            var expenses = qboClient.QueryAll<Purchase>($"select * from Purchase where TxnDate = '{reportDate}'"); // Entity ref isn't queryable, need to use a report to do this.
            List<Tuple<QuickBooksOnline.Models.Vendor, decimal?>> vendorTotals = new List<Tuple<QuickBooksOnline.Models.Vendor, decimal?>>();
            foreach (var vendor in payrollVendors)
            {
                var vendorExpenses = expenses
                    .Where(x => x.EntityRef != null &&
                                string.Equals(x.EntityRef.Type, "vendor", StringComparison.OrdinalIgnoreCase) &&
                                x.EntityRef.Value == vendor.Id)
                    .ToList();
                var vendorTotal = vendorExpenses.Sum(x => x.TotalAmount);
                vendorTotals.Add(new Tuple<QuickBooksOnline.Models.Vendor, decimal?>(vendor, vendorTotal));
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

            var rentalSalesReceipts = qboClient.QueryAll<SalesReceipt>($"select * from SalesReceipt Where TxnDate = '{reportDate}'")
                .Where(x => !nonRentalCustomers.Contains(int.Parse(x.CustomerRef.Value)));
            var rentalPayments = qboClient.QueryAll<Payment>($"select * from Payment Where TxnDate = '{reportDate}'")
                .Where(x => !nonRentalCustomers.Contains(int.Parse(x.CustomerRef.Value)));
            var rentalIncome = rentalSalesReceipts.Sum(x => x.TotalAmount) + rentalPayments.Sum(x => x.TotalAmount);
            
            List<Tuple<string, decimal?>> incomeTotals = new List<Tuple<string, decimal?>>();
            incomeTotals.Add(new Tuple<string, decimal?>("Rental Income", rentalIncome));
            foreach (var nonRentalCustomerId in nonRentalCustomers)
            {
                var customer = qboClient.Query<Customer>($"select * from Customer where Id = '{nonRentalCustomerId}'")
                    .FirstOrDefault();
                if (customer == null)
                {
                    throw new Exception("Customer not found: " + nonRentalCustomerId);
                }
                incomeTotals.Add(new Tuple<string, decimal?>(customer.DisplayName, GetTotalIncomeFromCustomer(qboClient, reportDate, nonRentalCustomerId, logger)));
            }

            logger.Log($"Income for {reportDate}");
            logger.Log($"Rent: {rentalIncome:C}");
            logger.Log($"Parking A: {GetTotalIncomeFromCustomer(qboClient, reportDate, CUSTOMER_PARKING_A, logger):C}");
            logger.Log($"Parking B: {GetTotalIncomeFromCustomer(qboClient, reportDate, CUSTOMER_PARKING_B, logger):C}");
            logger.Log($"Bar A: {GetTotalIncomeFromCustomer(qboClient, reportDate, CUSTOMER_BAR_A, logger):C}");
            logger.Log($"Bar B: {GetTotalIncomeFromCustomer(qboClient, reportDate, CUSTOMER_BAR_B, logger):C}");
            logger.Log($"Restaurant: {GetTotalIncomeFromCustomer(qboClient, reportDate, CUSTOMER_RESTAURANT, logger):C}");
            foreach (var incomeTotal in incomeTotals)
            {
                logger.Log($"{incomeTotal.Item1}: {incomeTotal.Item2:C}");
            }

            logger.Log($"\nPayments for {reportDate}");
            foreach (var vendorTotal in vendorTotals)
            {
                logger.Log($"{vendorTotal.Item1.DisplayName}: {vendorTotal.Item2:C}");
            }

            logger.Log($"\nTotal income for {reportDate} {incomeTotals.Sum(x => x.Item2):C}");
            logger.Log($"Total payroll for {reportDate} {vendorTotals.Sum(x => x.Item2):C}");
            var netIncome = incomeTotals.Sum(x => x.Item2) - vendorTotals.Sum(x => x.Item2);
            logger.Log($"Net income for {reportDate} {netIncome:C}");
            logger.Log("----------------------------------------------------");
        }

        public static decimal GetTotalIncomeFromCustomer(QuickBooksOnlineClient client, string date, int customerId, ILogging logger)
        {
            var salesReceipts = client.QueryAll<SalesReceipt>(
                $"select * from SalesReceipt Where TxnDate = '{date}' and CustomerRef = '{customerId}'");
            var payments = client.QueryAll<Payment>(
                $"select * from Payment Where TxnDate = '{date}' and CustomerRef = '{customerId}'");
            return salesReceipts.Sum(x => x.TotalAmount) + payments.Sum(x => x.TotalAmount);
        }
    }
}
