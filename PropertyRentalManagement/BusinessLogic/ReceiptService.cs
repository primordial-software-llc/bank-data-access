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
    public class ReceiptService
    {
        private DatabaseClient<Receipt> ReceiptDbClient { get; }
        private QuickBooksOnlineClient QuickBooksOnlineClient { get; }

        public ReceiptService(DatabaseClient<Receipt> receiptDbClient, QuickBooksOnlineClient quickBooksOnlineClient)
        {
            ReceiptDbClient = receiptDbClient;
            QuickBooksOnlineClient = quickBooksOnlineClient;
        }

        public ReceiptSaveResult SaveReceipt(Receipt receipt)
        {
            ValidateReceipt(receipt);
            ReceiptSaveResult result = new ReceiptSaveResult();
            receipt.Id = Guid.NewGuid().ToString();
            receipt.Timestamp = DateTime.UtcNow.ToString("O");
            ReceiptDbClient.Create(receipt);

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
                    CustomerRef = new Reference { Value = receipt.CustomerId.ToString() },
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
                        CustomerRef = new Reference {Value = receipt.CustomerId.ToString()},
                        TotalAmount = creditRemaining,
                        PrivateNote = receipt.Memo
                    };
                    result.UnappliedPayment = QuickBooksOnlineClient.Create("payment", payment);
                }
            }
            result.Receipt = receipt;
            return result;
        }

        private Invoice CreateInvoice(Receipt receipt, string transactionDate)
        {
            decimal quantity = 1;
            decimal taxableAmount = receipt.RentalAmount.GetValueOrDefault() / 1.068m;
            var invoice = new Invoice
            {
                TxnDate = transactionDate,
                CustomerRef = new Reference { Value = receipt.CustomerId.ToString() },
                Line = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        DetailType = "SalesItemLineDetail",
                        SalesItemLineDetail = new SalesItemLineDetail
                        {
                            ItemRef = new Reference { Value = Constants.QUICKBOOKS_PRODUCT_RENT.ToString() },
                            Quantity = quantity,
                            TaxCodeRef = new Reference { Value = Constants.QUICKBOOKS_INVOICE_LINE_TAXABLE },
                            UnitPrice = taxableAmount
                        },
                        Amount = quantity * taxableAmount
                    }
                },
                TxnTaxDetail = new TxnTaxDetail
                {
                    TxnTaxCodeRef = new Reference { Value = Constants.QUICKBOOKS_RENTAL_TAX_RATE.ToString() }
                },
                PrivateNote = receipt.Memo,
                SalesTermRef = new Reference { Value = Constants.QUICKBOOKS_TERMS_DUE_NOW.ToString() }
            };
            return invoice;
        }

        private void ValidateReceipt(Receipt receipt)
        {
            if (receipt.RentalAmount < 0)
            {
                throw new Exception("Rental amount must be greater or equal to zero.");
            }
            if (receipt.ThisPayment < 0)
            {
                throw new Exception("This payment must be greater or equal to zero.");
            }
            if ((receipt.Memo ?? string.Empty).Length > Constants.QUICKBOOKS_MEMO_MAX_LENGTH)
            {
                throw new Exception($"Memo must be less than or equal to {Constants.QUICKBOOKS_MEMO_MAX_LENGTH} characters.");
            }
        }
    }
}
