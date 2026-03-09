using Svend.LoanCalculator.Api.Shared;

namespace Svend.LoanCalculator.Api.Features.Simulate;

public class SimulateRequest
{
    public Table Table { get; set; }
    public decimal LoanAmount { get; set; }
    public int LoanTermInMonths { get; set; }
    public decimal InterestRate { get; set; }
    public decimal? InflationRate { get; set; }

    public SimulateIntermediatePaymentsRequest? IntermediatePayments { get; set; }
    public SimulateMonthlyFeesRequest? MonthlyFees { get; set; }
}

public class SimulateIntermediatePaymentsRequest
{
    public decimal? Amount { get; set; }
    public int? Term { get; set; }
    public IntermediatePaymentsType? Type { get; set; }
}

public class SimulateMonthlyFeesRequest
{
    public decimal MipPercentage { get; set; }
    public decimal DfiFee { get; set; }
    public decimal AdministrationFee { get; set; }
}
