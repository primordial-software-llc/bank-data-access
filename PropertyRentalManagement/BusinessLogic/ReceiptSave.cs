using System;
using System.Collections.Generic;
using FinanceApi.RequestModels;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using PropertyRentalManagement.QuickBooksOnline.Models.Payments;

namespace PropertyRentalManagement.BusinessLogic
{
    public class ReceiptSave
    {
        private DatabaseClient<ReceiptSaveResult> ReceiptDbClient { get; }
        private QuickBooksOnlineClient QuickBooksClient { get; }
        private decimal TaxRate { get; set; }

        public ReceiptSave(DatabaseClient<ReceiptSaveResult> receiptDbClient, QuickBooksOnlineClient quickBooksClient, decimal taxRate)
        {
            ReceiptDbClient = receiptDbClient;
            QuickBooksClient = quickBooksClient;
            TaxRate = taxRate;
        }

        public ReceiptSaveResult SaveReceipt(Receipt receipt, string firstName, string lastName, string email)
        {
            ReceiptSaveResult result = new ReceiptSaveResult
            {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow.ToString("O"),
                Receipt = JsonConvert.DeserializeObject<Receipt>(JsonConvert.SerializeObject(receipt)),
                CreatedBy = new ReceiptSaveResultUser
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email
                }
            };
            ReceiptDbClient.Create(result);

            string customerId = receipt.Customer.Id;
            if (string.IsNullOrWhiteSpace(customerId))
            {
                var customer = new Customer {DisplayName = receipt.Customer.Name};
                customer = QuickBooksClient.Create(customer);
                customerId = customer.Id.ToString();
            }

            if (receipt.RentalAmount > 0)
            {
                result.Invoice = QuickBooksClient.Create(CreateInvoice(customerId, receipt.RentalAmount, receipt.RentalDate, receipt.Memo));
            }

            result.Payments = new List<Payment>();
            if (receipt.ThisPayment > 0)
            {
                var unpaidInvoices = QuickBooksClient.QueryAll<Invoice>($"select * from Invoice where Balance != '0' and CustomerRef = '{customerId}' ORDERBY TxnDate");
                decimal payment = receipt.ThisPayment.GetValueOrDefault();
                var paymentApplicator = new PaymentApplicator(QuickBooksClient);
                foreach (var unpaidInvoice in unpaidInvoices)
                {
                    var paymentAppliedToInvoice = paymentApplicator.CreatePayment(
                        unpaidInvoice,
                        customerId,
                        payment,
                        receipt.TransactionDate,
                        receipt.Memo);
                    result.Payments.Add(paymentAppliedToInvoice);
                    payment -= paymentAppliedToInvoice.TotalAmount.GetValueOrDefault();
                    if (payment <= 0)
                    {
                        break;
                    }
                }
                if (payment > 0)
                {
                    var unappliedPayment = paymentApplicator.CreatePayment(
                        null,
                        customerId,
                        payment,
                        receipt.TransactionDate,
                        receipt.Memo
                    );
                    result.Payments.Add(unappliedPayment);
                }
            }

            ReceiptDbClient.Create(result);
            return result;
        }

        private Invoice CreateInvoice(string customerId, decimal? rentalAmount, string transactionDate, string memo)
        {
            decimal quantity = 1;
            decimal taxableAmount = rentalAmount.GetValueOrDefault() / (1 + TaxRate);
            var invoice = new Invoice
            {
                TxnDate = transactionDate,
                CustomerRef = new QuickBooksOnline.Models.Reference { Value = customerId },
                Line = new List<SalesLine>
                {
                    new SalesLine
                    {
                        DetailType = "SalesItemLineDetail",
                        SalesItemLineDetail = new SalesItemLineDetail
                        {
                            ItemRef = new QuickBooksOnline.Models.Reference { Value = Constants.QUICKBOOKS_PRODUCT_RENT.ToString() },
                            Quantity = quantity,
                            TaxCodeRef = new QuickBooksOnline.Models.Reference { Value = Constants.QUICKBOOKS_INVOICE_LINE_TAXABLE },
                            UnitPrice = taxableAmount
                        },
                        Amount = quantity * taxableAmount
                    }
                },
                TxnTaxDetail = new TxnTaxDetail
                {
                    TxnTaxCodeRef = new QuickBooksOnline.Models.Reference { Value = Constants.QUICKBOOKS_RENTAL_TAX_RATE.ToString() }
                },
                PrivateNote = memo,
                SalesTermRef = new QuickBooksOnline.Models.Reference { Value = Constants.QUICKBOOKS_TERMS_DUE_NOW.ToString() }
            };
            return invoice;
        }
    }
}
