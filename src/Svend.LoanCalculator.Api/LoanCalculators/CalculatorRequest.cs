using Svend.LoanCalculator.Api.Shared;

namespace Svend.LoanCalculator.Api.LoanCalculators;

public class CalculatorRequest
{
    public decimal LoanAmount { get; set; }
    public int LoanTermInMonths { get; set; }
    public decimal InterestRate { get; set; }
    public decimal? InflationRate { get; set; }

    public IntermediatePaymentsRequest? IntermediatePayments { get; set; } = null;

    public MonthlyFeesRequest? MonthlyFees { get; set; } = null;
}

public class IntermediatePaymentsRequest
{
    public IntermediatePaymentsRequest(decimal? intermediatePaymentsAmount, int? intermediatePaymentsTerm, IntermediatePaymentsType? intermediatePaymentsType)
    {
        IntermediatePaymentsAmount = intermediatePaymentsAmount;
        IntermediatePaymentsTerm = intermediatePaymentsTerm;
        IntermediatePaymentsType = intermediatePaymentsType;
    }

    public decimal? IntermediatePaymentsAmount { get; set; }
    public int? IntermediatePaymentsTerm { get; set; }
    public IntermediatePaymentsType? IntermediatePaymentsType { get; set; }
}

public class MonthlyFeesRequest
{
    public MonthlyFeesRequest()
    {
    }

    public MonthlyFeesRequest(decimal mipPercentage, decimal dfiFee, decimal administrationFee)
    {
        MipPercentage = mipPercentage;
        DfiFee = dfiFee;
        AdministrationFee = administrationFee;
    }

    public decimal MipPercentage { get; set; }
    public decimal DfiFee { get; set; }
    public decimal AdministrationFee { get; set; }
}
