using FluentAssertions;
using Svend.LoanCalculator.Api.LoanCalculators;
using Svend.LoanCalculator.Api.LoanCalculators.Impl;
using Svend.LoanCalculator.Api.Shared;

namespace Svend.LoanCalculator.UnitTest.Features.Simulate;

public class SACLoanCalculatorTest
{
    [Fact]
    public void Calculate_InflationRateIsNull_ShouldReturnCorrectInstallments()
    {
        // Arrange
        var calculator = new SACLoanCalculator();

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
        result.TableId.Should().Be(Table.Sac.ToString());
        result.FirstInstallment.Should().BeApproximately(7556.76M, 0.01M);
        result.LastInstallment.Should().BeApproximately(3089.13M, 0.01M);
        result.MinimumMonthlyIncome.Should().BeApproximately(30171M, 0.01M);
        result.IntermediatePaymentsAmount.Should().Be(52500M);
        result.InterestRate.Should().Be(req.InterestRate);
        result.InflationRate.Should().Be(req.InflationRate);

        result.Installments.Should().HaveCount(req.LoanTermInMonths);
        result.Installments.All(x => Math.Round(x.InstallmentResult.Amortization, 2) == 3032.99M).Should().BeTrue();
        result.Installments.All(x => x.InstallmentResult.AdministrationFee == 25M).Should().BeTrue();
        result.Installments.All(x => x.InstallmentResult.Dfi == 0M).Should().BeTrue();

        // Asset first month
        var firstInstallment = result.Installments.First();

        firstInstallment.Month.Should().Be(1);
        firstInstallment.InstallmentResult.Interest.Should().BeApproximately(4366.68M, 0.01M);
        firstInstallment.InstallmentResult.Installment.Should().BeApproximately(7399.67M, 0.01m);
        firstInstallment.InstallmentResult.FinalRemainingDebt.Should().BeApproximately(433717.01M, 0.01m);

        firstInstallment.ConsolidatedResult.InitialRemainingDebt.Should().BeApproximately(489250.00M, 0.01m);
        firstInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(486217.01M, 0.01m);

        firstInstallment.ConsolidatedResult.Mip.Should().BeApproximately(132.09M, 0.01M);
        firstInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes.Should().BeApproximately(7556.76M, 0.01M);

        // Assert last month
        var lastInstallment = result.Installments.Last();

        lastInstallment.Month.Should().Be(144);
        lastInstallment.InstallmentResult.Interest.Should().BeApproximately(30.32M, 0.01M);
        lastInstallment.InstallmentResult.Installment.Should().BeApproximately(3063.31M, 0.01m);
        lastInstallment.InstallmentResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);

