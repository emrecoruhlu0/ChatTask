public interface IUserService
{
    Task<bool> UserExistsAsync(Guid userId);
    Task<List<UserDto>> GetAllUsersAsync();
}