using InvestmentPortfolio.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestmentPortfolio.Infra.Data.Mappings;

public class AssetPriceHistoryMapping : IEntityTypeConfiguration<AssetPriceHistory>
{
    public void Configure(EntityTypeBuilder<AssetPriceHistory> builder)
    {
        builder.ToTable("AssetPriceHistories");
        builder.HasKey(h => h.Id);

        builder.HasOne(h => h.Asset)
            .WithMany()
            .HasForeignKey(h => h.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(h => h.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.HasIndex(h => new { h.AssetId, h.Date })
            .IsUnique();

        builder.Property(h => h.ClosePrice)
            .HasColumnType("NUMERIC(18,4)")
            .IsRequired();

        builder.Property(h => h.CreatedAt)
            .HasColumnType("timestamp without time zone");
    }
}
