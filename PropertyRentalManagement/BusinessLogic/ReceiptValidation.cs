using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using PropertyRentalManagement.DatabaseModel;

namespace PropertyRentalManagement.BusinessLogic
{
    public class ReceiptValidation
    {
        private ISpotReservationCheck SpotReservationCheck { get; }

        public ReceiptValidation(ISpotReservationCheck spotReservationCheck)
        {
            SpotReservationCheck = spotReservationCheck;
        }

        public static List<string> GetRentalDateValidation(string rentalDate)
        {
            var validation = new List<string>();
            if (string.IsNullOrWhiteSpace(rentalDate))
            {
                validation.Add("Rental date is required");
            }
            else if (!DateTime.TryParseExact(rentalDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedRentalDate))
            {
                validation.Add("Rental date must be in the format YYYY-MM-DD e.g. 1989-06-16");
            }
            else if (parsedRentalDate.DayOfWeek != DayOfWeek.Sunday)
            {
                validation.Add("Rental date must be a Sunday"); // Required for spot reservations to reduce complexity.
            }
            return validation;
        }

        public async Task<List<string>> Validate(Receipt receipt)
        {
            List<string> errors = new List<string>();

            var customerId = receipt.Customer?.Id;
            var customerName = receipt.Customer?.Name;

            if (receipt.RentalAmount.GetValueOrDefault() > 0 || receipt.Spots != null && receipt.Spots.Any())
            {
                errors.AddRange(GetRentalDateValidation(receipt.RentalDate));
            }

            if (string.IsNullOrWhiteSpace(customerId) && string.IsNullOrWhiteSpace(customerName))
            {
                errors.Add("Customer id or customer name is required");
            }
            else if (!string.IsNullOrWhiteSpace(customerId) &&
                     (!int.TryParse(customerId, out int customerIdParsed) ||
                     customerIdParsed < 1))
            {
                errors.Add("Customer id isn't a valid positive integer");
            }
            else if (!string.IsNullOrWhiteSpace(customerName) &&
                     customerName.Length > Constants.QUICKBOOKS_CUSTOMER_DISPLAY_NAME_MAX_LENGTH)
            {
                errors.Add($"Customer name can't exceed {Constants.QUICKBOOKS_CUSTOMER_DISPLAY_NAME_MAX_LENGTH} characters");
            }

            if (receipt.RentalAmount.GetValueOrDefault() == 0 && receipt.Spots != null && receipt.Spots.Any())
            {
                errors.Add("Rental amount is required in order to reserve a spot");
            }
            
            if (receipt.RentalAmount.GetValueOrDefault() == 0 && receipt.ThisPayment.GetValueOrDefault() == 0)
            {
                errors.Add("Rental amount or payment is required");
            }

            if (receipt.RentalAmount.HasValue && receipt.RentalAmount < 0)
            {
                errors.Add("Rental amount must be greater or equal to zero");
            }

            if (receipt.ThisPayment.HasValue && receipt.ThisPayment < 0)
            {
                errors.Add("This payment must be greater or equal to zero");
            }

            if (receipt.MakeCardPayment.GetValueOrDefault())
            {
                if (receipt.ThisPayment.GetValueOrDefault() <= 0)
                {
                    errors.Add("Payment is required.");
                }
                if (receipt.CardPayment == null || string.IsNullOrWhiteSpace(receipt.CardPayment.CardNumber))
                {
                    errors.Add("Credit card number is required.");
                }
                if (receipt.CardPayment == null || string.IsNullOrWhiteSpace(receipt.CardPayment.ExpirationMonth))
                {
                    errors.Add("Expiration month is required.");
                }
                if (receipt.CardPayment == null || string.IsNullOrWhiteSpace(receipt.CardPayment.ExpirationYear))
                {
                    errors.Add("Expiration year is required.");
                }
                if (receipt.CardPayment == null || string.IsNullOrWhiteSpace(receipt.CardPayment.Cvv))
                {
                    errors.Add("CVV is required.");
                }
            }

            if ((receipt.Memo ?? string.Empty).Length > Constants.QUICKBOOKS_MEMO_MAX_LENGTH)
            {
                errors.Add($"Memo must be less than or equal to {Constants.QUICKBOOKS_MEMO_MAX_LENGTH} characters");
            }

            if (!errors.Any())
            {
                errors.AddRange(await SpotReservationCheck.GetSpotConflicts(receipt.Spots, receipt.RentalDate));
            }

            return errors;
        }

    }
}
