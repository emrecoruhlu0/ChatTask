using Microsoft.AspNetCore.Http;

public class RegisterDto
{
    public string Name { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}