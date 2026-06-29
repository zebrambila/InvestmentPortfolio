# Plano: Integração com APIs Externas para Cotações de Ativos

## Contexto

O projeto atualmente só suporta atualização manual de preços via `PATCH /v1/assets/{id}/price`. O objetivo é automatizar a busca de cotações de Ações e FIIs da B3 através de uma API externa, com:
- Uma abstração (`IMarketDataService`) para trocar o provedor facilmente no futuro
- Um endpoint manual (`POST /v1/assets/sync-prices`) para acionar a sincronização
- Um background job (`IHostedService`) para sincronização automática em intervalo configurável

---

## Decisões

| Decisão | Escolha |
|---|---|
| Abstração de provedor | `IMarketDataService` (interface no Core) |
| Acionamento | Endpoint manual + Background Job |
| Provedor padrão | A definir (Brapi recomendada) |

---

## Arquitetura

```
Core/
  Services/
    IMarketDataService.cs         ← interface do provedor externo
  Handlers/
    IAssetHandler.cs              ← adicionar SyncPricesAsync → Response<int>

Infra/
  Services/
    BrapiMarketDataService.cs     ← implementação concreta (Brapi como padrão inicial)

Api/
  Handlers/
    AssetHandler.cs               ← implementar SyncPricesAsync
  Endpoints/Assets/
    SyncAssetPricesEndpoint.cs    ← POST /v1/assets/sync-prices
  BackgroundJobs/
    PriceSyncJob.cs               ← IHostedService com timer configurável
  Extensions/
    BuilderExtension.cs           ← registrar IMarketDataService, PriceSyncJob, HttpClient
  appsettings.json                ← adicionar seção "MarketData"
```

---

## Passos de Implementação

### 1. Core — Interface do serviço de mercado

**`Core/Services/IMarketDataService.cs`**
```csharp
public interface IMarketDataService
{
    Task<decimal?> GetCurrentPriceAsync(string ticker, CancellationToken ct = default);
    Task<Dictionary<string, decimal?>> GetCurrentPricesAsync(IEnumerable<string> tickers, CancellationToken ct = default);
}
```

### 2. Core — Handler interface

Adicionar em `IAssetHandler.cs`:
```csharp
Task<Response<int>> SyncPricesAsync(CancellationToken ct = default);
// Retorna: quantos preços foram atualizados com sucesso
```

### 3. Infra — Implementação concreta (Brapi)

**`Infra/Services/BrapiMarketDataService.cs`**

- Injeta `HttpClient` (named client: `"MarketData"`) e `IConfiguration`
- `GetCurrentPricesAsync` faz batch: `GET https://brapi.dev/api/v2/stocks/quote?symbols=PETR4,VALE3&token={ApiKey}`
- Parseia o JSON de resposta e retorna `Dictionary<string, decimal?>`
- Token é opcional na Brapi (plano free sem token tem rate limit menor)

Estrutura de resposta Brapi:
```json
{
  "results": [
    { "symbol": "PETR4", "regularMarketPrice": 38.50 }
  ]
}
```

### 4. Api — Handler implementation

**`Api/Handlers/AssetHandler.cs`** — adicionar `SyncPricesAsync`:
1. Busca todos os ativos ativos (`IsDeleted == false`) do banco
2. Chama `IMarketDataService.GetCurrentPricesAsync(tickers)`
3. Para cada ativo com preço retornado, atualiza `CurrentPrice` e `UpdatedAt`
4. Salva em batch (`SaveChangesAsync`)
5. Retorna `Response<int>` com quantidade de ativos atualizados

### 5. Api — Endpoint manual

**`Api/Endpoints/Assets/SyncAssetPricesEndpoint.cs`**
- `POST /v1/assets/sync-prices`
- Chama `IAssetHandler.SyncPricesAsync()`
- Retorna 200 com número de ativos atualizados

### 6. Api — Background Job

**`Api/BackgroundJobs/PriceSyncJob.cs`**
```csharp
public class PriceSyncJob : BackgroundService
{
    // Injeta IServiceScopeFactory (para resolver IAssetHandler com scope)
    // Lê intervalo de appsettings: MarketData:SyncIntervalMinutes (padrão: 60)
    // Loop: await Task.Delay(interval, stoppingToken); → chama SyncPricesAsync
}
```

### 7. Configuração

**`appsettings.json`** — adicionar:
```json
"MarketData": {
  "Provider": "Brapi",
  "BaseUrl": "https://brapi.dev/api",
  "ApiKey": "",
  "SyncIntervalMinutes": 60
}
```

**`BuilderExtension.cs`** — adicionar `AddMarketData(builder)`:
```csharp
builder.Services.AddHttpClient("MarketData", client => {
    client.BaseAddress = new Uri(config["MarketData:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IMarketDataService, BrapiMarketDataService>();
builder.Services.AddHostedService<PriceSyncJob>();
```

---

## APIs Avaliadas

### Brapi (brapi.dev) — Recomendada
- Endpoint cotação: `GET /api/v2/stocks/quote?symbols=PETR4,VALE3`
- Auth: `Authorization: Bearer TOKEN` (opcional no free tier)
- Campos: `regularMarketPrice`, `regularMarketChange`, `marketCap`, `fiftyTwoWeekHigh/Low`, `currency`, `logoUrl`
- Extras: histórico OHLCV, dividendos, fundamentos (DRE, balanço, estatísticas), perfil
- Vantagem: API documentada, estável, nativa para B3

### Yahoo Finance (não-oficial)
- Endpoint: `GET https://query2.finance.yahoo.com/v8/finance/chart/{TICKER}.SA`
- Auth: não requer (sem chave)
- Tickers B3 usam sufixo `.SA` (ex: PETR4.SA, MXRF11.SA)
- Campos em `meta`: `regularMarketPrice`, `regularMarketDayHigh/Low`, `regularMarketVolume`, `fiftyTwoWeekHigh/Low`, `chartPreviousClose`, `currency`, `longName`
- Desvantagem: API não-oficial, sem garantia de estabilidade

---

## Verificação

1. `dotnet build` — sem erros
2. Rodar a API e chamar `POST /v1/assets/sync-prices` via Swagger
3. Verificar log de atualização no console
4. Aguardar o intervalo configurado e verificar que o job dispara automaticamente (log)
5. `dotnet test` — testes existentes devem continuar passando
