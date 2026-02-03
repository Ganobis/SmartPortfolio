using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.Domain.Entities;

namespace SmartPortfolio.Infrastructure.Persistence
{
    public class SmartPortfolioDbContext : DbContext
    {
        public SmartPortfolioDbContext(DbContextOptions<SmartPortfolioDbContext> options) : base(options)
        {

        }

        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Portfolio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.OwnsOne(e => e.Balance, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("BalanceAmount")
                        .HasPrecision(18, 4)
                        .IsRequired();
                    money.Property(m => m.Currency)
                        .HasColumnName("BalanceCurrency")
                        .HasMaxLength(3)
                        .IsRequired();
                });

                entity.HasMany(e => e.Transactions)
                      .WithOne()
                      .HasForeignKey(e => e.PortfolioId);
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
