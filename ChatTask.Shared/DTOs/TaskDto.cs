using ChatTask.Shared.Enums;
using ChatTask.Shared.Models.Conversations;

namespace ChatTask.Shared.DTOs;

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public ChatTask.Shared.Enums.TaskStatus Status { get; set; }
    public ChatTask.Shared.Enums.TaskPriority Priority { get; set; }
    public Guid? TaskGroupId { get; set; }
    public string? TaskGroupName { get; set; }
    public List<TaskAssignmentDto> Assignments { get; set; } = new();
}

