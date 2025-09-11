using ChatTask.Shared.Enums;
using ChatTask.Shared.Models.Conversations;

namespace ChatTask.Shared.Models;

public class ProjectTask // Task yerine ProjectTask kullan
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
    public ChatTask.Shared.Enums.TaskStatus Status { get; set; } = ChatTask.Shared.Enums.TaskStatus.Pending;
    public ChatTask.Shared.Enums.TaskPriority Priority { get; set; } = ChatTask.Shared.Enums.TaskPriority.Medium;
    
    // Çoklu atama desteği
    public Guid? TaskGroupId { get; set; }
    public List<TaskAssignment> Assignments { get; set; } = new();
    
    // Navigation properties
    public TaskGroup? TaskGroup { get; set; }
}
