FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Svend.LoanCalculator.sln .
COPY src/Svend.LoanCalculator.Api/Svend.LoanCalculator.Api.csproj src/Svend.LoanCalculator.Api/
COPY test/Svend.LoanCalculator.UnitTest/Svend.LoanCalculator.UnitTest.csproj test/Svend.LoanCalculator.UnitTest/
RUN dotnet restore

COPY . .
RUN dotnet publish src/Svend.LoanCalculator.Api/Svend.LoanCalculator.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Svend.LoanCalculator.Api.dll"]
