using ChatTask.Shared.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatTask.ChatService.Models;

public abstract class Conversation
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; } // Navigation property
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedById { get; set; }
    public ConversationType Type { get; set; }
    
    // ORTAK ÖZELLİKLER - Tek liste
    public List<Member> Members { get; set; } = new();
    public List<Message> Messages { get; set; } = new();
    public bool IsArchived { get; set; }
    
    // ABSTRACT METODLAR - Alt sınıflar implement etmeli
    public abstract bool CanUserJoin(Guid userId);
    public abstract string GetDisplayName();
    public abstract ConversationType GetConversationType();
    
    // Conversation üyelerini filtrele
    [NotMapped]
    public IEnumerable<Member> ConversationMembers => 
        Members.Where(m => m.IsConversationMember);
}
