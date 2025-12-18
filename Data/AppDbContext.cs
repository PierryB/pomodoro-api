using pomodoro_api.Models;
using Microsoft.EntityFrameworkCore;

namespace pomodoro_api.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<PomodoroSession> PomodoroSessions { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.CreatedAt);

        modelBuilder.Entity<PomodoroSession>()
            .HasOne(p => p.Task)
            .WithMany()
            .HasForeignKey(p => p.TaskId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
