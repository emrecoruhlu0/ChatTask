using ChatTask.Shared.Enums;
using ChatTask.Shared.Models.Conversations;

namespace ChatTask.Shared.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
    public ChatTask.Shared.Enums.TaskStatus Status { get; set; } = ChatTask.Shared.Enums.TaskStatus.Pending;
    public ChatTask.Shared.Enums.TaskPriority Priority { get; set; } = ChatTask.Shared.Enums.TaskPriority.Medium;
    public Guid? TaskGroupId { get; set; }
    public List<Guid>? AssigneeIds { get; set; }
}

