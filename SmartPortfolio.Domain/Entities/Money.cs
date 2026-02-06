using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPortfolio.Domain.Entities;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public static readonly HashSet<string> AllowedCurrencies = ["PLN", "USD", "EUR", "GBP", "CHF"];

    //For EF
    private Money() { }

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || !AllowedCurrencies.Contains(currency.ToUpper()))
        {
            throw new ArgumentException(
                $"Invalid currency: {currency}, Available {string.Join(", ", AllowedCurrencies)}");
        }

        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(string currency) => new(0, currency);

    #region Operators
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException("Cannot add different currencies!");
        }

        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException("Cannot subtract different currencies!");
        }
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal multiplier)
    {
        return new Money(a.Amount * multiplier, a.Currency);
    }

    public static bool operator >(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException("Cannot compare different currencies!");
        }
        return a.Amount > b.Amount;
    }

    public static bool operator <(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException("Cannot compare different currencies!");
        }
        return a.Amount < b.Amount;
    }

    public static bool operator >=(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException("Cannot compare different currencies!");
        }
        return a.Amount >= b.Amount;
    }

    public static bool operator <=(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException("Cannot compare different currencies!");
        }
        return a.Amount <= b.Amount;
    }
    #endregion
}