using Microsoft.EntityFrameworkCore;
using QuizPlatform.Domain.Entities;
using System.Reflection.Emit;

namespace QuizPlatform.API.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

        // аналоги репозиториев для каждой сущности
        public DbSet<User> Users { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Puzzle> Puzzles { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<UserResult> UserResults { get; set; }

        // Настройка маппинга и связей -->
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация User -->
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
            });
            // Конфигурация User <--

            // Конфигурация Quiz -->
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasOne(q => q.CreatedBy)
                      .WithMany(u => u.CreatedQuizzes)
                      .HasForeignKey(q => q.CreatedById)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Конфигурация Quiz <--

            // Конфигурация Puzzle -->
            modelBuilder.Entity<Puzzle>(entity =>
            {
                entity.HasOne(p => p.Quiz)
                      .WithMany(q => q.Puzzles)
                      .HasForeignKey(p => p.QuizId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Конфигурация Puzzle <--

            // Конфигурация AnswerOption -->
            modelBuilder.Entity<AnswerOption>(entity =>
            {
                entity.HasOne(a => a.Puzzle)
                      .WithMany(p => p.AnswerOptions)
                      .HasForeignKey(a => a.PuzzleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Конфигурация AnswerOption <--

            // Конфигурация UserResult -->
            modelBuilder.Entity<UserResult>(entity =>
            {
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Results)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Quiz)
                      .WithMany(q => q.Results)
                      .HasForeignKey(r => r.QuizId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Конфигурация UserResult <--
        }
        // Настройка маппинга и связей <--
    }
}
