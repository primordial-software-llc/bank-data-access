using System;
using Xunit;
using Xunit.Abstractions;

namespace BankDataAccess.Tests
{
    public class LoanInterestSchedule
    {
        private ITestOutputHelper Output { get; }

        public LoanInterestSchedule(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void Test()
        {
            var apr = .07m;
            var monthlyRate = apr / 12;
            const decimal INITIAL_LOAN_AMOUNT = 1200m;
            decimal currentLoanPayoffAmount = INITIAL_LOAN_AMOUNT;

            var startYear = 2019;
            var startMonth = 11;
            var startDay = 19;
            var startTime = 0;
            var loanDate = new DateTime(startYear, startMonth, startDay, startTime, startTime, startTime, DateTimeKind.Utc);
            var currentDate = new DateTime(startYear, startMonth, startDay, startTime, startTime, startTime, DateTimeKind.Utc);
            var sampleEndDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var compoundIntervalMonths = 1;
            Output.WriteLine($"Loan Transfer Date: {currentDate:D}");
            Output.WriteLine($"Loan Amount: {currentLoanPayoffAmount:C} UTC United States Dollars");
            Output.WriteLine($"Loan Interest Annual Percentage Rate: {apr}");
            Output.WriteLine("Interest Type: Compounding Monthly in Perpetuity");
            Output.WriteLine("");
            Output.WriteLine($"Note: There is a minimum loan cost of one month's interest." +
                             $"The loan payoff amount is a sample of the potential payoff amounts. " +
                             $"The actual loan payoff amount will continue to compound monthly in perpetuity until the loan plus interest is paid in full. " +
                             $"Partial payments are allowed." +
                             $"A new payoff amount, using the same interest mechanics, will be provided after a partial payment is made. ");
            Output.WriteLine(string.Empty);
            currentDate = currentDate.AddMonths(compoundIntervalMonths);
            currentLoanPayoffAmount += currentLoanPayoffAmount * (monthlyRate * compoundIntervalMonths);
            do
            {
                Output.WriteLine($"Loan Payoff Amount if paid before {currentDate.AddDays(1):yyyy MMM dd} UTC: {currentLoanPayoffAmount:C} United States Dollars");
                currentLoanPayoffAmount += currentLoanPayoffAmount * (monthlyRate * compoundIntervalMonths);
                currentDate = currentDate.AddMonths(compoundIntervalMonths);
            } while (currentDate < sampleEndDate);
        }
    }
}
