using InvestmentPortfolio.Core.Enums;

namespace InvestmentPortfolio.Core.Entities;

/// <summary>Registra a última execução de cada rotina (uma linha por <see cref="EJobType"/>).</summary>
public class JobExecution
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public EJobType JobType { get; private set; }
    public DateTime ExecutedAt { get; private set; }
    public bool Success { get; private set; }
    public string? Message { get; private set; }

    private JobExecution() { }

    public JobExecution(EJobType jobType, bool success, string? message)
    {
        JobType = jobType;
        Register(success, message);
    }

    public void Register(bool success, string? message)
    {
        ExecutedAt = DateTime.Now;
        Success = success;
        Message = message;
    }
}
