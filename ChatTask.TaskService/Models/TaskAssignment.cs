using ChatTask.Shared.Enums;

namespace ChatTask.TaskService.Models;

public class TaskAssignment
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
    
    // Navigation properties
    public ProjectTask Task { get; set; } = null!; // Task yerine ProjectTask
}
