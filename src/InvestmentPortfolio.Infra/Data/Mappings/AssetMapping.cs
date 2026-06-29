using InvestmentPortfolio.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestmentPortfolio.Infra.Data.Mappings;

public class AssetMapping : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Ticker)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(a => a.Ticker)
            .IsUnique();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Type)
            .IsRequired();

        builder.Property(a => a.CurrentPrice)
            .HasColumnType("NUMERIC(18,4)");

        builder.Property(a => a.CreatedAt)
            .HasColumnType("timestamp without time zone");

        builder.Property(a => a.UpdatedAt)
            .HasColumnType("timestamp without time zone");

        builder.Property(a => a.DeletedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired(false);
    }
}
