using ChatTask.Shared.Models;
using ChatTask.Shared.Models.Conversations;
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
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Conversation inheritance - Table Per Hierarchy (TPH)
        modelBuilder.Entity<Conversation>()
            .HasDiscriminator<ChatTask.Shared.Enums.ConversationType>("Type")
            .HasValue<Channel>(ChatTask.Shared.Enums.ConversationType.Channel)
            .HasValue<Group>(ChatTask.Shared.Enums.ConversationType.Group)
            .HasValue<DirectMessage>(ChatTask.Shared.Enums.ConversationType.DirectMessage)
            .HasValue<TaskGroup>(ChatTask.Shared.Enums.ConversationType.TaskGroup);

        // Conversation foreign key configuration
        modelBuilder.Entity<Conversation>()
            .HasOne<Workspace>()
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
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Members)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
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

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}