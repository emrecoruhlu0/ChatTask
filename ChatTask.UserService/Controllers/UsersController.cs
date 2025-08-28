using ChatTask.Shared.DTOs;
using ChatTask.UserService.Context;
using ChatTask.UserService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> Register([FromForm] RegisterDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Name kontrol�
            bool isNameExists = await _context.Users.AnyAsync(p => p.Name == request.Name, cancellationToken);

            if (isNameExists)
            {
                return BadRequest(new { Message = "Bu kullan�c� ad� daha �nce kullan�lm��" });
            }

            // Avatar upload
            string avatar = "/avatar/default.png"; // Default avatar

            if (request.File != null)
            {
                // wwwroot/avatar klas�r�n� olu�tur
                var avatarDir = Path.Combine("wwwroot", "avatar");
                if (!Directory.Exists(avatarDir))
                {
                    Directory.CreateDirectory(avatarDir);
                }

                string avatarFileName = Guid.NewGuid() + Path.GetExtension(request.File.FileName);
                string avatarPath = Path.Combine(avatarDir, avatarFileName);

                using (var stream = new FileStream(avatarPath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream, cancellationToken);
                }

                avatar = $"/avatar/{avatarFileName}";
            }

            // User olu�tur
            User user = new()
            {
                Name = request.Name,
                Avatar = avatar,
                Status = "offline",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.AddAsync(user, cancellationToken);
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
            return StatusCode(500, new { Message = "Kay�t s�ras�nda hata olu�tu", Error = ex.Message });
        }
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