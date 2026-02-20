using SmartPortfolio.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    private Portfolio() {}

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
}