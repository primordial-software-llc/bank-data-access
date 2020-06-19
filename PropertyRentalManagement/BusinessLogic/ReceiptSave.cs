using System;
using System.Collections.Generic;
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
        private QuickBooksOnlineClient QuickBooksOnlineClient { get; }

        public ReceiptSave(DatabaseClient<ReceiptSaveResult> receiptDbClient, QuickBooksOnlineClient quickBooksOnlineClient)
        {
            ReceiptDbClient = receiptDbClient;
            QuickBooksOnlineClient = quickBooksOnlineClient;
        }

        public ReceiptSaveResult SaveReceipt(Receipt receipt)
        {
            ReceiptSaveResult result = new ReceiptSaveResult
            {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow.ToString("O"),
                Receipt = JsonConvert.DeserializeObject<Receipt>(JsonConvert.SerializeObject(receipt))
            };
            ReceiptDbClient.Create(result);

            string customerId = receipt.Customer.Id;
            if (string.IsNullOrWhiteSpace(customerId))
            {
                var customer = new Customer {DisplayName = receipt.Customer.Name};
                customer = QuickBooksOnlineClient.Create("customer", customer);
                customerId = customer.Id;
            }

            if (receipt.RentalAmount > 0)
            {
                result.Invoice = QuickBooksOnlineClient.Create("invoice", CreateInvoice(customerId, receipt.RentalAmount, receipt.RentalDate, receipt.Memo));
            }

            if (receipt.ThisPayment > 0 && receipt.RentalAmount > 0)
            {
                decimal appliedPaymentAmount = receipt.ThisPayment.GetValueOrDefault() <= receipt.RentalAmount.GetValueOrDefault()
                    ? receipt.ThisPayment.GetValueOrDefault()
                    : receipt.RentalAmount.GetValueOrDefault();
                var payment = new Payment
                {
                    TxnDate = receipt.RentalDate,
                    CustomerRef = new QuickBooksOnline.Models.Reference { Value = customerId },
                    TotalAmount = appliedPaymentAmount,
                    PrivateNote = receipt.Memo,
                    Line = new List<PaymentLine>
                    {
                        new PaymentLine
                        {
                            Amount = appliedPaymentAmount,
                            LinkedTxn = new List<LinkedTransaction>
                            {
                                new LinkedTransaction
                                {
                                    TxnId = result.Invoice.Id,
                                    TxnType = "Invoice"
                                }
                            }
                        }
                    }
                };
                result.PaymentAppliedToInvoice = QuickBooksOnlineClient.Create("payment", payment);
            }

            if (receipt.ThisPayment > 0)
            {
                decimal creditRemaining = receipt.ThisPayment.GetValueOrDefault() - receipt.RentalAmount.GetValueOrDefault();
                if (creditRemaining > 0)
                {
                    var payment = new Payment
                    {
                        TxnDate = receipt.RentalDate,
                        CustomerRef = new QuickBooksOnline.Models.Reference {Value = customerId},
                        TotalAmount = creditRemaining,
                        PrivateNote = receipt.Memo
                    };
                    result.UnappliedPayment = QuickBooksOnlineClient.Create("payment", payment);
                }
            }
            ReceiptDbClient.Create(result);
            return result;
        }

        private Invoice CreateInvoice(string customerId, decimal? rentalAmount, string transactionDate, string memo)
        {
            decimal quantity = 1;
            decimal taxableAmount = rentalAmount.GetValueOrDefault() / 1.068m;
            var invoice = new Invoice
            {
                TxnDate = transactionDate,
                CustomerRef = new QuickBooksOnline.Models.Reference { Value = customerId },
                Line = new List<InvoiceLine>
                {
                    new InvoiceLine
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
