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

        public DbSet<User> user { get; set; }
        public DbSet<UserSessions> UserSessions { get; set; }
        public DbSet<RefreshTokens> RefreshTokens { get; set; }

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
            });

            // Configure UserSessions entity
            modelBuilder.Entity<UserSessions>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.EmailAddress)
                    .HasPrincipalKey(u => u.EmailAddress);
            });

            // Configure RefreshTokens entity
            modelBuilder.Entity<RefreshTokens>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId);
            });
        }
    }
}
