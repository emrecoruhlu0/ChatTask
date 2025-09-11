using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class CreateMemberDto
{
    public Guid UserId { get; set; }
    public Guid ParentId { get; set; }  // WorkspaceId veya ConversationId
    public MemberRole Role { get; set; } = MemberRole.Member;
    public bool IsActive { get; set; } = true;
    public Guid? AddedById { get; set; } // Workspace üye ekleme için
}
