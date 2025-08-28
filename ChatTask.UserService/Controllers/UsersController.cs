using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatTask.UserService.Context.UserContextDb;
using ChatTask.Shared.DTOs;
using ChatTask.Shared.Models;

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
	public async Task<IActionResult> Register([FromForm] RegisterDto request)
	{
		// Mevcut Register kodunuzu buraya taþýyýn
		bool isNameExists = await _context.Users.AnyAsync(p => p.Name == request.Name);

		if (isNameExists)
		{
			return BadRequest(new { Message = "Bu kullanýcý adý daha önce kullanýlmýþ" });
		}

		// Avatar upload logic
		string avatarFileName = Guid.NewGuid() + Path.GetExtension(request.File.FileName);
		string avatarPath = Path.Combine("wwwroot/avatar", avatarFileName);

		using (var stream = new FileStream(avatarPath, FileMode.Create))
		{
			await request.File.CopyToAsync(stream);
		}

		User user = new()
		{
			Name = request.Name,
			Avatar = $"/avatar/{avatarFileName}"
		};

		await _context.AddAsync(user);
		await _context.SaveChangesAsync();

		return Ok(user);
	}

	[HttpGet("login")]
	public async Task<IActionResult> Login(string name)
	{
		// Mevcut Login kodunuzu buraya taþýyýn
		User? user = await _context.Users.FirstOrDefaultAsync(p => p.Name == name);

		if (user is null)
		{
			return BadRequest(new { Message = "Kullanýcý bulunamadý" });
		}

		user.Status = "online";
		await _context.SaveChangesAsync();

		return Ok(user);
	}

	[HttpGet]
	public async Task<IActionResult> GetAllUsers()
	{
		// Mevcut GetUsers kodunuzu buraya taþýyýn
		List<User> users = await _context.Users.OrderBy(p => p.Name).ToListAsync();
		return Ok(users);
	}
}