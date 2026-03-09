using MathAnalytics;
using Svend.LoanCalculator.Api.Shared;

namespace Svend.LoanCalculator.Api.LoanCalculators.Impl;

public abstract class LoanCalculatorBase
{
    public CalculatorResult Calculate(CalculatorRequest req)
    {
        Validate.GreaterThan(req.LoanAmount, 0M, "err_loan_amount_should_be_greater_than_zero");
        Validate.GreaterThan(req.LoanTermInMonths, 0, "err_loan_term_should_be_greater_than_zero");
        Validate.LessOrEqualThan(req.LoanTermInMonths, 420, "err_loan_term_should_be_less_or_equal_than_420");

        var result = new CalculatorResult();
        var monthlyFee = req.InterestRate.ProrataRate(12) / 100;
        var monthlyInflationRate = req.InflationRate.HasValue ? req.InflationRate.Value.ProrataRate(12) / 100 : 0M;

        var installments = CalculateInstallments(req, monthlyFee, monthlyInflationRate, (mr, iRes) => mr.InstallmentResult = iRes);

        if (req.IntermediatePayments != null)
        {
            var intermediates = CalculateIntermediatePaymentsInstallments(req);

            MergeIntermediatePaymentsWithInstallments(req, installments, intermediates);

            result.FirstInstallment = installments.OrderBy(x => x.Month).First().ConsolidatedResult.InstallmentWithCostAndTaxes;
            result.LastInstallment = installments.OrderBy(x => x.Month).Last().ConsolidatedResult.InstallmentWithCostAndTaxes;
        }
        else
        {
            result.FirstInstallment = installments.OrderBy(x => x.Month).First().InstallmentResult.InstallmentWithCostAndTaxes;
            result.LastInstallment = installments.OrderBy(x => x.Month).Last().InstallmentResult.InstallmentWithCostAndTaxes;
        }

        result.TableId = GetTable().ToString();
        result.Installments = installments;
        result.MinimumMonthlyIncome = Math.Ceiling(installments.Max(x => x.InstallmentResult.InstallmentWithCostAndTaxes) / 0.25M);
        result.IntermediatePaymentsAmount = req.IntermediatePayments?.IntermediatePaymentsAmount ?? 0M;
        result.InterestRate = req.InterestRate;
        result.InflationRate = req.InflationRate;

        return result;
    }

    protected abstract List<MonthlyResult> CalculateInstallments(CalculatorRequest req, decimal monthlyFee, decimal monthlyInflationRate, Action<MonthlyResult, InstallmentResult> action);

    protected abstract Table GetTable();

    #region Priv and Protected methods

    private List<MonthlyResult> CalculateIntermediatePaymentsInstallments(CalculatorRequest req)
    {
        int basisFee = GetBasisFee(req);
        var rate = req.InterestRate.ProrataRate(basisFee) / 100;
        var inflation = req.InflationRate.HasValue ? req.InflationRate.Value.ProrataRate(basisFee) / 100 : 0M;

        return CalculateInstallments(
            new CalculatorRequest
            {
                LoanAmount = req.IntermediatePayments!.IntermediatePaymentsAmount!.Value,
                LoanTermInMonths = req.IntermediatePayments!.IntermediatePaymentsTerm!.Value
            },
            rate,
            inflation,
            (mr, iRes) => mr.IntermediateResult = iRes);
    }

    private static void MergeIntermediatePaymentsWithInstallments(CalculatorRequest req, List<MonthlyResult> installments, List<MonthlyResult> intermediates)
    {
        var basisFee = GetBasisFee(req);
        var denominator = 12 / basisFee;
        var countIntermediates = 0;

        var intermediateRemainingDebt = req.IntermediatePayments!.IntermediatePaymentsAmount!.Value;

        bool shouldCalculateIntermediatePayment(int month, int intermediatePaymentsTerm) => month % denominator == 0 && countIntermediates < intermediatePaymentsTerm;

        foreach (var installment in installments)
        {
            var consolidated = new ConsolidatedResult
            {
                InitialRemainingDebt = installment.InstallmentResult.InitialRemainingDebt + intermediateRemainingDebt,
                Installment = installment.InstallmentResult.Installment
            };

            (consolidated.Mip, consolidated.Dfi, consolidated.AdministrationFee) = CalculateMonthlyFees(req, consolidated.InitialRemainingDebt);

            consolidated.InstallmentWithCostAndTaxes = consolidated.Installment + consolidated.Mip.GetValueOrDefault() + consolidated.Dfi.GetValueOrDefault() + consolidated.AdministrationFee.GetValueOrDefault();

            if (shouldCalculateIntermediatePayment(installment.Month, req.IntermediatePayments!.IntermediatePaymentsTerm!.Value))
            {
                var intermediate = intermediates[installment.Month / denominator - 1];

                intermediateRemainingDebt = intermediate.IntermediateResult.FinalRemainingDebt;
                consolidated.IntermediatePayment = intermediate.IntermediateResult.Installment;

                installment.IntermediateResult = intermediate.IntermediateResult;

                countIntermediates++;
            }

            consolidated.FinalRemainingDebt = installment.InstallmentResult.FinalRemainingDebt + intermediateRemainingDebt;

            installment.ConsolidatedResult = consolidated;
        }
    }

    private static int GetBasisFee(CalculatorRequest req)
    {
        var type = req.IntermediatePayments!.IntermediatePaymentsType!.Value;

        if (type.Equals(IntermediatePaymentsType.Annual)) return 1;
        if (type.Equals(IntermediatePaymentsType.Semiannual)) return 2;

        throw new BusinessException("err_invalid_type");
    }

    protected static void CalculateMonthlyFees(CalculatorRequest req, decimal lastRemainingDebt, InstallmentResult installmentResult)
    {
        (installmentResult.Mip, installmentResult.Dfi, installmentResult.AdministrationFee) = CalculateMonthlyFees(req, lastRemainingDebt);

        installmentResult.InstallmentWithCostAndTaxes = installmentResult.Installment + installmentResult.Mip.GetValueOrDefault() + installmentResult.Dfi.GetValueOrDefault() + installmentResult.AdministrationFee.GetValueOrDefault();
    }

    private static (decimal mip, decimal dfi, decimal administrationFee) CalculateMonthlyFees(CalculatorRequest req, decimal lastRemainingDebt)
    {
        if (req.MonthlyFees != null)
        {
            var mip = lastRemainingDebt * (req.MonthlyFees.MipPercentage / 100);
            var dfi = req.MonthlyFees.DfiFee;
            var administrationFee = req.MonthlyFees.AdministrationFee;

            return (mip, dfi, administrationFee);
        }

        return (0M, 0M, 0M);
    }
}

#endregion
