namespace Svend.LoanCalculator.Api.LoanCalculators;

public class CalculatorResult
{
    public string TableId { get; set; } = null!;
    public decimal FirstInstallment { get; set; }
    public decimal LastInstallment { get; set; }
    public decimal MinimumMonthlyIncome { get; set; }
    public decimal IntermediatePaymentsAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal? InflationRate { get; set; }

    public List<MonthlyResult> Installments { get; set; } = [];
}

public class MonthlyResult
{
    public int Month { get; set; }

    public InstallmentResult InstallmentResult { get; set; } = new();

    public InstallmentResult IntermediateResult { get; set; } = null!;

    public ConsolidatedResult ConsolidatedResult { get; set; } = null!;
}

public class InstallmentResult
{
    public decimal InitialRemainingDebt { get; set; }
    public decimal Inflation { get; set; }
    public decimal InflationAdjustedRemainingDebt { get; set; }
    public decimal Interest { get; set; }
    public decimal Amortization { get; set; }
    public decimal Installment { get; set; }

    public decimal? Mip { get; set; }
    public decimal? Dfi { get; set; }
    public decimal? AdministrationFee { get; set; }

    public decimal InstallmentWithCostAndTaxes { get; set; }

    public decimal FinalRemainingDebt { get; set; }
}

public class ConsolidatedResult
{
    public decimal InitialRemainingDebt { get; set; }

    public decimal? Mip { get; set; }
    public decimal? Dfi { get; set; }
    public decimal? AdministrationFee { get; set; }

    public decimal Installment { get; set; }
    public decimal InstallmentWithCostAndTaxes { get; set; }
    public decimal IntermediatePayment { get; set; }
    public decimal FinalRemainingDebt { get; set; }
}
