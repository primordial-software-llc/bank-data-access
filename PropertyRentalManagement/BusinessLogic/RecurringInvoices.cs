using System;
using System.Collections.Generic;
using System.Linq;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;

namespace PropertyRentalManagement.BusinessLogic
{
    public class RecurringInvoices
    {
        public List<Invoice> CreateInvoices(DateTime start, DateTime end, VendorService vendorService, QuickBooksOnlineClient qboClient, string frequency)
        {
            var allInvoices = new SalesReportService().GetInvoices(qboClient, start, end);
            var allActiveCustomers = qboClient.QueryAll<Customer>("select * from customer")
                .ToDictionary(x => x.Id);
            var vendors = new ActiveVendorSearch().GetActiveVendors(allActiveCustomers, vendorService, frequency);
            var newInvoices = new List<Invoice>();
            foreach (var vendor in vendors.Values)
            {
                var vendorInvoices = allInvoices.Where(x => x.CustomerRef.Value == vendor.QuickBooksOnlineId.ToString());
                if (!vendorInvoices.Any())
                {
                    var invoiceDate = string.Equals("weekly", frequency, StringComparison.OrdinalIgnoreCase) ? end : start;
                    newInvoices.Add(CreateInvoice(qboClient, invoiceDate, allActiveCustomers[vendor.QuickBooksOnlineId], vendor));
                }
            }
            var paymentApplicator = new PaymentApplicator(qboClient);
            foreach (var invoice in newInvoices)
            {
                paymentApplicator.ApplyUnappliedPaymentsToInvoice(invoice);
            }
            return newInvoices;
        }

        private Invoice CreateInvoice(QuickBooksOnlineClient qboClient, DateTime date, Customer customer, DatabaseModel.Vendor vendor)
        {
            decimal quantity = 1;
            decimal taxableAmount = vendor.RentPrice.GetValueOrDefault() / 1.068m;
            var invoice = new Invoice
            {
                TxnDate = date.ToString("yyyy-MM-dd"),
                CustomerRef = new Reference { Value = customer.Id.ToString() },
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
                PrivateNote = vendor.Memo,
                SalesTermRef = new Reference { Value = Constants.QUICKBOOKS_TERMS_DUE_NOW.ToString() }
            };
            return qboClient.Create(invoice);
        }

    }
}
