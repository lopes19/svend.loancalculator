using FluentAssertions;
using Svend.LoanCalculator.Api.LoanCalculators;
using Svend.LoanCalculator.Api.LoanCalculators.Impl;
using Svend.LoanCalculator.Api.Shared;

namespace Svend.LoanCalculator.UnitTest.Features.Simulate;

public class PriceLoanCalculatorTest
{
    [Fact]
    public void Calculate_InflationRateIsNull_ShouldReturnCorrectInstallments()
    {
        // Arrange
        var calculator = new PriceLoanCalculator();

        var req = new CalculatorRequest()
        {
            LoanAmount = 436750.00M,
            LoanTermInMonths = 144,
            InterestRate = 12.68M,

            IntermediatePayments = new IntermediatePaymentsRequest(52500M, 4, IntermediatePaymentsType.Annual),

            MonthlyFees = new MonthlyFeesRequest(0.027M, 0M, 25M)
        };

        // Act
        var result = calculator.Calculate(req);

        // Assert
        result.Should().NotBeNull();
        result.TableId.Should().Be(Table.Price.ToString());
        result.MinimumMonthlyIncome.Should().Be(23515M);
        result.IntermediatePaymentsAmount.Should().Be(52500M);

        result.Installments.Should().HaveCount(req.LoanTermInMonths);
        result.Installments.All(x => Math.Round(x.InstallmentResult.Installment, 2) == 5735.76M).Should().BeTrue();
        result.Installments.All(x => x.InstallmentResult.AdministrationFee == 25M).Should().BeTrue();
        result.Installments.All(x => x.InstallmentResult.Dfi == 0M).Should().BeTrue();

        // Asset intermediates
        result.Installments.Where(x => x.Month == 12 || x.Month == 24 || x.Month == 36 || x.Month == 48).All(x => Math.Round(x.ConsolidatedResult.IntermediatePayment, 2) == 17532.98M).Should().BeTrue();

        // Asset first month
        var firstInstallment = result.Installments.First();

        result.FirstInstallment.Should().Be(firstInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes);

        firstInstallment.Month.Should().Be(1);
        firstInstallment.InstallmentResult.Interest.Should().BeApproximately(4366.68M, 0.01M);
        firstInstallment.InstallmentResult.Amortization.Should().BeApproximately(1369.08M, 0.01m);
        firstInstallment.InstallmentResult.FinalRemainingDebt.Should().BeApproximately(435380.92M, 0.01m);

        firstInstallment.ConsolidatedResult.InitialRemainingDebt.Should().BeApproximately(489250.00M, 0.01m);
        firstInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(487880.92M, 0.01m);

        firstInstallment.ConsolidatedResult.Mip.Should().BeApproximately(132.09M, 0.01M);
        firstInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes.Should().BeApproximately(5892.86M, 0.01M);

        // Assert last month
        var lastInstallment = result.Installments.Last();

        result.LastInstallment.Should().Be(lastInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes);

        lastInstallment.Month.Should().Be(144);
        lastInstallment.InstallmentResult.Interest.Should().BeApproximately(56.78M, 0.01M);
        lastInstallment.InstallmentResult.Amortization.Should().BeApproximately(5678.99M, 0.01m);
        lastInstallment.InstallmentResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);

        lastInstallment.ConsolidatedResult.Mip.Should().BeApproximately(1.53M, 0.01M);
        lastInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes.Should().BeApproximately(5762.30M, 0.01M);
        lastInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);
    }

    [Fact]
    public void Calculate_InflationRateSet_ShouldReturnCorrectInstallments()
    {
        // Arrange
        var calculator = new PriceLoanCalculator();

        var req = new CalculatorRequest()
        {
            LoanAmount = 436750.00M,
            LoanTermInMonths = 144,
            InterestRate = 12.68M,
            InflationRate = 4.5M,

            IntermediatePayments = new IntermediatePaymentsRequest(52500M, 4, IntermediatePaymentsType.Annual),

            MonthlyFees = new MonthlyFeesRequest(0.027M, 0M, 25M)
        };

        // Act
        var result = calculator.Calculate(req);

        // Assert
        result.Should().NotBeNull();
        result.TableId.Should().Be(Table.Price.ToString());
        result.MinimumMonthlyIncome.Should().Be(39020M);
        result.IntermediatePaymentsAmount.Should().Be(52500M);
        result.InterestRate.Should().Be(req.InterestRate);
        result.InflationRate.Should().Be(req.InflationRate);

        result.Installments.Should().HaveCount(req.LoanTermInMonths);
        result.Installments.All(x => x.InstallmentResult.AdministrationFee == 25M).Should().BeTrue();
        result.Installments.All(x => x.InstallmentResult.Dfi == 0M).Should().BeTrue();

        // Asset intermediates
        result.Installments.First(x => x.Month == 12).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(18321.96M, 0.01M);
        result.Installments.First(x => x.Month == 24).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(19146.45M, 0.01M);
        result.Installments.First(x => x.Month == 36).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(20008.04M, 0.01M);
        result.Installments.First(x => x.Month == 48).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(20908.40M, 0.01M);

        // Asset first month
        var firstInstallment = result.Installments.First();

        result.FirstInstallment.Should().Be(firstInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes);

        firstInstallment.Month.Should().Be(1);
        firstInstallment.InstallmentResult.InitialRemainingDebt.Should().BeApproximately(436750.00M, 0.01m);
        firstInstallment.InstallmentResult.Interest.Should().BeApproximately(4382.73M, 0.01M);
        firstInstallment.InstallmentResult.Amortization.Should().BeApproximately(1374.11M, 0.01m);
        firstInstallment.InstallmentResult.Installment.Should().BeApproximately(5756.84M, 0.01m);
        firstInstallment.InstallmentResult.FinalRemainingDebt.Should().BeApproximately(436980.86M, 0.01m);

        firstInstallment.ConsolidatedResult.InitialRemainingDebt.Should().BeApproximately(489250.00M, 0.01m);
        firstInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(489480.86M, 0.01m);

        firstInstallment.ConsolidatedResult.Mip.Should().BeApproximately(132.10M, 0.01M);
        firstInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes.Should().BeApproximately(5913.94M, 0.01M);

        // Assert last month
        var lastInstallment = result.Installments.Last();

        result.LastInstallment.Should().Be(lastInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes);

        lastInstallment.Month.Should().Be(144);
        lastInstallment.InstallmentResult.Interest.Should().BeApproximately(96.29M, 0.01M);
        lastInstallment.InstallmentResult.Amortization.Should().BeApproximately(9630.88M, 0.01m);
        lastInstallment.InstallmentResult.Installment.Should().BeApproximately(9727.17M, 0.01m);
        lastInstallment.InstallmentResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);

        lastInstallment.ConsolidatedResult.Mip.Should().BeApproximately(2.60M, 0.01M);
        lastInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes.Should().BeApproximately(9754.77M, 0.01M);
        lastInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);
    }
}
