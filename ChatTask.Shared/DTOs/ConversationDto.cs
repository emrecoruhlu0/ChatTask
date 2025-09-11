using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class ConversationDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ConversationType Type { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public MessageDto? LastMessage { get; set; }
    public List<MemberDto> Members { get; set; } = new();
}

