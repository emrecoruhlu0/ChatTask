using ChatTask.Shared.Enums;

namespace ChatTask.ChatService.Models;

public class Channel : Conversation
{
    public ChannelPurpose Purpose { get; set; }
    public bool AutoAddNewMembers { get; set; } = true;
    public bool IsPublic { get; set; } = true; // Public channel'lar herkes tarafından görülebilir
    public string Topic { get; set; } = string.Empty;
    
    public override bool CanUserJoin(Guid userId)
    {
        // Zaten üye mi?
        if (Members.Any(m => m.UserId == userId))
            return true;
            
        // Public channel ise herkes katılabilir
        return IsPublic;
    }
    
    public override string GetDisplayName()
    {
        return $"#{Name}";
    }
    
    public override ConversationType GetConversationType()
    {
        return ConversationType.Channel;
    }
}
