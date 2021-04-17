using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using AwsDataAccess;
using AwsTools;
using PropertyRentalManagement;
using PropertyRentalManagement.Clover;
using PropertyRentalManagement.Clover.Models;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using Constants = Accounting.Constants;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SendRestaurantSalesToAccounting
{
    /// <summary>
    /// I am responsible for keeping quickbooks up to date with clover.
    /// </summary>
    public class Function
    {
        public string FunctionHandler(ILambdaContext context)
        {
            var logger = new ConsoleLogger();
            var date = new DateTime(DateTime.UtcNow.Date.Year, DateTime.UtcNow.Date.Month, 1).AddMonths(-1);
            logger.Log($"Scheduled function PropertyRentalManagement.SendRestaurantSalesToAccounting started for {date:yyyy-MM-dd}");
            SendToAccounting(date, logger);
            Console.WriteLine("Scheduled function PropertyRentalManagement.SendRestaurantSalesToAccounting completed");
            return "Scheduled function PropertyRentalManagement.SendRestaurantSalesToAccounting completed";
        }
        
        public void SendToAccounting(DateTime date, ILogging logger)
        {
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient(), logger);
            var qboClient = new QuickBooksOnlineClient(PrivateAccounting.Constants.LakelandMiPuebloRealmId, databaseClient, logger);

            var originalMonth = date.Month;

            var client = new CloverClient(
                Environment.GetEnvironmentVariable("CLOVER_MI_PUEBLO_CHICKEN_ACCESS_TOKEN"),
                Environment.GetEnvironmentVariable("CLOVER_MI_PUEBLO_CHICKEN_MERCHANT_ID"),
                logger
            );

            var restaurantCustomer = qboClient
                .QueryAll<Customer>($"select * from Customer where Id = '{PrivateAccounting.Constants.LakelandMiPuebloAccountRestaurant}'")
                .First();

            var cashTenderType = client.QueryAll<Tender>($"tenders")
                .Single(x => string.Equals(x.LabelKey, "com.clover.tender.cash", StringComparison.OrdinalIgnoreCase));

            do
            {
                var salesReceipts = qboClient.QueryAll<SalesReceipt>(
                    $"select * from SalesReceipt where CustomerRef = '{PrivateAccounting.Constants.LakelandMiPuebloAccountRestaurant}' and TxnDate = '{date:yyyy-MM-dd}'");

                if (salesReceipts.Any())
                {
                    logger.Log($"Skipping {date:yyyy-MM-dd}, because a sales receipt already exists.");
                }
                else
                {
                    var today = new DateTimeOffset(date).ToUnixTimeMilliseconds();
                    var tomorrow = new DateTimeOffset(date.AddDays(1)).ToUnixTimeMilliseconds();
                    var result =
                        client.QueryAll<Payment>($"payments?filter=createdTime>={today}&filter=createdTime<{tomorrow}");

                    var cashPayments = result.Where(x => x.Tender.Id == cashTenderType.Id).ToList();
                    var cardPayments = result.Where(x => x.Tender.Id != cashTenderType.Id).ToList();
                    var cashTotal = GetTotal(cashPayments);
                    var cardTotal = GetTotal(cardPayments);
                    if (cashTotal > 0)
                    {
                        CreateSalesReceipt(
                            qboClient,
                            restaurantCustomer.Id,
                            restaurantCustomer.DefaultTaxCodeRef.Value,
                            PrivateAccounting.Constants.LakelandMiPuebloProductRestaurant,
                            date,
                            GetTotal(cashPayments),
                            "Restaurant sales using cash in Clover Register");
                    }

                    if (cardTotal > 0)
                    {
                        CreateSalesReceipt(
                            qboClient,
                            restaurantCustomer.Id,
                            restaurantCustomer.DefaultTaxCodeRef.Value,
                            PrivateAccounting.Constants.LakelandMiPuebloProductRestaurant,
                            date,
                            cardTotal,
                            "Restaurant sales using credit card in Clover Register");
                    }
                }
                date = date.AddDays(1);
            } while (date.Month == originalMonth);
        }

        private void CreateSalesReceipt(
            QuickBooksOnlineClient client,
            int? customerId,
            string taxCode,
            int product,
            DateTime date,
            decimal amount,
            string memo)
        {
            var salesReceipt = new SalesReceipt
            {
                CustomerRef = new PropertyRentalManagement.QuickBooksOnline.Models.Reference { Value = customerId.ToString() },
                TxnDate = date.ToString("yyyy-MM-dd"),
                TotalAmount = amount,
                PrivateNote = memo,
                Line = new List<SalesLine>
                {
                    new SalesLine
                    {
                        Amount = amount,
                        DetailType = "SalesItemLineDetail",
                        SalesItemLineDetail = new SalesItemLineDetail
                        {
                            ItemRef = new PropertyRentalManagement.QuickBooksOnline.Models.Reference { Value = product.ToString() },
                            Quantity = 1,
                            UnitPrice = amount,
                            TaxCodeRef = new PropertyRentalManagement.QuickBooksOnline.Models.Reference { Value = Constants.QUICKBOOKS_INVOICE_LINE_TAXABLE }
                        }
                    }
                },
                TxnTaxDetail = new TxnTaxDetail
                {
                    TxnTaxCodeRef = new PropertyRentalManagement.QuickBooksOnline.Models.Reference { Value = taxCode }
                }
            };
            client.Create(salesReceipt);
        }

        private decimal GetTotal(List<Payment> payments)
        {
            return payments.Sum(x => x.Amount - x.TaxAmount - x.TipAmount - x.CashbackAmount) / 100;
        }

    }
}
