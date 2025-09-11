using ChatTask.Shared.Enums;

namespace ChatTask.ChatService.Models;

public class TaskGroup : Conversation
{
    // Task'lar TaskService'de yönetiliyor, burada sadece referans
    public int TaskCount { get; set; } = 0;
    public TaskGroupStatus Status { get; set; } = TaskGroupStatus.Active;
    
    public override bool CanUserJoin(Guid userId)
    {
        // Task group'a sadece atanan kişiler katılabilir
        return Members.Any(m => m.GetUserId() == userId);
    }
    
    public override string GetDisplayName()
    {
        return $"📋 {Name}";
    }
    
    public override ConversationType GetConversationType()
    {
        return ConversationType.TaskGroup;
    }
}
