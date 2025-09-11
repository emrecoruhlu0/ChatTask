using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
    public Guid? TaskGroupId { get; set; }
    public List<Guid>? AssigneeIds { get; set; }
}