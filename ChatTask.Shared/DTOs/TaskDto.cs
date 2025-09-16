using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Enums.TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public Guid? TaskGroupId { get; set; }
    public int AssignmentCount { get; set; }
    public List<TaskAssignmentDto> Assignments { get; set; } = new();
}