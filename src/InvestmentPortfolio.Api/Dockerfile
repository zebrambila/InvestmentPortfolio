FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/InvestmentPortfolio.Api/InvestmentPortfolio.Api.csproj src/InvestmentPortfolio.Api/
COPY src/InvestmentPortfolio.Core/InvestmentPortfolio.Core.csproj src/InvestmentPortfolio.Core/
COPY src/InvestmentPortfolio.Infra/InvestmentPortfolio.Infra.csproj src/InvestmentPortfolio.Infra/
RUN dotnet restore src/InvestmentPortfolio.Api/InvestmentPortfolio.Api.csproj

COPY src/InvestmentPortfolio.Api/ src/InvestmentPortfolio.Api/
COPY src/InvestmentPortfolio.Core/ src/InvestmentPortfolio.Core/
COPY src/InvestmentPortfolio.Infra/ src/InvestmentPortfolio.Infra/
RUN dotnet publish src/InvestmentPortfolio.Api/InvestmentPortfolio.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "InvestmentPortfolio.Api.dll"]