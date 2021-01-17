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
            const int MAXIMUM_LOAN_LENGTH = 12;
            var startYear = 2021;
            var startMonth = 2;
            var startDay = 1;
            var startTime = 0;
            const string lender = "LENDER_NAME";
            const string borrower = "BORROWER_NAME";
            const string wittness = "WITTNESS_NAME";
            var loanDate = new DateTime(startYear, startMonth, startDay, startTime, startTime, startTime, DateTimeKind.Utc);
            Output.WriteLine($"Lender: {lender}");
            Output.WriteLine($"Borrower: {borrower}");
            Output.WriteLine($"Loan Transfer Date: {loanDate:D}");
            Output.WriteLine($"Loan Amount: {INITIAL_LOAN_AMOUNT:C} United States Dollars");
            Output.WriteLine($"Loan Interest Annual Percentage Rate: {apr}");
            Output.WriteLine("Interest Type: Compounding Monthly in Perpetuity");
            Output.WriteLine("");
            var minimumLoanRepayment = INITIAL_LOAN_AMOUNT + GetInterest(INITIAL_LOAN_AMOUNT, monthlyRate);
            Output.WriteLine($"TERMS \n" +
                             $"All payments have a 7 day grace period. " +
                             $"Early repayment is allowed, creating a new amortization schedule, but the total repayment can't be less than {minimumLoanRepayment:C}. " +
                             $"The loan SHOULD be repaid on {loanDate.AddMonths(loanLengthInMonths):D} according to the amortization schedule in TABLE 1. " +
                             $"If the loan is not repaid by {loanDate.AddMonths(loanLengthInMonths):D}, interest will accrue each month and the loan MUST be repaid according to the amortization schedule shown in TABLE 2.");
            Output.WriteLine("\nIf the loan isn't repaid at a minimum according to the amortization schedule in TABLE 2, then the loan must be repaid in full immediately or legal action can be taken with the losing party paying all attorney's fees.");

            Output.WriteLine(string.Empty);

            Output.WriteLine("TABLE 1");
            PrintAmortization(INITIAL_LOAN_AMOUNT, loanLengthInMonths, loanDate, monthlyRate);

            Output.WriteLine("TABLE 2");
            PrintAmortization(INITIAL_LOAN_AMOUNT, MAXIMUM_LOAN_LENGTH, loanDate, monthlyRate);

            Output.WriteLine($"Lender: {lender}");
            Output.WriteLine("Lender Signature: ________________________________________________");

            Output.WriteLine($"Borrower Name: {borrower}");
            Output.WriteLine("Borrower Signature: ________________________________________________");

            Output.WriteLine($"Witness: {wittness}");
            Output.WriteLine("Witness Signature: ________________________________________________");
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
