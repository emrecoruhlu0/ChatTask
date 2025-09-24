using ChatTask.Shared.DTOs;

public interface IUserService
{
    Task<bool> UserExistsAsync(Guid userId);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(Guid userId);
}