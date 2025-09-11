namespace ChatTask.Shared.DTOs;

public class SendMessageDto
{
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ThreadId { get; set; }
}