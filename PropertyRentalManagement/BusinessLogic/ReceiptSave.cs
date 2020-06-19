using System;
using System.Collections.Generic;
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
                Receipt = receipt
            };
            ReceiptDbClient.Create(result);

            if (string.IsNullOrWhiteSpace(receipt.Customer.Id))
            {
                var customer = new Customer {DisplayName = receipt.Customer.Name};
                customer = QuickBooksOnlineClient.Create("customer", customer);
                receipt.Customer.Id = customer.Id;
            }

            if (receipt.RentalAmount > 0)
            {
                var invoice = CreateInvoice(receipt, receipt.RentalDate);
                result.Invoice = QuickBooksOnlineClient.Create("invoice", invoice);
            }

            if (receipt.ThisPayment > 0 && receipt.RentalAmount > 0)
            {
                decimal appliedPaymentAmount = receipt.ThisPayment.GetValueOrDefault() <= receipt.RentalAmount.GetValueOrDefault()
                    ? receipt.ThisPayment.GetValueOrDefault()
                    : receipt.RentalAmount.GetValueOrDefault();
                var payment = new Payment
                {
                    TxnDate = receipt.RentalDate,
                    CustomerRef = new QuickBooksOnline.Models.Reference { Value = receipt.Customer.Id },
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
                        CustomerRef = new QuickBooksOnline.Models.Reference {Value = receipt.Customer.Id},
                        TotalAmount = creditRemaining,
                        PrivateNote = receipt.Memo
                    };
                    result.UnappliedPayment = QuickBooksOnlineClient.Create("payment", payment);
                }
            }
            ReceiptDbClient.Create(result);
            return result;
        }

        private Invoice CreateInvoice(Receipt receipt, string transactionDate)
        {
            decimal quantity = 1;
            decimal taxableAmount = receipt.RentalAmount.GetValueOrDefault() / 1.068m;
            var invoice = new Invoice
            {
                TxnDate = transactionDate,
                CustomerRef = new QuickBooksOnline.Models.Reference { Value = receipt.Customer.Id },
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
                PrivateNote = receipt.Memo,
                SalesTermRef = new QuickBooksOnline.Models.Reference { Value = Constants.QUICKBOOKS_TERMS_DUE_NOW.ToString() }
            };
            return invoice;
        }
    }
}
