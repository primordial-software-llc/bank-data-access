using System;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests
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
            var apr = .15m;
            var monthlyRate = apr / 12;
            var loanLengthInMonths = 2;
            const decimal INITIAL_LOAN_AMOUNT = 30 * 1000m;
            var startYear = 2021;
            var startMonth = 2;
            var startDay = 1;
            var startTime = 0;
            var loanDate = new DateTime(startYear, startMonth, startDay, startTime, startTime, startTime, DateTimeKind.Utc);
            Output.WriteLine($"Loan Transfer Date: {loanDate:D}");
            Output.WriteLine($"Loan Amount: {INITIAL_LOAN_AMOUNT:C} United States Dollars");
            Output.WriteLine($"Loan Interest Annual Percentage Rate: {apr}");
            Output.WriteLine("Interest Type: Compounding Monthly in Perpetuity");
            PrintAmortization(INITIAL_LOAN_AMOUNT, loanLengthInMonths, loanDate, monthlyRate);
        }

        private void PrintAmortization(decimal loanAmount, int loanLengthInMonths, DateTime loanDate, decimal monthlyRate)
        {
            int remainingLoanLengthInMonths = loanLengthInMonths;
            decimal currentLoanPayoffAmount = loanAmount;
            var currentRepaymentDate = loanDate.AddMonths(1);
            for (var ct = 0; ct < loanLengthInMonths; ct++)
            {
                Output.WriteLine($"{currentRepaymentDate:D}");
                var interest = currentLoanPayoffAmount * monthlyRate;
                var principlePayment = currentLoanPayoffAmount / remainingLoanLengthInMonths;
                currentLoanPayoffAmount -= principlePayment;
                Output.WriteLine($"Principle Due {principlePayment:C}. Interest Due {interest:C}. Total Due {principlePayment + interest:C}. Remaining balance {currentLoanPayoffAmount:C}.");
                Output.WriteLine("");
                currentRepaymentDate = currentRepaymentDate.AddMonths(1);
                remainingLoanLengthInMonths -= 1;
            }
        }

        private decimal GetInterest(decimal principle, decimal monthlyRate)
        {
            return principle * monthlyRate;
        }
    }
}
