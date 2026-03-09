using Svend.LoanCalculator.Api.LoanCalculators;
using Svend.LoanCalculator.Api.Shared;

namespace Svend.LoanCalculator.Api.Features.Simulate;

public static class SimulateEndpoint
{
    public static void MapSimulateEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/simulate", Handle)
            .WithName("Simulate")
            .WithTags("Loan Simulation")
            .WithSummary("Simula financiamento imobiliário")
            .WithDescription("Calcula a tabela de financiamento completa (PRICE, SAC ou SACOC) dados os parâmetros do empréstimo.")
            .Produces<CalculatorResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static IResult Handle(SimulateRequest request)
    {
        try
        {
            var calculator = LoanCalculatorFactory.GetLoanCalculator(request.Table);

            var calculatorRequest = new CalculatorRequest
            {
                LoanAmount = request.LoanAmount,
                LoanTermInMonths = request.LoanTermInMonths,
                InterestRate = request.InterestRate,
                InflationRate = request.InflationRate,
                IntermediatePayments = request.IntermediatePayments != null
                    ? new IntermediatePaymentsRequest(
                        request.IntermediatePayments.Amount,
                        request.IntermediatePayments.Term,
                        request.IntermediatePayments.Type)
                    : null,
                MonthlyFees = request.MonthlyFees != null
                    ? new MonthlyFeesRequest(
                        request.MonthlyFees.MipPercentage,
                        request.MonthlyFees.DfiFee,
                        request.MonthlyFees.AdministrationFee)
                    : null
            };

            var result = calculator.Calculate(calculatorRequest);

            return Results.Ok(result);
        }
        catch (BusinessException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    private class ProblemDetails
    {
        public string Title { get; set; } = null!;
        public string Detail { get; set; } = null!;
        public int Status { get; set; }
    }
}
