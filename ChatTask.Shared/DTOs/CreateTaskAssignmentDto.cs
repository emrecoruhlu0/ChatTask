using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class CreateTaskAssignmentDto
{
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
}
