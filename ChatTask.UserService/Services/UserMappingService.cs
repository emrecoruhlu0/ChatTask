using ChatTask.UserService.Models;
using ChatTask.Shared.DTOs;

namespace ChatTask.UserService.Services;

public class UserMappingService
{
    // Model → DTO
    public UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Avatar = user.Avatar,
            Status = user.Status,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    // DTO → Model
    public User ToModel(RegisterDto dto)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            Avatar = dto.Avatar ?? "/avatar/default.png",
            Status = "offline",
            PasswordHash = dto.PasswordHash,
            PasswordSalt = dto.PasswordSalt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // DTO → Model (Update)
    public void UpdateModel(User user, UpdateUserDto dto)
    {
        user.Name = dto.Name;
        user.Email = dto.Email;
        user.Avatar = dto.Avatar;
        user.Status = dto.Status;
        user.UpdatedAt = DateTime.UtcNow;
    }

    // Model → DTO (List)
    public List<UserDto> ToDtoList(IEnumerable<User> users)
    {
        return users.Select(ToDto).ToList();
    }
}
