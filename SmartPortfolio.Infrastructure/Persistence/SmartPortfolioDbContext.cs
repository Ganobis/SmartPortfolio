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
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetLot> AssetLots { get; set; }

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
                var transactionsNavigation = entity.Metadata.FindNavigation(nameof(Portfolio.Transactions));
                transactionsNavigation?.SetPropertyAccessMode(PropertyAccessMode.Field);

                entity.HasMany(e => e.Assets)
                      .WithOne()
                      .HasForeignKey(a => a.PortfolioId)
                      .OnDelete(DeleteBehavior.Cascade);
                var assetsNavigation = entity.Metadata.FindNavigation(nameof(Portfolio.Assets));
                assetsNavigation?.SetPropertyAccessMode(PropertyAccessMode.Field);
            });
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Amount).HasPrecision(18, 4);

                entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            });
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id).ValueGeneratedNever();

                entity.Property(a => a.Ticker).IsRequired().HasMaxLength(10);

                entity.Ignore(a => a.TotalQuantity);

                entity.HasMany(a => a.AssetLots)
                      .WithOne()
                      .HasForeignKey(a => a.AssetId)
                      .OnDelete(DeleteBehavior.Cascade);
                var lotsNavigation = entity.Metadata.FindNavigation(nameof(Asset.AssetLots));
                lotsNavigation?.SetPropertyAccessMode(PropertyAccessMode.Field);
            });

            modelBuilder.Entity<AssetLot>(entity => { 
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id).ValueGeneratedNever();
                entity.Property(al => al.Quantity).HasPrecision(18, 8).IsRequired();
                entity.Property(al => al.PricePerShare).HasPrecision(18, 4).IsRequired();
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
