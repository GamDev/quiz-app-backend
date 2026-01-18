using com.QuizAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace com.QuizAppBackend.Data
{
    public class QuizAppDBContext : DbContext
    {
        public QuizAppDBContext(DbContextOptions<QuizAppDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}

