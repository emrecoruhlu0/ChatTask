using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class MemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ParentId { get; set; }
    public MemberRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
}