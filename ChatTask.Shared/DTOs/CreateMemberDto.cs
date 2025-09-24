using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class CreateMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string AddedById { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
}