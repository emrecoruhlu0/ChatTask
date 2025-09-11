using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class MemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserDto User { get; set; } = null!;
    public MemberRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
    public Guid? WorkspaceId { get; set; }
    public Guid? ConversationId { get; set; }
    public string? WorkspaceName { get; set; }
    public string? ConversationName { get; set; }
    public EntityType? EntityType { get; set; }
}
