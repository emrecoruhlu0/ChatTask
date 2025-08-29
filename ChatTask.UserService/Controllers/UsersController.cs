using ChatTask.Shared.DTOs;
using ChatTask.UserService.Context;
using ChatTask.UserService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ChatTask.UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _context;

    public UsersController(UserDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Debug logging
            Console.WriteLine($"Register request received - Name: '{request.Name}', Password: '{request.Password}'");
            
            // Validation
            if (string.IsNullOrEmpty(request.Name))
            {
                Console.WriteLine("Name is null or empty");
                return BadRequest(new { Message = "Name is required" });
            }
            
            if (string.IsNullOrEmpty(request.Password))
            {
                Console.WriteLine("Password is null or empty");
                return BadRequest(new { Message = "Password is required" });
            }

            // Name kontrolü
            bool isNameExists = await _context.Users.AnyAsync(p => p.Name == request.Name, cancellationToken);

            if (isNameExists)
            {
                Console.WriteLine($"User name '{request.Name}' already exists");
                return BadRequest(new { Message = "Bu kullanıcı adı daha önce kullanılmış" });
            }

            // Default avatar (PNG yükleme zorunlu değil)
            string avatar = "/avatar/default.png";

            // Parola hashle
            CreatePasswordHash(request.Password, out string hash, out string salt);

            // User oluştur
            User user = new()
            {
                Name = request.Name,
                Avatar = avatar,
                Status = "offline",
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"User '{request.Name}' registered successfully with ID: {user.Id}");

            // UserDto olarak döndür
            var userDto = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Avatar = user.Avatar,
                Status = user.Status
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Register error: {ex.Message}");
            return StatusCode(500, new { Message = "Kayıt sırasında hata oluştu", Error = ex.Message });
        }
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == request.UserName, cancellationToken);
            if (user is null)
            {
                return Unauthorized(new { Message = "Geersiz kullanc veya parola" });
            }

            if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized(new { Message = "Geersiz kullanc veya parola" });
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Avatar = user.Avatar,
                Status = user.Status
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Dorulama srasnda hata olutu", Error = ex.Message });
        }
    }

    private static void CreatePasswordHash(string password, out string hash, out string salt)
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[16];
        rng.GetBytes(saltBytes);
        salt = Convert.ToBase64String(saltBytes);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
        hash = Convert.ToBase64String(pbkdf2.GetBytes(32));
    }

    private static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
        var computedHash = Convert.ToBase64String(pbkdf2.GetBytes(32));
        return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(storedHash), Convert.FromBase64String(computedHash));
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login([FromQuery] string name, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await _context.Users.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

            if (user is null)
            {
                return BadRequest(new { Message = "Kullan�c� bulunamad�" });
            }

            // Status g�ncelle
            user.Status = "online";
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            // UserDto olarak d�nd�r
            var userDto = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Avatar = user.Avatar,
                Status = user.Status
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Giri� s�ras�nda hata olu�tu", Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        try
        {
            List<User> users = await _context.Users
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);

            // UserDto listesi olarak d�nd�r
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Avatar = u.Avatar,
                Status = u.Status
            }).ToList();

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Kullan�c�lar al�n�rken hata olu�tu", Error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await _context.Users.FindAsync(id);

            if (user is null)
            {
                return NotFound(new { Message = "Kullan�c� bulunamad�" });
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Avatar = user.Avatar,
                Status = user.Status
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Kullan�c� al�n�rken hata olu�tu", Error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await _context.Users.FindAsync(id);

            if (user is null)
            {
                return NotFound(new { Message = "Kullan�c� bulunamad�" });
            }

            user.Status = status;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Status g�ncellendi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Status g�ncellenirken hata olu�tu", Error = ex.Message });
        }
    }
}