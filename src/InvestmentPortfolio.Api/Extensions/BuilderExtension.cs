using System.Text.Json.Serialization;
using InvestmentPortfolio.Api.Handlers;
using InvestmentPortfolio.Api.Services;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Services;
using InvestmentPortfolio.Infra.Data;
using InvestmentPortfolio.Infra.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

namespace InvestmentPortfolio.Api.Extensions;

public static class BuilderExtension
{
    public static void AddHandlers(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IAssetHandler, AssetHandler>();
        builder.Services.AddTransient<ITransactionHandler, TransactionHandler>();
        builder.Services.AddTransient<IDividendHandler, DividendHandler>();
        builder.Services.AddTransient<IPatrimonyHandler, PatrimonyHandler>();
    }

    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<InvestmentContext>(opts =>
            opts.UseNpgsql(connectionString));
    }

    public static void AddMarketData(this WebApplicationBuilder builder)
    {
        var baseUrl = builder.Configuration["MarketData:BaseUrl"]
            ?? "https://query2.finance.yahoo.com/";

        builder.Services.AddHttpClient("MarketData", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        builder.Services.AddScoped<IMarketDataService, YahooFinanceMarketDataService>();
        builder.Services.AddScoped<IAssetPriceHistoryService, AssetPriceHistoryService>();
    }

    public static void AddCors(this WebApplicationBuilder builder)
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173"];

        builder.Services.AddCors(opts =>
        {
            opts.AddPolicy("Frontend", policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });
    }

    public static void AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opts =>
        {
            opts.UseInlineDefinitionsForEnums();

            opts.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Investment Portfolio API",
                Version = "v1",
                Description = "API para gerenciamento de carteira de investimentos em Ações e FIIs."
            });
        });
    }
}
