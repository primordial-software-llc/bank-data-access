using AwsDataAccess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Extensions;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using Vendor = PropertyRentalManagement.DatabaseModel.Vendor;

namespace PropertyRentalManagement.BusinessLogic
{
    public class ReceiptSave
    {
        private DatabaseClient<ReceiptSaveResult> ReceiptDbClient { get; }
        private DatabaseClient<SpotReservation> SpotReservationDbClient { get; }
        private QuickBooksOnlineClient QuickBooksClient { get; }
        private Location Location { get; }
        private ILogging Logger { get; }
        private CardPayment CardPayment { get; }

        public ReceiptSave(
            DatabaseClient<ReceiptSaveResult> receiptDbClient,
            QuickBooksOnlineClient quickBooksClient,
            DatabaseClient<SpotReservation> spotReservationDbClient,
            ILogging logger,
            CardPayment cardPayment,
            Location location)
        {
            ReceiptDbClient = receiptDbClient;
            QuickBooksClient = quickBooksClient;
            SpotReservationDbClient = spotReservationDbClient;
            Logger = logger;
            CardPayment = cardPayment;
            Location = location;
        }

        public ReceiptSaveResult SaveReceipt(
            Receipt receipt,
            string customerId,
            string firstName,
            string lastName,
            string email,
            Vendor vendor,
            string userIp,
            string invoiceItemId)
        {
            ReceiptSaveResult result = null;
            try
            {
                SaveReceiptCore(
                    receipt,
                    customerId,
                    firstName,
                    lastName,
                    email,
                    vendor,
                    userIp,
                    invoiceItemId,
                    out result);
            }
            catch (Exception e)
            {
                if (result != null)
                {
                    Logger.Log("Failed to process receipt. Reverting charge, invoice, payments and spot reservations: " + e);
                    Revert(result);
                }
                throw;
            }

            if (receipt.MakeCardPayment.GetValueOrDefault() && (result.CardCaptureResult == null || result.CardCaptureResult["error"] != null))
            {
                Logger.Log("Failed to get card capture result. Reverting charge, invoice, payments and spot reservations.");
                Revert(result);
            }

            return result;
        }

        private void SaveReceiptCore(
            Receipt receipt,
            string customerId,
            string firstName,
            string lastName,
            string email,
            Vendor vendor,
            string userIp,
            string invoiceItemId,
            out ReceiptSaveResult result)
        {
            var existing = ReceiptDbClient.Get(new ReceiptSaveResult { Id = receipt.Id }, true).Result;
            if (existing != null)
            {
                throw new Exception($"A receipt with id {receipt.Id} has already been sent for processing.");
            }
            var receiptCopyToSave = JsonConvert.DeserializeObject<Receipt>(JsonConvert.SerializeObject(receipt));
            receiptCopyToSave.CardPayment = null;
            result = new ReceiptSaveResult
            {
                Id = receipt.Id,
                Timestamp = DateTime.UtcNow.ToString("O"),
                Receipt = receiptCopyToSave,
                Vendor = vendor,
                CreatedBy = new ReceiptSaveResultUser
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Ip = userIp
                }
            };
            ReceiptDbClient.Create(result);
            var memo = receipt.Memo;
            if (receipt.MakeCardPayment.GetValueOrDefault())
            {
                var last4 = receipt.CardPayment.CardNumber.Substring(receipt.CardPayment.CardNumber.Length - 4);
                Logger.Log($"{firstName} {lastName} {email} is charging card ending in {last4} ${receipt.ThisPayment:C} on {DateTime.UtcNow:O} for receipt {result.Id}.");
                var chargeResult = CardPayment.Authorize(
                    receipt.ThisPayment * 100,
                    receipt.CardPayment.CardNumber,
                    receipt.CardPayment.ExpirationMonth,
                    receipt.CardPayment.ExpirationYear,
                    receipt.CardPayment.Cvv,
                    receipt.IsCardPresent);
                result.CardAuthorizationResult = chargeResult;
                ReceiptDbClient.Create(result);
                if (chargeResult["error"] != null)
                {
                    return;
                }
            }

