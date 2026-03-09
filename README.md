# Svend Loan Calculator API

API para simulação de financiamento imobiliário com suporte às tabelas **PRICE**, **SAC** e **SACOC**.

## Funcionalidades

- Cálculo de parcelas mensais com amortização detalhada
- Correção monetária (inflação) sobre saldo devedor
- Pagamentos intermediários (anuais ou semestrais)
- Taxas mensais configuráveis (MIP, DFI, taxa de administração)
- Cálculo de renda mínima necessária

## Tabelas de amortização

| Tabela | Descrição |
|--------|-----------|
| **PRICE** | Parcelas fixas — prestação constante ao longo do financiamento |
| **SAC** | Sistema de Amortização Constante — amortização fixa com parcelas decrescentes |
| **SACOC** | SAC com Operação de Crédito — parcelas decrescentes calculadas sobre saldo + juros |

## Pré-requisitos

- [Docker](https://docs.docker.com/get-docker/) e Docker Compose **ou**
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Como executar

### Com Docker (recomendado)

```bash
docker compose up --build
```

A API estará disponível em `http://localhost:5137`.

### Sem Docker

```bash
dotnet run --project src/Svend.LoanCalculator.Api
```

## Documentação da API

Com a aplicação rodando, acesse a documentação interativa (Scalar):

```
http://localhost:5137/scalar/v1
```

## Endpoint

### `POST /api/simulate`

Simula um financiamento e retorna a tabela completa de parcelas.

**Request body:**

```json
{
  "table": 1,
  "loanAmount": 500000,
  "loanTermInMonths": 360,
  "interestRate": 12.6,
  "inflationRate": 5.0,
  "intermediatePayments": {
    "amount": 10000,
    "term": 24,
    "type": 1
  },
  "monthlyFees": {
    "mipPercentage": 0.027,
    "dfiFee": 10.50,
    "administrationFee": 25.00
  }
}
```

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|:-----------:|-----------|
| `table` | int | Sim | Tabela de amortização: `1` = PRICE, `2` = SAC, `3` = SACOC |
| `loanAmount` | decimal | Sim | Valor do empréstimo (> 0) |
| `loanTermInMonths` | int | Sim | Prazo em meses (1–420) |
| `interestRate` | decimal | Sim | Taxa de juros anual (%) |
| `inflationRate` | decimal | Não | Taxa de correção monetária anual (%) |
| `intermediatePayments` | object | Não | Configuração de parcelas intermediárias |
| `intermediatePayments.amount` | decimal | — | Valor da parcela intermediária |
| `intermediatePayments.term` | int | — | Prazo em meses |
| `intermediatePayments.type` | int | — | `1` = Anual, `2` = Semestral |
| `monthlyFees` | object | Não | Taxas mensais |
| `monthlyFees.mipPercentage` | decimal | — | % do MIP sobre saldo devedor |
| `monthlyFees.dfiFee` | decimal | — | Valor fixo mensal do DFI (R$) |
| `monthlyFees.administrationFee` | decimal | — | Taxa de administração mensal (R$) |

**Resposta (200):**

```json
{
  "tableId": "PRICE",
  "firstInstallment": 5320.45,
  "lastInstallment": 5320.45,
  "minimumMonthlyIncome": 21281.80,
  "intermediatePaymentsAmount": 0,
  "interestRate": 12.6,
  "inflationRate": null,
  "installments": [
    {
      "month": 1,
      "installmentResult": {
        "initialRemainingDebt": 500000,
        "interest": 4975.50,
        "amortization": 344.95,
        "installment": 5320.45,
        "finalRemainingDebt": 499655.05
      }
    }
  ]
}
```

**Erros (400):** Retornados no formato `ProblemDetails` quando a validação falha.

## Testes

```bash
dotnet test
```

## Estrutura do projeto

```
├── src/
│   └── Svend.LoanCalculator.Api/
│       ├── Features/Simulate/       # Endpoint e request models
│       ├── LoanCalculators/         # Interfaces, factory e implementações
│       │   └── Impl/                # PRICE, SAC, SACOC
│       └── Shared/                  # Validação e exceções
├── test/
│   └── Svend.LoanCalculator.UnitTest/
├── docker-compose.yml
├── Dockerfile
└── Svend.LoanCalculator.sln
```

## Tecnologias

- .NET 10 / ASP.NET Core Minimal API
- [Scalar](https://github.com/ScalarHQ/scalar) — documentação interativa da API
- [MathAnalytics](https://www.nuget.org/packages/MathAnalytics) — cálculos financeiros
- xUnit + FluentAssertions — testes unitários
