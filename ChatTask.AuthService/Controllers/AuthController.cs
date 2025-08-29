using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatTask.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChatTask.AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("UserService");

        var response = await client.PostAsJsonAsync("api/users/validate", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Unauthorized();
        }

        var user = await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        var token = GenerateJwt(user);
        return Ok(new TokenResponseDto
        {
            AccessToken = token.token,
            ExpiresAt = token.expires
        });
    }

    private (string token, DateTime expires) GenerateJwt(UserDto user)
    {
        string issuer = _configuration["Jwt:Issuer"] ?? "chat-task";
        string audience = _configuration["Jwt:Audience"] ?? "chat-task-audience";
        string key = _configuration["Jwt:Key"] ?? "dev-secret-key-change-me-please-1234567890";
        int expiresMinutes = _configuration.GetValue<int?>("Jwt:ExpiresMinutes") ?? 60;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, expires);
    }
}


