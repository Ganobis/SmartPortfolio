namespace SmartPortfolio.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid PortfolioId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }

    private Transaction() { }

    public Transaction(Guid portfolioId, decimal amount, string currency, DateTime timestamp)
    {
        Id = Guid.NewGuid();
        PortfolioId = portfolioId;
        Amount = amount;
        Currency = currency;
        Timestamp = timestamp;
    }
}

