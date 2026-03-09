using Svend.LoanCalculator.Api.LoanCalculators;
using Svend.LoanCalculator.Api.Shared;

namespace Svend.LoanCalculator.Api.Features.Simulate;

public static class SimulateEndpoint
{
    private const string Description = """
        Calcula a tabela de financiamento completa (PRICE, SAC ou SACOC) dados os parâmetros do empréstimo.

        ## Tabelas disponíveis

        | Valor | Tabela |
        |-------|--------|
        | 1     | PRICE  |
        | 2     | SAC    |
        | 3     | SACOC  |

        ## Tipos de pagamento intermediário

        | Valor | Tipo      |
        |-------|-----------|
        | 1     | Anual     |
        | 2     | Semestral |

        ## Exemplo de request

        ```json
        {
          "table": 1,
          "loanAmount": 1500000,
          "loanTermInMonths": 144,
          "interestRate": 12.6,
          "inflationRate": 4.5,
          "intermediatePayments": {
            "amount": 300000,
            "term": 6,
            "type": 1
          },
          "monthlyFees": {
            "mipPercentage": 0.027,
            "dfiFee": 0,
            "administrationFee": 15
          }
        }
        ```

        ## Campos do request

        | Campo | Tipo | Obrigatório | Descrição |
        |-------|------|:-----------:|-----------|
        | `table` | int | Sim | Tipo da tabela de amortização (ver tabela acima) |
        | `loanAmount` | decimal | Sim | Valor total do financiamento em reais. Deve ser maior que zero |
        | `loanTermInMonths` | int | Sim | Prazo do financiamento em meses. Deve ser entre 1 e 420 |
        | `interestRate` | decimal | Sim | Taxa de juros anual (ex: 12.6 = 12,6% a.a.) |
        | `inflationRate` | decimal | Não | Taxa de correção monetária anual (ex: 4.5 = 4,5% a.a.) |

        ### intermediatePayments (opcional)

        | Campo | Tipo | Obrigatório | Descrição |
        |-------|------|:-----------:|-----------|
        | `amount` | decimal | Não | Valor das parcelas intermediárias em reais |
        | `term` | int | Não | Prazo em meses das parcelas intermediárias |
        | `type` | int | Não | Tipo de pagamento intermediário (ver tabela acima) |

        ### monthlyFees (opcional)

        | Campo | Tipo | Obrigatório | Descrição |
        |-------|------|:-----------:|-----------|
        | `mipPercentage` | decimal | Sim* | Percentual do MIP sobre o saldo devedor (ex: 0.027 = 0,0027%) |
        | `dfiFee` | decimal | Sim* | Valor fixo mensal do DFI em reais |
        | `administrationFee` | decimal | Sim* | Taxa de administração mensal em reais |

        > *Obrigatório quando `monthlyFees` é informado.
        """;

    public static void MapSimulateEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/simulate", Handle)
            .WithName("Simulate")
            .WithTags("Loan Simulation")
            .WithSummary("Simula financiamento imobiliário")
            .WithDescription(Description)
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
