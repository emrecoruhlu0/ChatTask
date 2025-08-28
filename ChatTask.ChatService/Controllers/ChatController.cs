using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("api/[controller]")]
public class ChatsController : ControllerBase
{
    private readonly ChatDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IUserService _userService;

    public ChatsController(ChatDbContext context, IHubContext<ChatHub> hubContext, IUserService userService)
    {
        _context = context;
        _hubContext = hubContext;
        _userService = userService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        // User Service'ten kullanýcýlarý al
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{userId}/{toUserId}")]
    public async Task<IActionResult> GetChats(Guid userId, Guid toUserId)
    {
        // Mevcut GetChats kodunuzu buraya taþýyýn
        var chats = await _context.Chats
            .Where(p => (p.UserId == userId && p.ToUserId == toUserId) ||
                       (p.ToUserId == userId && p.UserId == toUserId))
            .OrderBy(p => p.Date)
            .ToListAsync();

        return Ok(chats);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(SendMessageDto request)
    {
        // User doðrulama
        if (!await _userService.UserExistsAsync(request.UserId) ||
            !await _userService.UserExistsAsync(request.ToUserId))
        {
            return BadRequest("Invalid user ID");
        }

        Chat chat = new()
        {
            UserId = request.UserId,
            ToUserId = request.ToUserId,
            Message = request.Message,
            Date = DateTime.Now
        };

        await _context.AddAsync(chat);
        await _context.SaveChangesAsync();

        // SignalR ile gönder (mevcut kodunuz)
        try
        {
            var connectionId = ChatHub.Users.FirstOrDefault(p => p.Value == chat.ToUserId).Key;
            if (!string.IsNullOrEmpty(connectionId))
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("Messages", chat);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request
            Console.WriteLine($"SignalR Error: {ex.Message}");
        }

        return Ok(chat);
    }
}