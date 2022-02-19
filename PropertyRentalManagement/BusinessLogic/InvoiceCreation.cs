using System.Collections.Generic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;

namespace PropertyRentalManagement.BusinessLogic
{
    public class InvoiceCreation
    {
        public Invoice CreateInvoiceModel(
            QuickBooksOnlineClient quickBooksClient,
            string customerId,
            decimal? rentalAmount,
            string transactionDate,
            string memo,
            string invoiceItemId,
            string taxCodeId,
            bool taxable)
        {
            decimal quantity = 1;
            var taxRate = taxable ? new Tax().GetTaxRate(quickBooksClient, taxCodeId) : 0;
            decimal taxableAmount = rentalAmount.GetValueOrDefault() / (1 + taxRate);
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
                            ItemRef = new QuickBooksOnline.Models.Reference { Value = invoiceItemId },
                            Quantity = quantity,
                            TaxCodeRef = new QuickBooksOnline.Models.Reference
                            {
                                Value = taxable ? Accounting.Constants.QUICKBOOKS_INVOICE_LINE_TAXABLE : Accounting.Constants.QUICKBOOKS_INVOICE_LINE_NON_TAXABLE
                            },
                            UnitPrice = taxableAmount
                        },
                        Amount = quantity * taxableAmount
                    }
                },
                TxnTaxDetail = new TxnTaxDetail
                {
                    TxnTaxCodeRef = new QuickBooksOnline.Models.Reference { Value = taxCodeId }
                },
                PrivateNote = memo,
                SalesTermRef = new QuickBooksOnline.Models.Reference { Value = Constants.QUICKBOOKS_TERMS_DUE_NOW.ToString() }
            };
            return invoice;
        }
    }
}
