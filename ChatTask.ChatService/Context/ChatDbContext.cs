using ChatTask.ChatService.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatTask.ChatService.Context;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    public DbSet<Chat> Chats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.ToUserId).IsRequired();
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            // Indexler
            entity.HasIndex(e => new { e.UserId, e.ToUserId });
            entity.HasIndex(e => e.Date);
        });
    }
}