        lastInstallment.ConsolidatedResult.Mip.Should().BeApproximately(0.82M, 0.01M);
        lastInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes.Should().BeApproximately(3089.13M, 0.01M);
        lastInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);
    }

    [Fact]
    public void Calculate_InflationRateSet_ShouldReturnCorrectInstallments()
    {
        // Arrange
        var calculator = new SACLoanCalculator();

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
        result.TableId.Should().Be(Table.Sac.ToString());
        result.FirstInstallment.Should().BeApproximately(7583.95M, 0.01M);
        result.LastInstallment.Should().BeApproximately(5221.39M, 0.01M);
        result.MinimumMonthlyIncome.Should().BeApproximately(30281M, 0.01M);
        result.IntermediatePaymentsAmount.Should().Be(52500M);

        result.Installments.Should().HaveCount(req.LoanTermInMonths);
        result.Installments.All(x => x.InstallmentResult.AdministrationFee == 25M).Should().BeTrue();
        result.Installments.All(x => x.InstallmentResult.Dfi == 0M).Should().BeTrue();

        // Asset intermediates
        result.Installments.First(x => x.Month == 12).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(20672.18M, 0.01M);
        result.Installments.First(x => x.Month == 24).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(19785.03M, 0.01M);
        result.Installments.First(x => x.Month == 36).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(18776.17M, 0.01M);
        result.Installments.First(x => x.Month == 48).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(17636.45M, 0.01M);

        // Asset first month
        var firstInstallment = result.Installments.First();

        firstInstallment.Month.Should().Be(1);
        firstInstallment.InstallmentResult.Interest.Should().BeApproximately(4382.73M, 0.01M);
        firstInstallment.InstallmentResult.Amortization.Should().BeApproximately(3044.13M, 0.01m);
        firstInstallment.InstallmentResult.Installment.Should().BeApproximately(7426.86M, 0.01m);
        firstInstallment.InstallmentResult.FinalRemainingDebt.Should().BeApproximately(435310.84M, 0.01m);

        firstInstallment.ConsolidatedResult.InitialRemainingDebt.Should().BeApproximately(489250.00M, 0.01m);
        firstInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(487810.84M, 0.01m);

        firstInstallment.ConsolidatedResult.Mip.Should().BeApproximately(132.09M, 0.01M);
        firstInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes.Should().BeApproximately(7583.95M, 0.01M);

        // Assert last month
        var lastInstallment = result.Installments.Last();

        lastInstallment.Month.Should().Be(144);
        lastInstallment.InstallmentResult.Interest.Should().BeApproximately(51.42M, 0.01M);
        lastInstallment.InstallmentResult.Amortization.Should().BeApproximately(5143.58M, 0.01m);
        lastInstallment.InstallmentResult.Installment.Should().BeApproximately(5195.01M, 0.01m);
        lastInstallment.InstallmentResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);

        lastInstallment.ConsolidatedResult.Mip.Should().BeApproximately(1.38M, 0.01M);
        lastInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes.Should().BeApproximately(5221.39M, 0.01M);
        lastInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);
    }

    [Fact]
    public void Calculate_ShouldReturnCorrectIntermediatePayments()
    {
        // Arrange
        var calculator = new SACLoanCalculator();

        var req = new CalculatorRequest()
        {
            LoanAmount = 436750.00M,
            LoanTermInMonths = 144,
            InterestRate = 12.68M,

            IntermediatePayments = new IntermediatePaymentsRequest(52500M, 4, IntermediatePaymentsType.Annual)
        };

        // Act
        var result = calculator.Calculate(req);

        // Assert
        result.Should().NotBeNull();
        result.IntermediatePaymentsAmount.Should().Be(52500M);

        // Asset intermediates
        result.Installments.Where(x => x.Month != 12 && x.Month != 24 && x.Month != 36 && x.Month != 48).All(x => Math.Round(x.ConsolidatedResult.IntermediatePayment, 2) == 0M).Should().BeTrue();

        // Asset first month
        var firstInstallment = result.Installments.First();

        firstInstallment.Month.Should().Be(1);
        firstInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(486217.01M, 0.01m);

        // Asset intermediate payments
        result.Installments.First(x => x.Month == 12).IntermediateResult.FinalRemainingDebt.Should().BeApproximately(39375.00M, 0.01m);
        result.Installments.First(x => x.Month == 12).ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(439729.17M, 0.01m);
        result.Installments.First(x => x.Month == 12).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(19782.00M, 0.01m);

        result.Installments.First(x => x.Month == 24).IntermediateResult.FinalRemainingDebt.Should().BeApproximately(26250M, 0.01m);
        result.Installments.First(x => x.Month == 24).ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(390208.33M, 0.01m);
        result.Installments.First(x => x.Month == 24).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(18117.75M, 0.01m);

        result.Installments.First(x => x.Month == 48).IntermediateResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);
        result.Installments.First(x => x.Month == 48).ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(291166.67M, 0.01m);
        result.Installments.First(x => x.Month == 48).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(14789.25M, 0.01m);

        // Assert last month
        var lastInstallment = result.Installments.Last();

        lastInstallment.Month.Should().Be(144);
        lastInstallment.IntermediateResult.Should().BeNull();

        lastInstallment.ConsolidatedResult.Mip.Should().Be(0);
        lastInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);
    }

    [Fact]
    public void Calculate_ShouldReturnCorrectCostAndTaxes()
    {
        // Arrange
        var calculator = new SACLoanCalculator();

        var req = new CalculatorRequest()
        {
            LoanAmount = 436750.00M,
            LoanTermInMonths = 144,
            InterestRate = 12.68M,

            MonthlyFees = new MonthlyFeesRequest(0.027M, 0M, 25M)
        };

        // Act
        var result = calculator.Calculate(req);

        // Assert
        result.Should().NotBeNull();
        result.FirstInstallment.Should().BeApproximately(7542.59M, 0.01M);
        result.LastInstallment.Should().BeApproximately(3089.13M, 0.01M);

        result.Installments.All(x => x.InstallmentResult.AdministrationFee == 25M).Should().BeTrue();
        result.Installments.All(x => x.InstallmentResult.Dfi == 0M).Should().BeTrue();

        // Asset first month
        var firstInstallment = result.Installments.First();

        firstInstallment.Month.Should().Be(1);

        firstInstallment.InstallmentResult.Mip.Should().BeApproximately(117.92M, 0.01M);
        firstInstallment.InstallmentResult.InstallmentWithCostAndTaxes.Should().BeApproximately(7542.59M, 0.01M);

        // Assert last month
        var lastInstallment = result.Installments.Last();

        lastInstallment.Month.Should().Be(144);

        lastInstallment.InstallmentResult.Mip.Should().BeApproximately(0.82M, 0.01M);
        lastInstallment.InstallmentResult.InstallmentWithCostAndTaxes.Should().BeApproximately(3089.13M, 0.01M);
    }

    [Fact]
    public void Calculate_MissingData_ShouldThrowExceptions()
    {
        // Arrange
        var calculator = new SACLoanCalculator();

        // Act 1
        Action comparison = () =>
        {
            var req = new CalculatorRequest()
            {
                LoanAmount = 0M,
                LoanTermInMonths = 144,
                InterestRate = 12.68M
            };

            var result = calculator.Calculate(req);
        };

        // Assert 1
        comparison.Should().Throw<BusinessException>();

        // Act 2
        Action comparison2 = () =>
        {
            var req = new CalculatorRequest()
            {
                LoanAmount = 10000M,
                LoanTermInMonths = 0,
                InterestRate = 12.68M
            };

            var result = calculator.Calculate(req);
        };

        // Assert 2
        comparison2.Should().Throw<BusinessException>();

        // Act 3
        Action comparison3 = () =>
        {
            var req = new CalculatorRequest()
            {
                LoanAmount = 10000M,
                LoanTermInMonths = 421,
                InterestRate = 12.68M
            };

            var result = calculator.Calculate(req);
        };

        // Assert 3
        comparison3.Should().Throw<BusinessException>().WithMessage("err_loan_term_should_be_less_or_equal_than_420");
    }
}
