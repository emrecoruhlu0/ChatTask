using ChatTask.Shared.DTOs;
using ChatTask.UserService.Context;
using ChatTask.UserService.Models;
using ChatTask.UserService.Services;
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
    private readonly UserMappingService _mappingService;
    private readonly IHttpClientFactory _httpClientFactory;

    public UsersController(UserDbContext context, UserMappingService mappingService, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _mappingService = mappingService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Debug logging
            Console.WriteLine($"Register request received - Name: '{request.Name}', Email: '{request.Email}', Password: '{request.Password}'");
            
            // Validation
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                Console.WriteLine("Name is null, empty or whitespace");
                return BadRequest(new { 
                    Message = "Kullanıcı adı gereklidir", 
                    Code = "NAME_REQUIRED",
                    Field = "name"
                });
            }
            
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                Console.WriteLine("Password is null, empty or whitespace");
                return BadRequest(new { 
                    Message = "Şifre gereklidir", 
                    Code = "PASSWORD_REQUIRED",
                    Field = "password"
                });
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                Console.WriteLine("Email is null, empty or whitespace");
                return BadRequest(new { 
                    Message = "E-posta adresi gereklidir", 
                    Code = "EMAIL_REQUIRED",
                    Field = "email"
                });
            }

            // Email format validation
            if (!IsValidEmail(request.Email))
            {
                Console.WriteLine($"Invalid email format: '{request.Email}'");
                return BadRequest(new { 
                    Message = "Geçersiz e-posta formatı", 
                    Code = "INVALID_EMAIL_FORMAT",
                    Field = "email"
                });
            }

            // Password strength validation
            if (request.Password.Length < 6)
            {
                Console.WriteLine("Password too short");
                return BadRequest(new { 
                    Message = "Şifre en az 6 karakter olmalıdır", 
                    Code = "PASSWORD_TOO_SHORT",
                    Field = "password"
                });
            }

            // Name kontrolü
            bool isNameExists = await _context.Users.AnyAsync(p => p.Name == request.Name, cancellationToken);

            if (isNameExists)
            {
                Console.WriteLine($"User name '{request.Name}' already exists");
                return Conflict(new { 
                    Message = "Bu kullanıcı adı daha önce kullanılmış", 
                    Code = "USERNAME_ALREADY_EXISTS",
                    Field = "name",
                    Suggestion = "Farklı bir kullanıcı adı deneyin"
                });
            }

            // Email kontrolü
            bool isEmailExists = await _context.Users.AnyAsync(p => p.Email == request.Email, cancellationToken);

            if (isEmailExists)
            {
                Console.WriteLine($"Email '{request.Email}' already exists");
                return Conflict(new { 
                    Message = "Bu e-posta adresi daha önce kullanılmış", 
                    Code = "EMAIL_ALREADY_EXISTS",
                    Field = "email",
                    Suggestion = "Farklı bir e-posta adresi kullanın"
                });
            }

            // Default avatar (PNG yükleme zorunlu değil)
            string avatar = "/avatar/default.png";

            // Parola hashle
            CreatePasswordHash(request.Password, out string hash, out string salt);

            // User oluştur
            var registerDto = new RegisterDto
            {
                Name = request.Name,
                Email = request.Email,
                Avatar = avatar,
                PasswordHash = hash,
                PasswordSalt = salt
            };
            
            User user = _mappingService.ToModel(registerDto);

            await _context.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"User '{request.Name}' registered successfully with ID: {user.Id}");

            // UserDto olarak döndür
            var userDto = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = request.Email,
                Avatar = user.Avatar,
                Status = user.Status
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Register error: {ex.Message}");
            return StatusCode(500, new { 
                Message = "Kayıt sırasında beklenmeyen bir hata oluştu", 
                Code = "REGISTRATION_ERROR",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                return BadRequest(new { 
                    Message = "Kullanıcı adı gereklidir", 
                    Code = "USERNAME_REQUIRED",
                    Field = "userName"
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { 
                    Message = "Şifre gereklidir", 
                    Code = "PASSWORD_REQUIRED",
                    Field = "password"
                });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == request.UserName, cancellationToken);
            if (user is null)
            {
                return Unauthorized(new { 
                    Message = "Kullanıcı adı veya şifre hatalı", 
                    Code = "INVALID_CREDENTIALS",
                    Field = "userName"
                });
            }

            if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized(new { 
                    Message = "Kullanıcı adı veya şifre hatalı", 
                    Code = "INVALID_CREDENTIALS",
                    Field = "password"
                });
            }

            // ChatService'e kullanıcı login bilgisini gönder
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var chatServiceUrl = "http://chattask-chatservice:5002"; // Docker service name
                var response = await httpClient.PostAsync(
                    $"{chatServiceUrl}/api/conversations/users/{user.Id}/login", 
                    null, 
                    cancellationToken);
                
                Console.WriteLine($"ChatService notification: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                // ChatService fail olsa bile login başarılı olsun
                Console.WriteLine($"ChatService notification failed: {ex.Message}");
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Avatar = user.Avatar,
                Status = user.Status
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Message = "Doğrulama sırasında beklenmeyen bir hata oluştu", 
                Code = "VALIDATION_ERROR",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
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

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
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
                Email = user.Email,
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
                Email = u.Email,
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
                Email = user.Email,
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

    [HttpGet("{id:guid}/workspaces")]
    public async Task<IActionResult> GetUserWorkspaces(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            // Geçici olarak dummy workspace döndür
            // Gerçek implementasyonda kullanıcının workspace'lerini database'den al
            var dummyWorkspaces = new[]
            {
                new { id = "d34fa2d4-5438-4f76-9ca3-ce095c2599a5", name = "Default Workspace" }
            };

            return Ok(dummyWorkspaces);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Kullanıcı workspace'leri alınırken hata oluştu", Error = ex.Message });
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