using System;
using System.Collections.Generic;
using PropertyRentalManagement.DatabaseModel;

namespace PropertyRentalManagement.BusinessLogic
{
    public class ReceiptValidation
    {
        public List<string> Validate(Receipt receipt)
        {
            List<string> errors = new List<string>();

            var customerId = receipt.Customer?.Id;
            var customerName = receipt.Customer?.Name;

            if (string.IsNullOrWhiteSpace(receipt.RentalDate))
            {
                errors.Add("Rental date is required");
            }
            else if (!IsRentalDateValid(receipt.RentalDate))
            {
                errors.Add("Rental date must be in the format YYYY-MM-DD e.g. 1989-16-06");
            }

            if (string.IsNullOrWhiteSpace(receipt.TransactionDate))
            {
                errors.Add("Transaction date is required");
            }
            else if (!IsRentalDateValid(receipt.RentalDate))
            {
                errors.Add("Transaction date must be in the format YYYY-MM-DD e.g. 1989-16-06");
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

            return errors;
        }

        private bool IsRentalDateValid(string rentalDate)
        {
            var parts = rentalDate.Split("-");
            if (parts.Length != 3)
            {
                return false;
            }
            if (!int.TryParse(parts[0], out int year))
            {
                return false;
            }
            if (!int.TryParse(parts[1], out int month))
            {
                return false;
            }
            if (!int.TryParse(parts[2], out int dayOfMonth))
            {
                return false;
            }
            try
            {
                var date = new DateTime(year, month, dayOfMonth);
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
