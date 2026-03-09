using Svend.LoanCalculator.Api.LoanCalculators.Impl;
using Svend.LoanCalculator.Api.Shared;

namespace Svend.LoanCalculator.Api.LoanCalculators;

public static class LoanCalculatorFactory
{
    public static ILoanCalculator GetLoanCalculator(Table table)
    {
        return table switch
        {
            Table.Price => new PriceLoanCalculator(),
            Table.Sac => new SACLoanCalculator(),
            Table.Sacoc => new SACOCLoanCalculator(),
            _ => throw new ArgumentException("err_invalid_table"),
        };
    }
}
