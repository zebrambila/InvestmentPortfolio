using InvestmentPortfolio.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestmentPortfolio.Infra.Data.Mappings;

public class JobExecutionMapping : IEntityTypeConfiguration<JobExecution>
{
    public void Configure(EntityTypeBuilder<JobExecution> builder)
    {
        builder.ToTable("JobExecutions");
        builder.HasKey(j => j.Id);

        builder.Property(j => j.JobType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(j => j.JobType)
            .IsUnique();

        builder.Property(j => j.ExecutedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(j => j.Success)
            .IsRequired();

        builder.Property(j => j.Message)
            .HasMaxLength(500);
    }
}