            if (receipt.Spots != null && receipt.Spots.Any())
            {
                memo += Environment.NewLine + "Spots: " + string.Join(", ", receipt.Spots.Select(x => $"{x.Section?.Name} - {x.Name}"));
            }

            if (receipt.RentalAmount > 0)
            {
                result.Invoice = QuickBooksClient.Create(
                    new InvoiceCreation().CreateInvoiceModel(
                        QuickBooksClient,
                        customerId,
                        receipt.RentalAmount,
                        receipt.RentalDate,
                        memo,
                        invoiceItemId,
                        Location.SalesRentalTaxRateId.ToString(),
                        true));
                ReceiptDbClient.Create(result);
            }

            result.Payments = new List<Payment>();
            if (receipt.ThisPayment > 0)
            {
                ZonedClock easternClock = SystemClock.Instance.InZone(DateTimeZoneProviders.Tzdb["America/New_York"]);
                var paymentMemo = $"True Payment Date: {easternClock.GetCurrentDate():yyyy-MM-dd}. " +
                                        $"Payment entered by {firstName} {lastName} from IP {userIp}." +
                                        Environment.NewLine + Environment.NewLine + memo;
                var unpaidInvoices = QuickBooksClient.QueryAll<Invoice>($"select * from Invoice where Balance != '0' and CustomerRef = '{customerId}' ORDERBY TxnDate");
                decimal payment = receipt.ThisPayment.GetValueOrDefault();
                var paymentApplicator = new PaymentApplicator(QuickBooksClient);
                foreach (var unpaidInvoice in unpaidInvoices)
                {
                    var paymentAppliedToInvoice = paymentApplicator.CreatePayment(
                        unpaidInvoice,
                        customerId,
                        payment,
                        $"{easternClock.GetCurrentDate():yyyy-MM-dd}",
                        paymentMemo);
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
                        $"{easternClock.GetCurrentDate():yyyy-MM-dd}",
                        paymentMemo
                    );
                    result.Payments.Add(unappliedPayment);
                }
            }
            ReceiptDbClient.Create(result);

            if (receipt.Spots != null)
            {
                result.SpotReservations = new List<SpotReservation>();
                foreach (var spot in receipt.Spots)
                {
                    var spotReservation = new SpotReservation
                    {
                        SpotId = spot.Id,
                        RentalDate = receipt.RentalDate,
                        QuickBooksOnlineId = int.Parse(customerId),
                        VendorId = vendor.Id
                    };
                    SpotReservationDbClient.Create(spotReservation);
                    result.SpotReservations.Add(spotReservation);
                }
            }

            if (receipt.MakeCardPayment.GetValueOrDefault())
            {
                result.CardCaptureResult = CardPayment.Capture(result.CardAuthorizationResult["id"].Value<string>());
                for (var ct = 0; ct < result.Payments.Count; ct++)
                {
                    result.Payments[ct].PrivateNote += Environment.NewLine + Environment.NewLine + $"Card payment confirmation {result.CardAuthorizationResult.ToString(Formatting.Indented)}";
                    result.Payments[ct] = QuickBooksClient.SparseUpdate(result.Payments[ct]);
                }
            }
            ReceiptDbClient.Create(result);
        }

        private void Revert(ReceiptSaveResult result)
        {
            if (result.Invoice != null)
            {
                QuickBooksClient.Delete(result.Invoice);
                result.Invoice = null;
            }
            foreach (var payment in result.Payments ?? new List<Payment>())
            {
                QuickBooksClient.Delete(payment);
            }
            result.Payments = null;
            foreach (var reservation in result.SpotReservations ?? new List<SpotReservation>())
            {
                SpotReservationDbClient.Delete(reservation);
            }
            result.SpotReservations = null;
        }
    }
}
