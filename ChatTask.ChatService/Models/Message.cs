namespace ChatTask.ChatService.Models;

public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public string? ThreadId { get; set; }
    
    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
}
