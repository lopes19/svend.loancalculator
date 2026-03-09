namespace Svend.LoanCalculator.Api.LoanCalculators;

public interface ILoanCalculator
{
    CalculatorResult Calculate(CalculatorRequest req);
}
