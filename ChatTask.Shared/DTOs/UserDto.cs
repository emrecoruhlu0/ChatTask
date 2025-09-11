namespace ChatTask.Shared.DTOs;

public class UserDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Avatar { get; set; } = string.Empty;
	public string Status { get; set; } = "offline";
	public bool IsActive { get; set; } = true;
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
}