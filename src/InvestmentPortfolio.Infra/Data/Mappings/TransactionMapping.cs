using InvestmentPortfolio.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestmentPortfolio.Infra.Data.Mappings;

public class TransactionMapping : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);

        builder.HasOne(t => t.Asset)
            .WithMany()
            .HasForeignKey(t => t.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.Type)
            .IsRequired();

        builder.Property(t => t.Quantity)
            .HasColumnType("NUMERIC(18,4)");

        builder.Property(t => t.UnitPrice)
            .HasColumnType("NUMERIC(18,4)");

        builder.Property(t => t.TotalValue)
            .HasColumnType("NUMERIC(18,4)");

        builder.Property(t => t.DateBuyorSell)
            .HasColumnName("DateBuyorSell")
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(t => t.Custodian)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnType("timestamp without time zone");

        builder.Property(t => t.UpdatedAt)
            .HasColumnType("timestamp without time zone");

        builder.Property(t => t.DeletedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired(false);
    }
}
