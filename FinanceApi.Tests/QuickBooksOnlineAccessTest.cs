using System;
using System.Collections.Generic;
using Amazon.CloudWatchEvents.Model.Internal.MarshallTransformations;
using FinanceApi.Tests.InfrastructureAsCode;
using Newtonsoft.Json;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests
{
    public class QuickBooksOnlineAccessTest
    {
        private ITestOutputHelper Output { get; }

        public QuickBooksOnlineAccessTest(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void GetCustomers()
        {
            var qboClient = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));
            var customers = qboClient.QueryAll<Customer>("select * from Customer");
            Output.WriteLine(customers.Count.ToString());
            Output.WriteLine(JsonConvert.SerializeObject(customers));
        }

        private void CreateExpense(QuickBooksOnlineClient client, string txnDate, decimal amount, string account, string privateNote)
        {
            const int paymentAccountUndepositedFunds = 24;
            var expense = new Purchase
            {
                TxnDate = txnDate,
                PaymentType = "Cash",
                AccountRef = new Reference { Value = paymentAccountUndepositedFunds.ToString() },
                Line = new List<PurchaseLine>
                {
                    new PurchaseLine
                    {
                        DetailType = "AccountBasedExpenseLineDetail",
                        Amount = amount,
                        AccountBasedExpenseLineDetail = new AccountBasedExpenseLineDetail
                        {
                            TaxCodeRef = new Reference {Value = "NON"},
                            AccountRef = new Reference {Value = account},
                            BillableStatus = "NotBillable"
                        }
                    }
                },
                PrivateNote = privateNote
            };
            client.Create(expense);
        }

    }
}
