using ChatTask.Shared.Enums;

namespace ChatTask.Shared.Models.Conversations;

public class TaskGroup : Conversation
{
    public List<ProjectTask> Tasks { get; set; } = new(); // Task yerine ProjectTask
    public ChatTask.Shared.Enums.TaskGroupStatus Status { get; set; } = ChatTask.Shared.Enums.TaskGroupStatus.Active;
    
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
        return ChatTask.Shared.Enums.ConversationType.TaskGroup;
    }
}
