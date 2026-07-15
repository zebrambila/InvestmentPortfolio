using InvestmentPortfolio.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestmentPortfolio.Infra.Data.Mappings;

public class DividendMapping : IEntityTypeConfiguration<Dividend>
{
    public void Configure(EntityTypeBuilder<Dividend> builder)
    {
        builder.ToTable("Dividends");
        builder.HasKey(d => d.Id);

        builder.HasOne(d => d.Asset)
            .WithMany()
            .HasForeignKey(d => d.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(d => d.ReceiptDate)
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(d => d.TotalFinancial)
            .HasColumnType("NUMERIC(18,4)");

        builder.Property(d => d.Custodian)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .HasColumnType("timestamp without time zone");

        builder.Property(d => d.UpdatedAt)
            .HasColumnType("timestamp without time zone");

        builder.Property(d => d.DeletedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired(false);
    }
}
