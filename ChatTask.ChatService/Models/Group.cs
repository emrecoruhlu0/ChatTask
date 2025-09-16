using ChatTask.Shared.Enums;

namespace ChatTask.ChatService.Models;

public class Group : Conversation
{
    public GroupPurpose Purpose { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    public override bool CanUserJoin(Guid userId)
    {
        // Group'a sadece davet ile katılım
        return Members.Any(m => m.UserId == userId);
    }
    
    public override string GetDisplayName()
    {
        return Name;
    }
    
    public override ConversationType GetConversationType()
    {
        return ConversationType.Group;
    }
}
