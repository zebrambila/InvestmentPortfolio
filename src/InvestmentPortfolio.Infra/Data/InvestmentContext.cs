using InvestmentPortfolio.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Infra.Data;

public class InvestmentContext(DbContextOptions<InvestmentContext> options) : DbContext(options)
{
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Dividend> Dividends { get; set; } = null!;
    public DbSet<AssetPriceHistory> AssetPriceHistories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvestmentContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.Touch();
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
