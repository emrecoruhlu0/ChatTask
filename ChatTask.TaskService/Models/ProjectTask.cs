using ChatTask.Shared.Enums;

namespace ChatTask.TaskService.Models;

public class ProjectTask // Task yerine ProjectTask kullan
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
    public ChatTask.Shared.Enums.TaskStatus Status { get; set; } = ChatTask.Shared.Enums.TaskStatus.Pending;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public bool IsPrivate { get; set; } = false;
    public Guid CreatedById { get; set; } // Task'ı oluşturan kişi
    
    // Çoklu atama desteği
    public Guid? TaskGroupId { get; set; }
    public List<TaskAssignment> Assignments { get; set; } = new();
}
