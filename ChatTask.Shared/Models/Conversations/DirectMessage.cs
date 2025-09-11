using ChatTask.Shared.Enums;

namespace ChatTask.Shared.Models.Conversations;

public class DirectMessage : Conversation
{
    public override bool CanUserJoin(Guid userId)
    {
        // DM'e sadece katılımcılar katılabilir ve maksimum 2 kişi
        return Members.Any(m => m.GetUserId() == userId) && Members.Count <= 2;
    }
    
    public override string GetDisplayName()
    {
        // İki kullanıcının adını birleştir
        if (Members.Count == 2)
        {
            var otherMember = Members.FirstOrDefault(m => m.GetUserId() != CreatedById);
            return otherMember?.User?.Name ?? "Direct Message";
        }
        return "Direct Message";
    }
    
    public override ConversationType GetConversationType()
    {
        return ChatTask.Shared.Enums.ConversationType.DirectMessage;
    }
}
