using Microsoft.EntityFrameworkCore;
using SmartPortfolio.Domain.Entities;

namespace SmartPortfolio.Application.Common.Interfaces;

public interface ISmartPortfolioDbContext
{
    DbSet<User> Users { get; }
    DbSet<Portfolio> Portfolios { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Asset> Assets { get; }
    DbSet<AssetLot> AssetLots { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}