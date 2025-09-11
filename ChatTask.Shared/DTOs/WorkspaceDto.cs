namespace ChatTask.Shared.DTOs;

public class WorkspaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Domain { get; set; } = string.Empty;
    public Guid CreatedById { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public int ConversationCount { get; set; }
}
