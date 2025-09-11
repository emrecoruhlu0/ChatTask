using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class UpdateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Enums.TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime DueDate { get; set; }
}