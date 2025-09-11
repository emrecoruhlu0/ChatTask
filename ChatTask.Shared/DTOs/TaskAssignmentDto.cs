using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class TaskAssignmentDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public AssignmentStatus Status { get; set; }
}