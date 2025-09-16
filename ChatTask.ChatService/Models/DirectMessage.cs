using ChatTask.Shared.Enums;

namespace ChatTask.ChatService.Models;

public class DirectMessage : Conversation
{
    public override bool CanUserJoin(Guid userId)
    {
        // DM'e sadece katılımcılar katılabilir ve maksimum 2 kişi
        return Members.Any(m => m.UserId == userId) && Members.Count <= 2;
    }
    
    public override string GetDisplayName()
    {
        // İki kullanıcının adını birleştir
        if (Members.Count == 2)
        {
            var otherMember = Members.FirstOrDefault(m => m.UserId != CreatedById);
            return otherMember?.UserId.ToString() ?? "Direct Message";
        }
        return "Direct Message";
    }
    
    public override ConversationType GetConversationType()
    {
        return ConversationType.DirectMessage;
    }
}
