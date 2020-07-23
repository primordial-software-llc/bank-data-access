using System;
using System.Collections.Generic;
using System.Linq;
using Accounting;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using PropertyRentalManagement.QuickBooksOnline.Models.Tax;

namespace PropertyRentalManagement.QuickBooksOnline
{
    public class AccountingQuickBooksOnlineClient : IAccountingClient
    {
        private QuickBooksOnlineClient QuickBooksClient { get; set; }

        public AccountingQuickBooksOnlineClient(QuickBooksOnlineClient quickBooksClient)
        {
            QuickBooksClient = quickBooksClient;
        }

        public int? RecordExpense(IJournalEntry journalEntry)
        {
            var expense = new Purchase
            {
                TxnDate = DateTime.Parse(journalEntry.Date).ToString("yyyy-MM-dd"),
                AccountRef = new Reference { Value = 66.ToString() },
                PaymentType = "Cash",
                Line = new List<PurchaseLine>
                {
                    new PurchaseLine
                    {
                        DetailType = "AccountBasedExpenseLineDetail",
                        Amount = journalEntry.Amount,
                        AccountBasedExpenseLineDetail = new AccountBasedExpenseLineDetail
                        {
                            TaxCodeRef = new Reference { Value = "NON" },
                            AccountRef = new Reference { Value = journalEntry.Account.ToString() },
                            BillableStatus = "NotBillable"
                        }
                    }
                },
                PrivateNote = journalEntry.Memo
            };
            return QuickBooksClient.Create(expense).Id;
        }

        public int? RecordIncome(IJournalEntry journalEntry)
        {
            var taxCode = QuickBooksClient.Query<TaxCode>($"select * from TaxCode where Id = '{journalEntry.TaxCode}'").Single();
            var taxRateId = taxCode.SalesTaxRateList.TaxRateDetail.Single().TaxRateRef.Value;
            var taxRate = QuickBooksClient.Query<TaxRate>($"select * from TaxRate where Id = '{taxRateId}'").Single();
            var taxableAmount = journalEntry.Amount / (1 + (taxRate.RateValue / 100));
            var salesReceipt = new SalesReceipt
            {
                CustomerRef = new Reference { Value = journalEntry.Account.GetValueOrDefault().ToString() },
                TxnDate = DateTime.Parse(journalEntry.Date).ToString("yyyy-MM-dd"),
                TotalAmount = taxableAmount,
                PrivateNote = journalEntry.Memo,
                Line = new List<SalesLine>
                {
                    new SalesLine
                    {
                        Amount = taxableAmount,
                        DetailType = "SalesItemLineDetail",
                        SalesItemLineDetail = new SalesItemLineDetail
                        {
                            ItemRef = new Reference { Value = journalEntry.Product.ToString() },
                            Quantity = 1,
                            UnitPrice = taxableAmount,
                            TaxCodeRef = new Reference { Value = Constants.QUICKBOOKS_INVOICE_LINE_TAXABLE }
                        }
                    }
                },
                TxnTaxDetail = new TxnTaxDetail { TxnTaxCodeRef = new Reference { Value = journalEntry.TaxCode.ToString() }}
            };
            return QuickBooksClient.Create(salesReceipt).Id;
        }
    }
}
