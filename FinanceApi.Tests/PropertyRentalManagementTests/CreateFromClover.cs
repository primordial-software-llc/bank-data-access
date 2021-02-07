using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using FinanceApi.Tests.InfrastructureAsCode;
using Newtonsoft.Json;
using PrivateAccounting;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.Clover;
using PropertyRentalManagement.Clover.Models;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using Xunit;
using Xunit.Abstractions;
using Reference = PropertyRentalManagement.QuickBooksOnline.Models.Reference;

namespace FinanceApi.Tests.PropertyRentalManagementTests
{
    public class CreateFromClover
    {
        private ITestOutputHelper Output { get; }

        const int ACCOUNT_RESTAURANT = 1864;
        const int PRODUCT_RESTAURANT = 238;


        public CreateFromClover(ITestOutputHelper output)
        {
            Output = output;
        }


        //[Fact]
        public void RollBackBatch()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            client.ScanAsync(new ScanRequest(Constants.PrivateDatabase));

            var batch1018 = GetFromBatch("DONTFIND0e85763b-d2e7-498c-9027-b4e785746ea7");
            Output.WriteLine(batch1018.Count.ToString());
            foreach (var record in batch1018)
            {
                DeleteInAccounting(record, "2020-10-18");
            }

            var batch1011 = GetFromBatch("DONTFINDb4d3aaa9-28a1-49b4-92ff-7d0edd440da7");
            Output.WriteLine(batch1011.Count.ToString());
            foreach (var record in batch1011)
            {
                DeleteInAccounting(record, "2020-10-11");
            }
        }

        private void DeleteInAccounting(JournalEntry privateJournalEntry, string date)
        {
            var qboClient = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));

            if (privateJournalEntry.AccountingId > 0)
            {
                if (string.Equals(privateJournalEntry.Type, "income"))
                {
                    var salesReceipt = qboClient.Query<SalesReceipt>($"select * from SalesReceipt where id = '{privateJournalEntry.AccountingId}'")
                        .FirstOrDefault();
                    if (salesReceipt != null)
                    {
                        qboClient.Delete(salesReceipt);
                    }
                }
                else
                {
                    var purchase = qboClient.Query<Purchase>($"select * from Purchase where id = '{privateJournalEntry.AccountingId}'")
                        .FirstOrDefault();
                    if (purchase != null)
                    {
                        qboClient.Delete(purchase);
                    }
                }
            }

            var client = Factory.CreateAmazonDynamoDbClient();

            var key = new Dictionary<string, AttributeValue> { { "id", new AttributeValue { S = privateJournalEntry.Id } } };

            var updates = new Dictionary<string, AttributeValueUpdate>
            {
                {"batch", new AttributeValueUpdate(null, AttributeAction.DELETE)},
                {"accountingId", new AttributeValueUpdate(null, AttributeAction.DELETE)},
                {"amount", new AttributeValueUpdate(new AttributeValue { N = privateJournalEntry.Amount.ToString() }, AttributeAction.PUT)},
                {"date", new AttributeValueUpdate(new AttributeValue { S = date }, AttributeAction.PUT )}
            };

            var privateUpdates = client.UpdateItemAsync(Constants.PrivateDatabase, key, updates).Result;
            var auditUpdates = client.UpdateItemAsync(Constants.AuditDatabase, key, updates).Result;

        }

        public List<JournalEntry> GetFromBatch(string batch)
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var scanRequest = new ScanRequest(PrivateAccounting.Constants.PrivateDatabase)
            {
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":batch", new AttributeValue { S = batch }}
                },
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#batch", "batch" }
                },
                FilterExpression = "#batch = :batch"
            };
            ScanResponse scanResponse = null;
            var items = new List<JournalEntry>();
            do
            {
                if (scanResponse != null)
                {
                    scanRequest.ExclusiveStartKey = scanResponse.LastEvaluatedKey;
                }
                scanResponse = client.ScanAsync(scanRequest).Result;
                if (scanResponse.Items.Any())
                {
                    foreach (var item in scanResponse.Items)
                    {
                        items.Add(JsonConvert.DeserializeObject<JournalEntry>(Document.FromAttributeMap(item).ToJson()));
                    }
                }
            } while (scanResponse.LastEvaluatedKey.Any());
            return items;
        }

        [Fact]
        public void SendCloverIncomeToQuickBooksOnlineForMonth()
        {
            var date = new DateTime(2020, 12, 1);
            var originalMonth = date.Month;

            var qboClient = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));
            var client = new CloverClient(
                Environment.GetEnvironmentVariable("CLOVER_MI_PUEBLO_CHICKEN_ACCESS_TOKEN"),
                Environment.GetEnvironmentVariable("CLOVER_MI_PUEBLO_CHICKEN_MERCHANT_ID"),
                new XUnitLogger(Output)
            );

            var restaurantCustomer = qboClient
                .QueryAll<Customer>($"select * from Customer where Id = '{ACCOUNT_RESTAURANT}'")
                .First();

            var cashTenderType = client.QueryAll<Tender>($"tenders")
                .Single(x => string.Equals(x.LabelKey, "com.clover.tender.cash", StringComparison.OrdinalIgnoreCase));

            do
            {
                var salesReceipts = qboClient.QueryAll<SalesReceipt>(
                    $"select * from SalesReceipt where CustomerRef = '{ACCOUNT_RESTAURANT}' and TxnDate = '{date:yyyy-MM-dd}'");

                if (salesReceipts.Any())
                {
                    Output.WriteLine($"Skipping {date:yyyy-MM-dd}, because a sales receipt already exists.");
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
                            PRODUCT_RESTAURANT,
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
                            PRODUCT_RESTAURANT,
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
                CustomerRef = new Reference { Value = customerId.ToString() },
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
                            ItemRef = new Reference { Value = product.ToString() },
                            Quantity = 1,
                            UnitPrice = amount,
                            TaxCodeRef = new Reference { Value = PropertyRentalManagement.Constants.QUICKBOOKS_INVOICE_LINE_TAXABLE }
                        }
                    }
                },
                TxnTaxDetail = new TxnTaxDetail { TxnTaxCodeRef = new Reference { Value = taxCode } }
            };
            client.Create(salesReceipt);
        }

        private decimal GetTotal(List<Payment> payments)
        {
            return payments.Sum(x => x.Amount - x.TaxAmount - x.TipAmount - x.CashbackAmount) / 100;
        }

    }
}
