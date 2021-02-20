using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Payments;

namespace PropertyRentalManagement.BusinessLogic
{
    public class PaymentApplicator
    {
        private QuickBooksOnlineClient Client { get; set; }

        public PaymentApplicator(QuickBooksOnlineClient client)
        {
            Client = client;
        }

        public void ApplyUnappliedPaymentsToInvoice(Invoice invoice)
        {
            var unappliedPayments = Client
                .QueryAll<Payment>($"select * from Payment where CustomerRef = '{invoice.CustomerRef.Value}' ORDERBY TxnDate")
                .Where(x => x.UnappliedAmt > 0)
                .ToList();
            foreach (var unappliedPayment in unappliedPayments)
            {
                if (invoice.Balance == 0)
                {
                    return;
                }

                decimal paymentAmount = GetPayment(unappliedPayment.UnappliedAmt.GetValueOrDefault(), invoice.Balance);
                var newPaymentLine = new PaymentLine
                {
                    Amount = paymentAmount,
                    LinkedTxn = new List<LinkedTransaction>
                    {
                        new LinkedTransaction {TxnId = invoice.Id.ToString(), TxnType = "Invoice"}
                    }
                };
                unappliedPayment.Line ??= new List<PaymentLine>();
                unappliedPayment.Line.Add(newPaymentLine);

                var paymentDate = GetPaymentDate(unappliedPayment.TxnDate, invoice.TxnDate);
                if (paymentDate != unappliedPayment.TxnDate)
                {
                    var newNote = "This payment was automatically applied to an invoice during invoice generation. " +
                                       $"The payment date was moved from {unappliedPayment.TxnDate} to the date of the invoice it's being applied to {paymentDate}.";
                    unappliedPayment.TxnDate = paymentDate;
                    if (!string.IsNullOrWhiteSpace(unappliedPayment.PrivateNote))
                    {
                        newNote = newNote + Environment.NewLine + Environment.NewLine + unappliedPayment.PrivateNote;
                    }
                    unappliedPayment.PrivateNote = newNote;
                }

                invoice.Balance -= paymentAmount;
                Client.SparseUpdate(unappliedPayment);
            }
        }

        public Payment CreatePayment(
            Invoice invoice,
            string customerId,
            decimal maxPayment,
            string datePaymentOccurred,
            string memo)
        {
            var payment = GetPayment(maxPayment, invoice?.Balance);
            var appliedPayment = new Payment
            {
                TxnDate = GetPaymentDate(datePaymentOccurred, invoice?.TxnDate),
                CustomerRef = new QuickBooksOnline.Models.Reference { Value = customerId },
                TotalAmount = payment,
                PrivateNote = memo
            };
            if (invoice != null)
            {
                appliedPayment.Line = new List<PaymentLine>
                {
                    new PaymentLine
                    {
                        Amount = payment,
                        LinkedTxn = new List<LinkedTransaction>
                        {
                            new LinkedTransaction {TxnId = invoice.Id.ToString(), TxnType = "Invoice"}
                        }
                    }
                };
            }
            return Client.Create(appliedPayment);
        }

        /// <summary>
        /// If payment date is prior to invoice date,
        /// the QuickBooks Online profit and loss report run on a cash basis will put the income under the
        /// "Unapplied Cash Payment Income" account rather than the respective account.
        ///
        /// When the payment date is prior to the invoice date, payment applicator fixes the issue by setting payment date to invoice date
        /// forcing an accrual basis for rental income received prior to the rental date.
        ///
        /// Cash basis income is reported for rental income using the cash basis income endpoint.
        /// </summary>
        public static string GetPaymentDate(string datePaymentOccurred, string invoiceDate)
        {
            if (string.IsNullOrWhiteSpace(invoiceDate))
            {
                return datePaymentOccurred;
            }

            var datePaymentOccurredParsed = System.DateTime.ParseExact(datePaymentOccurred, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            var dateOfInvoice = System.DateTime.ParseExact(invoiceDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

            var prepayment = datePaymentOccurredParsed < dateOfInvoice;
            return prepayment ? invoiceDate : datePaymentOccurred;
        }

        public static decimal GetPayment(decimal payment, decimal? invoiceBalance)
        {
            return !invoiceBalance.HasValue || payment <= invoiceBalance
                ? payment
                : invoiceBalance.GetValueOrDefault();
        }
    }
}
