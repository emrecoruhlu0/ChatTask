using ChatTask.Shared.Enums;
using ChatTask.Shared.Helpers;

namespace ChatTask.ChatService.Models;

public class Member
{
    // ID harmanlama: UserId + ParentId + Role
    public Guid Id { get; set; }  // Harmanlanmış ID
    public MemberRole Role { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Foreign key properties (EF Core için)
    public Guid UserId { get; set; }
    public Guid ParentId { get; set; }
    
    // Navigation properties
    public Workspace? Workspace { get; set; }
    public Conversation? Conversation { get; set; }
    
    // ID'den bilgi çıkarma metodları
    public Guid GetUserId() => MemberIdHelper.ExtractUserId(Id);
    public Guid GetParentId() => MemberIdHelper.ExtractParentId(Id);
    
    // Helper properties - EntityType kontrolü kaldırıldı
    public bool IsWorkspaceMember => Workspace != null;
    public bool IsConversationMember => Conversation != null;
}
