# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

.NET 10 minimal-API backend for tracking a Brazilian stock/FII (fund) investment portfolio: assets, buy/sell transactions, dividends, and monthly patrimony (net worth) evolution. Domain text (entity names, validation messages, enum labels) is in Portuguese. Solution file: `InvestmentPortfolio.slnx`.

## Commands

```bash
# Build
dotnet build InvestmentPortfolio.slnx

# Run the API (from src/InvestmentPortfolio.Api) — serves Swagger UI at /swagger in Development
dotnet run --project src/InvestmentPortfolio.Api

# Run all tests
dotnet test src/InvestmentPortfolio.Tests/InvestmentPortfolio.Tests.csproj

# Run a single test (MSTest fully-qualified name filter)
dotnet test src/InvestmentPortfolio.Tests/InvestmentPortfolio.Tests.csproj --filter "FullyQualifiedName~AssetHandlerTests.Dado_RequestValido_Quando_CreateAsyncChamado_Entao_RetornaCode201"

# EF Core migrations (run from src/InvestmentPortfolio.Infra, referencing the API's startup project)
dotnet ef migrations add <Name> --project src/InvestmentPortfolio.Infra --startup-project src/InvestmentPortfolio.Api
dotnet ef database update --project src/InvestmentPortfolio.Infra --startup-project src/InvestmentPortfolio.Api

# Docker: compose file lives in the sibling ../zefinance-react repo (builds this repo as the `api`
# service and itself as `web`). Expects postgres reachable at host.docker.internal.
cd ../zefinance-react && docker-compose up --build
```

There is no lint step or CI workflow configured in this repo.

## Architecture

Three-project layered solution plus a test project, all targeting `net10.0` with nullable reference types enabled:

- **InvestmentPortfolio.Core** — domain layer, no dependency on EF Core or infrastructure. Contains entities, enums, request/response DTOs, `Handlers/I*Handler` interfaces, `Services/I*Service` interfaces, and static `Queries/*Queries` classes holding reusable `Expression<Func<T,bool>>` predicates.
- **InvestmentPortfolio.Infra** — EF Core `DbContext` (`InvestmentContext`), fluent `Mappings/` (`IEntityTypeConfiguration<T>`), migrations, and `Services/YahooFinanceMarketDataService` (implements `IMarketDataService`, calls Yahoo Finance's `query2.finance.yahoo.com`).
- **InvestmentPortfolio.Api** — ASP.NET Core minimal API host. `Endpoints/<Feature>/<Action>Endpoint.cs` are static classes with a `Map(WebApplication app)` method registering one route each; `Handlers/<Feature>Handler.cs` implement the Core interfaces and contain the actual logic (talk to `InvestmentContext` and `IMarketDataService` directly — no MediatR/CQRS library). `Program.cs` wires everything and calls each endpoint's `Map(app)`. `Extensions/BuilderExtension.cs` holds the `WebApplicationBuilder` DI/config extension methods (`AddDatabase`, `AddHandlers`, `AddMarketData`, `AddCors`, `AddSwagger`).
- **InvestmentPortfolio.Tests** — MSTest project referencing all three layers directly (implicit usings include `Microsoft.VisualStudio.TestTools.UnitTesting`). `Fakes/InvestmentContextFake.Create()` gives a fresh EF Core InMemory `InvestmentContext` per test; `Fakes/MarketDataServiceFake` and `Fakes/AssetPriceHistoryServiceFake` are hand-written fakes (no mocking library) passed straight into handler constructors — no DI container in tests. Test method names follow a Portuguese Given/When/Then pattern: `Dado_<contexto>_Quando_<acao>_Entao_<resultado>`.

### Request flow

`Endpoint.Map()` registers a route → binds a `Request` DTO from Core → calls the matching `I*Handler` method → handler runs EF Core queries/domain logic and returns `Response<T>` (or a paged variant) → endpoint maps `IsSuccess`/`Code` to the HTTP result.

### Conventions to follow

- **Response wrapper**: almost every handler method returns `Response<TData>` (`Data`, `Code`, `Message`, `IsSuccess` derived from `Code` being 2xx). List endpoints return a paged subtype (e.g. `AssetPagedResponse`) instead.
- **Entities**: all inherit `AuditableEntity` (`Id`, `CreatedAt`, `UpdatedAt`, `Touch()`). Mutation happens through instance methods (`Update`, `Delete`, `UpdateCurrentPrice`, ...), not public setters — properties are `private set`. Soft-delete via `IsDeleted`/`DeletedAt`; queries must filter `!a.IsDeleted` (see `*Queries.GetActive()` helpers).
- **Handlers**: wrap logic in try/catch and return `Response` with code 500 and a Portuguese message on unexpected failure — do not let exceptions bubble to the endpoint.
- **New endpoint checklist**: add a `Request` in `Core/Requests/<Feature>/`, a method on the `I<Feature>Handler` interface and its handler implementation, a static `Endpoints/<Feature>/<Action>Endpoint.cs` with `Map()`, and register that `Map(app)` call in `Program.cs` under the matching `// <Feature>` comment block.
- **Market data**: `IMarketDataService` (ticker price lookup, dividend events) is Yahoo-Finance-backed today; `IAssetPriceHistoryService` handles historical price backfill. Both are swappable via DI (registered in `BuilderExtension.AddMarketData`).