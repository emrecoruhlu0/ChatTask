using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class CreateMemberDto
{
    public Guid UserId { get; set; }
    public Guid ParentId { get; set; }
    public MemberRole Role { get; set; }
    public Guid AddedById { get; set; }
}