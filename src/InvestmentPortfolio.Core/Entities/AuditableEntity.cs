namespace InvestmentPortfolio.Core.Entities;

public abstract class AuditableEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    public DateTime UpdatedAt { get; private set; } = DateTime.Now;
    public void Touch() => UpdatedAt = DateTime.Now;
}