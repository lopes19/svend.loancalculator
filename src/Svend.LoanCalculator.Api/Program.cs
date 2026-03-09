using Svend.LoanCalculator.Api.Features.Simulate;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapSimulateEndpoint();

app.Run();
