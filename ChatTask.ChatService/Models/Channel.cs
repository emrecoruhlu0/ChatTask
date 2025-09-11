using ChatTask.Shared.Enums;

namespace ChatTask.ChatService.Models;

public class Channel : Conversation
{
    public ChannelPurpose Purpose { get; set; }
    public bool AutoAddNewMembers { get; set; } = true;
    public string Topic { get; set; } = string.Empty;
    
    public override bool CanUserJoin(Guid userId)
    {
        // Channel'a workspace üyesi herkes katılabilir (private değilse)
        return !IsPrivate || Members.Any(m => m.GetUserId() == userId);
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
