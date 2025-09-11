using ChatTask.Shared.Models;
using ChatTask.Shared.Models.Conversations;
using ChatTask.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatTask.TaskService.Context;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }

    // Task yönetimi için gerekli DbSet'ler
    public DbSet<ProjectTask> Tasks { get; set; }
    public DbSet<TaskAssignment> TaskAssignments { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);



        // Task configuration
        modelBuilder.Entity<ProjectTask>(entity => // Task yerine ProjectTask
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => new { e.Status, e.Priority });
            
            // TaskGroup foreign key kaldırıldı (şimdilik)
            // WorkspaceId property'si de kaldırıldı
        });

        // TaskGroup configuration - Conversation'dan inherit ettiği için key konfigürasyonu yok
        modelBuilder.Entity<TaskGroup>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name);
        });

        // TaskAssignment configuration
        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TaskId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            
            entity.HasOne(e => e.Task)
                .WithMany(t => t.Assignments)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User configuration (minimal for TaskService)
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}

