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


        // Workspace configuration
        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Domain).HasMaxLength(50);
            entity.HasIndex(e => e.Domain).IsUnique();
        });


        // Conversation configuration - TPH inheritance
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.WorkspaceId).IsRequired();
            entity.HasIndex(e => new { e.WorkspaceId, e.Name });
            
            // TPH inheritance configuration
            entity.HasDiscriminator(e => e.Type)
                .HasValue<Channel>(ConversationType.Channel)
                .HasValue<Group>(ConversationType.Group)
                .HasValue<DirectMessage>(ConversationType.DirectMessage)
                .HasValue<TaskGroup>(ConversationType.TaskGroup);
                
            // Conversation-Workspace relationship (TPH içinde)
            entity.HasOne(c => c.Workspace)
                .WithMany(w => w.Conversations)
                .HasForeignKey(c => c.WorkspaceId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Shadow property'leri ignore et
        modelBuilder.Entity<Channel>().Ignore("WorkspaceId1");
        modelBuilder.Entity<Group>().Ignore("WorkspaceId2");
        modelBuilder.Entity<DirectMessage>().Ignore("WorkspaceId2");
        modelBuilder.Entity<TaskGroup>().Ignore("WorkspaceId2");
        modelBuilder.Entity<Group>().Ignore("Group_WorkspaceId2");
        modelBuilder.Entity<TaskGroup>().Ignore("TaskGroup_WorkspaceId2");



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

        // Member configuration - Composite Key Only (No FK relationships)
        modelBuilder.Entity<Member>(entity =>
        {
            // Composite Key: UserId + ParentId + ParentType
            entity.HasKey(e => new { e.UserId, e.ParentId, e.ParentType });
            
            // Indexes for performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ParentId);
            entity.HasIndex(e => new { e.ParentId, e.ParentType });
            entity.HasIndex(e => new { e.UserId, e.ParentType });
            
            // Explicitly ignore all navigation properties to prevent FK inference
            entity.Ignore(e => e.Workspace);
            entity.Ignore(e => e.Conversation);
            
            // No foreign key relationships at all - pure composite key entity
            // This prevents EF Core from trying to infer relationships
        });

    }
}