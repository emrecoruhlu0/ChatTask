using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class UpdateTaskAssignmentDto
{
    public AssignmentStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
}
