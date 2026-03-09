using FluentAssertions;
using Svend.LoanCalculator.Api.LoanCalculators;
using Svend.LoanCalculator.Api.LoanCalculators.Impl;
using Svend.LoanCalculator.Api.Shared;

namespace Svend.LoanCalculator.UnitTest.Features.Simulate;

public class SACOCLoanCalculatorTest
{
    private ILoanCalculator calculator;

    public SACOCLoanCalculatorTest()
    {
        calculator = new SACOCLoanCalculator();
    }

    [Fact]
    public void Calculate_InflationRateIsNull_ShouldReturnCorrectInstallments()
    {
        // Arrange
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
        result.TableId.Should().Be(Table.Sacoc.ToString());
        result.FirstInstallment.Should().BeApproximately(3220.40M, 0.01M);
        result.LastInstallment.Should().BeApproximately(12735.08M, 0.01M);
        result.MinimumMonthlyIncome.Should().BeApproximately(50941M, 0.01M);
        result.IntermediatePaymentsAmount.Should().Be(52500M);
        result.InterestRate.Should().Be(req.InterestRate);
        result.InflationRate.Should().Be(req.InflationRate);

        result.Installments.Should().HaveCount(req.LoanTermInMonths);
        result.Installments.All(x => Math.Round(x.InstallmentResult.Amortization, 2) == 0M).Should().BeTrue();
        result.Installments.All(x => x.InstallmentResult.AdministrationFee == 25M).Should().BeTrue();
        result.Installments.All(x => x.InstallmentResult.Dfi == 0M).Should().BeTrue();

        // Asset first month
        var firstInstallment = result.Installments.First();

        firstInstallment.Month.Should().Be(1);
        firstInstallment.InstallmentResult.Interest.Should().BeApproximately(4366.68M, 0.01M);
        firstInstallment.InstallmentResult.Installment.Should().BeApproximately(3063.31M, 0.01m);
        firstInstallment.InstallmentResult.FinalRemainingDebt.Should().BeApproximately(438053.37M, 0.01m);

        firstInstallment.ConsolidatedResult.InitialRemainingDebt.Should().BeApproximately(489250.00M, 0.01m);
        firstInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(490553.37M, 0.01m);

        firstInstallment.ConsolidatedResult.Mip.Should().BeApproximately(132.09M, 0.01M);
        firstInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes.Should().BeApproximately(3220.40M, 0.01M);

        // Assert last month
        var lastInstallment = result.Installments.Last();

        lastInstallment.Month.Should().Be(144);
        lastInstallment.InstallmentResult.Interest.Should().BeApproximately(125.78M, 0.01M);
        lastInstallment.InstallmentResult.Installment.Should().BeApproximately(12706.69M, 0.01m);
        lastInstallment.InstallmentResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);

