using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class ChangeRoleDto
{
    public Guid RequestedBy { get; set; }
    public MemberRole Role { get; set; }
}

