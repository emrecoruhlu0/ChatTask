using ChatTask.ChatService.Models;
using ChatTask.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatTask.ChatService.Context;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    // Yeni model'lar
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<DirectMessage> DirectMessages { get; set; }
    public DbSet<TaskGroup> TaskGroups { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Conversation inheritance - Table Per Hierarchy (TPH)
        modelBuilder.Entity<Conversation>()
            .HasDiscriminator<ConversationType>("Type")
            .HasValue<Channel>(ConversationType.Channel)
            .HasValue<Group>(ConversationType.Group)
            .HasValue<DirectMessage>(ConversationType.DirectMessage)
            .HasValue<TaskGroup>(ConversationType.TaskGroup);

        // Conversation foreign key configuration - Explicit navigation property ile
        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.Workspace)
            .WithMany(w => w.Conversations)
            .HasForeignKey(c => c.WorkspaceId)
            .OnDelete(DeleteBehavior.NoAction);

        // Workspace configuration
        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Domain).HasMaxLength(50);
            entity.HasIndex(e => e.Domain).IsUnique();
        });

        // Conversation configuration
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => new { e.WorkspaceId, e.Name });
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(4000);
            entity.HasIndex(e => new { e.ConversationId, e.CreatedAt });
            
            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Member configuration
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.ParentId }).IsUnique();
            
            // User referansı yok - UserService'den DTO ile alınır
                
            entity.HasOne(e => e.Workspace)
                .WithMany(w => w.Members)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
                
            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Members)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
        });

    }
}