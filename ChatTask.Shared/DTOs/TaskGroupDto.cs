using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class TaskGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskGroupStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public int MemberCount { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public List<TaskDto> Tasks { get; set; } = new();
}

