
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
        public DbSet<RefreshTokens> RefreshTokens { get; set; } = null!;
    }
}
