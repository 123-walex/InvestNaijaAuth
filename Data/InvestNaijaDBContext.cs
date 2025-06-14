using System;
using InvestNaijaAuth.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvestNaijaAuth.Data
{
    public class InvestNaijaDBContext : DbContext
    {
        public InvestNaijaDBContext(DbContextOptions<InvestNaijaDBContext> options) : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<UserSessions> UserSessions { get; set; }
        public DbSet<RefreshTokens> RefreshTokens { get; set; }
        public DbSet<Wallet> Wallet { get; set; } 
        public DbSet<WalletTransaction> WalletTransaction { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmailAddress).IsRequired();
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.HashedPassword).IsRequired();
                entity.HasIndex(e => e.EmailAddress).IsUnique();
            });

            // Configure UserSessions entity
            modelBuilder.Entity<UserSessions>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Sessions)
                    .HasForeignKey(e => e.EmailAddress)
                    .HasPrincipalKey(u => u.EmailAddress)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure RefreshTokens entity
            modelBuilder.Entity<RefreshTokens>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Wallet>()
                .HasMany(w => w.Transactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId);

            modelBuilder.Entity<WalletTransaction>()
                .Property(w => w.Type)
                .HasConversion<string>();

            // Configure Stock entity
            modelBuilder.Entity<Stock>(entity =>
            {
                entity.HasKey(e => e.StockId);
                entity.Property(e => e.Symbol).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.CurrentPrice).HasPrecision(18, 2);
            });

            // Configure Portfolio entity
            modelBuilder.Entity<Portfolio>(entity =>
            {
                entity.HasKey(e => e.PortfolioId);
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Stock>()
                    .WithMany()
                    .HasForeignKey(e => e.StockId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.AveragePrice).HasPrecision(18, 2);
            });

            // Configure StockTransaction entity
            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId);
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Stock>()
                    .WithMany()
                    .HasForeignKey(e => e.StockId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.Price).HasPrecision(18, 2);
            });
        }
    }
}
