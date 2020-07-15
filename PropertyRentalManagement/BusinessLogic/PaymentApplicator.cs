using System.Collections.Generic;
using System.Linq;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
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
                unappliedPayment.Line = unappliedPayment.Line ?? new List<PaymentLine>();
                unappliedPayment.Line.Add(newPaymentLine);

                invoice.Balance -= paymentAmount;
                Client.SparseUpdate(unappliedPayment);
            }
        }

        public Payment CreatePayment(
            Invoice invoice,
            string customerId,
            decimal maxPayment,
            string date,
            string memo)
        {
            var payment = GetPayment(maxPayment, invoice?.Balance);
            var appliedPayment = new Payment
            {
                TxnDate = date,
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

        public static decimal GetPayment(decimal payment, decimal? invoiceBalance)
        {
            return !invoiceBalance.HasValue || payment <= invoiceBalance
                ? payment
                : invoiceBalance.GetValueOrDefault();
        }
    }
}
