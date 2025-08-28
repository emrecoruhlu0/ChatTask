using Microsoft.AspNetCore.Http;
namespace ChatTask.Shared.DTOs;

public class RegisterDto
{
    public string Name { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}