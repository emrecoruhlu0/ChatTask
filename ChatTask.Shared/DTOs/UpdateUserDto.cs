namespace ChatTask.Shared.DTOs;

public class UpdateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Status { get; set; } = "offline";
}
