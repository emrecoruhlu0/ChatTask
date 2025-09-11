using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class UpdateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public ChatTask.Shared.Enums.TaskStatus Status { get; set; }
    public ChatTask.Shared.Enums.TaskPriority Priority { get; set; }
}

