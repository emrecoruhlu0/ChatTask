using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // String olarak değiştir
    public string Priority { get; set; } = "Medium"; // String olarak değiştir
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
    public bool IsPrivate { get; set; } = false;
    public string CreatedById { get; set; } = string.Empty; // String olarak değiştir
    public Guid? TaskGroupId { get; set; }
    public Guid? ChannelId { get; set; } // Channel'dan oluşturuluyorsa
    public List<string>? AssigneeIds { get; set; } // String list olarak değiştir
}