        lastInstallment.ConsolidatedResult.Mip.Should().BeApproximately(3.39M, 0.01M);
        lastInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes.Should().BeApproximately(12735.08M, 0.01M);
        lastInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);
    }

    [Fact]
    public void Calculate_InflationRateSet_ShouldReturnCorrectInstallments()
    {
        // Arrange
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
        result.TableId.Should().Be(Table.Sacoc.ToString());
        result.FirstInstallment.Should().BeApproximately(3231.66M, 0.01M);
        result.LastInstallment.Should().BeApproximately(21579.78M, 0.01M);
        result.MinimumMonthlyIncome.Should().BeApproximately(86320M, 0.01M);
        result.IntermediatePaymentsAmount.Should().Be(52500M);

        result.Installments.Should().HaveCount(req.LoanTermInMonths);
        result.Installments.All(x => x.InstallmentResult.AdministrationFee == 25M).Should().BeTrue();
        result.Installments.All(x => x.InstallmentResult.Dfi == 0M).Should().BeTrue();

        // Asset intermediates
        result.Installments.First(x => x.Month == 12).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(15454.77M, 0.01M);
        result.Installments.First(x => x.Month == 24).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(18198.08M, 0.01M);
        result.Installments.First(x => x.Month == 36).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(21428.35M, 0.01M);
        result.Installments.First(x => x.Month == 48).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(25232.01M, 0.01M);

        // Asset first month
        var firstInstallment = result.Installments.First();

        firstInstallment.Month.Should().Be(1);
        firstInstallment.InstallmentResult.Interest.Should().BeApproximately(4382.73M, 0.01M);
        firstInstallment.InstallmentResult.Amortization.Should().BeApproximately(0M, 0.01m);
        firstInstallment.InstallmentResult.Installment.Should().BeApproximately(3074.56M, 0.01m);
        firstInstallment.InstallmentResult.FinalRemainingDebt.Should().BeApproximately(439663.13M, 0.01m);

        firstInstallment.ConsolidatedResult.InitialRemainingDebt.Should().BeApproximately(489250.00M, 0.01m);
        firstInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(492163.13M, 0.01m);

        firstInstallment.ConsolidatedResult.Mip.Should().BeApproximately(132.09M, 0.01M);
        firstInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes.Should().BeApproximately(3231.66M, 0.01M);

        // Assert last month
        var lastInstallment = result.Installments.Last();

        lastInstallment.Month.Should().Be(144);
        lastInstallment.InstallmentResult.Interest.Should().BeApproximately(213.31M, 0.01M);
        lastInstallment.InstallmentResult.Amortization.Should().BeApproximately(0M, 0.01m);
        lastInstallment.InstallmentResult.Installment.Should().BeApproximately(21549.04M, 0.01m);
        lastInstallment.InstallmentResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);

        lastInstallment.ConsolidatedResult.Mip.Should().BeApproximately(5.73M, 0.01M);
        lastInstallment.ConsolidatedResult.InstallmentWithCostAndTaxes.Should().BeApproximately(21579.78M, 0.01M);
        lastInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);
    }

    [Fact]
    public void Calculate_ShouldReturnCorrectIntermediatePayments()
    {
        // Arrange
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
        firstInstallment.ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(490553.37M, 0.01m);

        // Asset intermediate payments
        result.Installments.First(x => x.Month == 12).IntermediateResult.FinalRemainingDebt.Should().BeApproximately(44367.75M, 0.01m);
        result.Installments.First(x => x.Month == 12).ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(495486.82M, 0.01m);
        result.Installments.First(x => x.Month == 12).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(14789.2500M, 0.01m);

        result.Installments.First(x => x.Month == 24).IntermediateResult.FinalRemainingDebt.Should().BeApproximately(33329.05M, 0.01m);
        result.Installments.First(x => x.Month == 24).ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(495439.02M, 0.01m);
        result.Installments.First(x => x.Month == 24).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(16664.52M, 0.01m);

        result.Installments.First(x => x.Month == 48).IntermediateResult.FinalRemainingDebt.Should().BeApproximately(0M, 0.01m);
        result.Installments.First(x => x.Month == 48).ConsolidatedResult.FinalRemainingDebt.Should().BeApproximately(469384.78M, 0.01m);
        result.Installments.First(x => x.Month == 48).ConsolidatedResult.IntermediatePayment.Should().BeApproximately(21158.58M, 0.01m);

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
        result.FirstInstallment.Should().BeApproximately(3206.23M, 0.01M);
        result.LastInstallment.Should().BeApproximately(12735.08M, 0.01M);

        result.Installments.All(x => x.InstallmentResult.AdministrationFee == 25M).Should().BeTrue();
        result.Installments.All(x => x.InstallmentResult.Dfi == 0M).Should().BeTrue();

        // Asset first month
        var firstInstallment = result.Installments.First();

        firstInstallment.Month.Should().Be(1);

        firstInstallment.InstallmentResult.Mip.Should().BeApproximately(117.92M, 0.01M);
        firstInstallment.InstallmentResult.InstallmentWithCostAndTaxes.Should().BeApproximately(3206.23M, 0.01M);

        // Assert last month
        var lastInstallment = result.Installments.Last();

        lastInstallment.Month.Should().Be(144);

        lastInstallment.InstallmentResult.Mip.Should().BeApproximately(3.39M, 0.01M);
        lastInstallment.InstallmentResult.InstallmentWithCostAndTaxes.Should().BeApproximately(12735.08M, 0.01M);
    }

    [Fact]
    public void Calculate_MissingData_ShouldThrowExceptions()
    {
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
