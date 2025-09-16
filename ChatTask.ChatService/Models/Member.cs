using ChatTask.Shared.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatTask.ChatService.Models;

public class Member
{
    // Composite Key: UserId + ParentId + ParentType
    public Guid UserId { get; set; }
    public Guid ParentId { get; set; }
    public ParentType ParentType { get; set; }
    public MemberRole Role { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties (NotMapped - explicit FK yok)
    [NotMapped]
    public Workspace? Workspace { get; set; }
    [NotMapped]
    public Conversation? Conversation { get; set; }
    
    // Helper properties
    public bool IsWorkspaceMember => ParentType == ParentType.Workspace;
    public bool IsConversationMember => ParentType == ParentType.Conversation;
}
