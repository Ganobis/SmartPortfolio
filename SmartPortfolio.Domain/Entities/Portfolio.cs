using SmartPortfolio.Domain.ValueObjects;

namespace SmartPortfolio.Domain.Entities;

public class Portfolio
{
    #region privarte fields

    private readonly List<Transaction> _transactions = new();

    #endregion
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;
    public Money Balance { get; private set; } = null!;
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private readonly List<Asset> _assets = new();
    public IReadOnlyCollection<Asset> Assets => _assets.AsReadOnly();

    private Portfolio() { }

    public Portfolio(string name, Guid ownerId, string currency)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Portfolio name cannot be empty!");
        }

        Id = Guid.NewGuid();
        Name = name;
        OwnerId = ownerId;
        Balance = Money.Zero(currency);
    }

    public void Deposit(Money amount)
    {
        if (amount.Amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be positive!");
        }

        Balance += amount;
        _transactions.Add(new Transaction(Id, amount.Amount, amount.Currency, DateTime.UtcNow));
    }
    public void Withdraw(Money amount)
    {
        if (amount.Amount <= 0)
        {
            throw new ArgumentException("Withdraw amount must be positive!");
        }

        if (amount > Balance)
        {
            throw new InvalidOperationException($"Insufficient funds. Current balance: {Balance.Amount}, requested: {amount.Amount}");
        }

        Balance -= amount;
        _transactions.Add(new Transaction(Id, -amount.Amount, amount.Currency, DateTime.UtcNow));
    }
    public void BuyAsset(string ticker, decimal quantity, decimal pricePerShare, DateTime? transactionDate = null)
    {
        var totalCost = quantity * pricePerShare;
        if (Balance.Amount <= totalCost)
        {
            throw new InvalidOperationException($"Insufficient funds. You need {totalCost} {Balance.Currency} but have only {Balance.Amount}.");
        }
        Withdraw(new Money(totalCost, Balance.Currency));
        var existingAsset = _assets.FirstOrDefault(a => a.Ticker == ticker.ToUpper());
        if (existingAsset != null)
        {
            existingAsset.AddLot(quantity, pricePerShare, transactionDate);
        }
        else
        {
            var newAsset = new Asset(Id, ticker);
            newAsset.AddLot(quantity, pricePerShare, transactionDate);
            _assets.Add(newAsset);
        }
    }

    public void SellAsset(string ticker, decimal quantity, decimal sellPricePerShare)
    {
        var existingAsset = _assets.FirstOrDefault(a => a.Ticker == ticker.ToUpper());

        if (existingAsset == null || existingAsset.TotalQuantity < quantity)
        {
            throw new InvalidOperationException($"You don't own enough shares of {ticker}.");
        }

        existingAsset.RemoveQuantity(quantity);

        if (existingAsset.TotalQuantity == 0)
        {
            _assets.Remove(existingAsset);
        }

        var totalRevenue = quantity * sellPricePerShare;
        Deposit(new Money(totalRevenue, Balance.Currency));
    }
}