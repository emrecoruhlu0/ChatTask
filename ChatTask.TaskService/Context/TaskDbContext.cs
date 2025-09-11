using ChatTask.TaskService.Models;
using ChatTask.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatTask.TaskService.Context;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }

    public DbSet<ProjectTask> Tasks { get; set; }
    public DbSet<TaskAssignment> TaskAssignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ProjectTask configuration
        modelBuilder.Entity<ProjectTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Priority).HasConversion<string>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.DueDate).HasDefaultValueSql("DATEADD(day, 7, GETUTCDATE())");
            entity.HasIndex(e => new { e.Status, e.Priority });
        });

        // TaskAssignment configuration
        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => new { e.TaskId, e.UserId }).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.Status });
            
            entity.HasOne(e => e.Task)
                .WithMany(t => t.Assignments)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}