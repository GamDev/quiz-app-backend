
using Microsoft.EntityFrameworkCore;
using QuizApp.Backend.Quizzes.Models;
using QuizApp.Backend.Tokens;
using QuizApp.Backend.Users;

namespace QuizApp.Backend.Data
{
    /// <summary>
    /// Represents the Entity Framework Core database context
    /// for the Quiz application.
    /// </summary>
    public class QuizAppDBContext : DbContext
    {
        public QuizAppDBContext(DbContextOptions<QuizAppDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                        .HasIndex(u => u.Email)
                        .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                        .HasIndex(refreshToken => refreshToken.TokenHash)
                        .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                        .HasOne(refreshToken => refreshToken.User)
                       .WithMany(user => user.RefreshTokens)
                        .HasForeignKey(refreshToken => refreshToken.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quiz>()
                        .HasOne(q => q.CreatedByUser)
                        .WithMany()
                        .HasForeignKey(q => q.CreatedBy)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Quiz>()
                        .HasMany(quiz => quiz.Questions)
                        .WithOne(question => question.Quiz)
                        .HasForeignKey(question => question.QuizId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                        .HasMany(question => question.Options)
                        .WithOne(option => option.Question)
                        .HasForeignKey(option => option.QuestionId)
                        .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

