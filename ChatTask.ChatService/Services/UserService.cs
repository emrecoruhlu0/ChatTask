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
            // Base URL zaten Program.cs'te tanımlandı, sadece endpoint ekle
            var response = await _httpClient.GetAsync("api/users");
            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
                return users?.Any(u => u.Id == userId) ?? false;
            }
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"UserService.UserExistsAsync Error: {ex.Message}");
        }
        return false;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        try
        {
            // Base URL zaten Program.cs'te tanımlandı, sadece endpoint ekle
            var response = await _httpClient.GetAsync("api/users");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
            }
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"UserService.GetAllUsersAsync Error: {ex.Message}");
        }
        return new List<UserDto>();
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.GetUserByIdAsync Error: {ex.Message}");
        }
        return null;
    }
}