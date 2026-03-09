using MathAnalytics;
using Svend.LoanCalculator.Api.Shared;

namespace Svend.LoanCalculator.Api.LoanCalculators.Impl;

public class PriceLoanCalculator : LoanCalculatorBase, ILoanCalculator
{
    protected override List<MonthlyResult> CalculateInstallments(CalculatorRequest req, decimal monthlyFee, decimal monthlyInflationRate, Action<MonthlyResult, InstallmentResult> action)
    {
        var lastRemainingDebt = req.LoanAmount;

        var installments = new List<MonthlyResult>();

        for (int i = 0; i < req.LoanTermInMonths; i++)
        {
            var monthlyInstallment = new MonthlyResult
            {
                Month = i + 1
            };

            var installmentResult = new InstallmentResult
            {
                InitialRemainingDebt = lastRemainingDebt
            };

            var inflation = lastRemainingDebt * monthlyInflationRate;
            lastRemainingDebt += inflation;
            var installment = (-lastRemainingDebt).Payment(monthlyFee, req.LoanTermInMonths - i);
            var interest = lastRemainingDebt * monthlyFee;
            var amortization = installment - interest;
            var remainingDebt = (lastRemainingDebt + interest) - installment;

            installmentResult.Inflation = inflation;
            installmentResult.InflationAdjustedRemainingDebt = lastRemainingDebt;
            installmentResult.Interest = interest;
            installmentResult.Amortization = amortization;
            installmentResult.Installment = installment;
            installmentResult.FinalRemainingDebt = remainingDebt;

            CalculateMonthlyFees(req, lastRemainingDebt, installmentResult);

            action(monthlyInstallment, installmentResult);

            installments.Add(monthlyInstallment);

            lastRemainingDebt = remainingDebt;
        }

        return installments;
    }

    protected override Table GetTable()
    {
        return Table.Price;
    }
}
