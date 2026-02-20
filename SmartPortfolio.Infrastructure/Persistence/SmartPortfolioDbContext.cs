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
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(user =>
            {
                user.HasKey(u => u.Id);
                user.HasIndex(u => u.Email).IsUnique();
                user.Property(u => u.Name).IsRequired().HasMaxLength(100);
                user.Property(u => u.Email).IsRequired().HasMaxLength(255);
            });

            modelBuilder.Entity<Portfolio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();

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
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Amount).HasPrecision(18, 4);

                entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
