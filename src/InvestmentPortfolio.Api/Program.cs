using InvestmentPortfolio.Api.Endpoints.Assets;
using InvestmentPortfolio.Api.Endpoints.Dividends;
using InvestmentPortfolio.Api.Endpoints.Transactions;
using InvestmentPortfolio.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddDatabase();
builder.AddHandlers();
builder.AddMarketData();
builder.AddSwagger();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opts =>
    {
        opts.SwaggerEndpoint("/swagger/v1/swagger.json", "Investment Portfolio API v1");
        opts.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Assets
CreateAssetEndpoint.Map(app);
UpdateAssetEndpoint.Map(app);
UpdateAssetPriceEndpoint.Map(app);
SyncAssetPricesEndpoint.Map(app);
DeleteAssetEndpoint.Map(app);
GetAssetByIdEndpoint.Map(app);
GetAllAssetsEndpoint.Map(app);

// Transactions
CreateTransactionEndpoint.Map(app);
UpdateTransactionEndpoint.Map(app);
DeleteTransactionEndpoint.Map(app);
GetTransactionByIdEndpoint.Map(app);
GetAllTransactionsEndpoint.Map(app);

// Dividends
CreateDividendEndpoint.Map(app);
UpdateDividendEndpoint.Map(app);
DeleteDividendEndpoint.Map(app);
GetDividendByIdEndpoint.Map(app);
GetAllDividendsEndpoint.Map(app);

app.Run();
