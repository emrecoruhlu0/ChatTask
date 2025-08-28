using ChatTask.Shared.DTOs;

namespace ChatTask.ChatService.Services;
public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"http://localhost:5001/api/users");
            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
                return users?.Any(u => u.Id == userId) ?? false;
            }
        }
        catch
        {
            // Log error
        }
        return false;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"http://localhost:5001/api/users");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
            }
        }
        catch
        {
            // Log error
        }
        return new List<UserDto>();
    }
}