using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PropertyRentalManagement.BusinessLogic
{
    public class ReceiptValidation
    {
        private SpotReservationCheck SpotReservationCheck { get; }

        public ReceiptValidation(SpotReservationCheck spotReservationCheck)
        {
            SpotReservationCheck = spotReservationCheck;
        }

        public List<string> Validate(Receipt receipt)
        {
            List<string> errors = new List<string>();

            var customerId = receipt.Customer?.Id;
            var customerName = receipt.Customer?.Name;

            if (receipt.RentalAmount.GetValueOrDefault() > 0)
            {
                if (string.IsNullOrWhiteSpace(receipt.RentalDate))
                {
                    errors.Add("Rental date is required");
                }
                else if (!DateTime.TryParseExact(receipt.RentalDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var rentalDate))
                {
                    errors.Add("Rental date must be in the format YYYY-MM-DD e.g. 1989-06-16");
                }
                else if(rentalDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    errors.Add("Rental date must be a Sunday"); // Required for spot reservations to reduce complexity.
                }
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

            if ((receipt.Memo ?? string.Empty).Length > Constants.QUICKBOOKS_MEMO_MAX_LENGTH)
            {
                errors.Add($"Memo must be less than or equal to {Constants.QUICKBOOKS_MEMO_MAX_LENGTH} characters");
            }

            if (!errors.Any())
            {
                errors.AddRange(SpotReservationCheck.GetSpotConflicts(receipt.Spots, receipt.RentalDate));
            }

            return errors;
        }

    }
}
