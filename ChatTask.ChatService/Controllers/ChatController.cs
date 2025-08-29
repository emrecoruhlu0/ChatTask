using ChatTask.ChatService.Context;
using ChatTask.ChatService.Hubs;
using ChatTask.ChatService.Models;
using ChatTask.ChatService.Services;
using ChatTask.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatTask.ChatService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
        try
        {
            // User Service'ten kullan�c�lar� al
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Kullan�c�lar al�n�rken hata olu�tu", Error = ex.Message });
        }
    }

    [HttpGet("{userId:guid}/{toUserId:guid}")]
    public async Task<IActionResult> GetChats(Guid userId, Guid toUserId, CancellationToken cancellationToken)
    {
        try
        {
            // User'lar�n var oldu�unu kontrol et
            if (!await _userService.UserExistsAsync(userId) || !await _userService.UserExistsAsync(toUserId))
            {
                return BadRequest(new { Message = "Ge�ersiz kullan�c� ID'si" });
            }

            var chats = await _context.Chats
                .Where(p => (p.UserId == userId && p.ToUserId == toUserId) ||
                           (p.ToUserId == userId && p.UserId == toUserId))
                .OrderBy(p => p.Date)
                .ToListAsync(cancellationToken);

            // ChatDto listesi olarak d�nd�r
            var chatDtos = chats.Select(c => new ChatDto
            {
                Id = c.Id,
                UserId = c.UserId,
                ToUserId = c.ToUserId,
                Message = c.Message,
                Date = c.Date
            }).ToList();

            return Ok(chatDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Mesajlar al�n�rken hata olu�tu", Error = ex.Message });
        }
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request, CancellationToken cancellationToken)
    {
        try
        {
            // User do�rulama
            if (!await _userService.UserExistsAsync(request.UserId) ||
                !await _userService.UserExistsAsync(request.ToUserId))
            {
                return BadRequest(new { Message = "Ge�ersiz kullan�c� ID'si" });
            }

            // Chat olu�tur
            Chat chat = new()
            {
                UserId = request.UserId,
                ToUserId = request.ToUserId,
                Message = request.Message,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AddAsync(chat, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // SignalR ile real-time g�nderim
            try
            {
                var connectionId = ChatHub.Users.FirstOrDefault(p => p.Value == chat.ToUserId).Key;

                if (!string.IsNullOrEmpty(connectionId))
                {
                    var chatDto = new ChatDto
                    {
                        Id = chat.Id,
                        UserId = chat.UserId,
                        ToUserId = chat.ToUserId,
                        Message = chat.Message,
                        Date = chat.Date
                    };

                    await _hubContext.Clients.Client(connectionId).SendAsync("Messages", chatDto, cancellationToken);
                }
            }
            catch (Exception signalREx)
            {
                // SignalR hatas� loglayabilirsin ama API response'u etkilemesin
                Console.WriteLine($"SignalR Error: {signalREx.Message}");
            }

            // ChatDto olarak d�nd�r
            var resultDto = new ChatDto
            {
                Id = chat.Id,
                UserId = chat.UserId,
                ToUserId = chat.ToUserId,
                Message = chat.Message,
                Date = chat.Date
            };

            return Ok(resultDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Mesaj g�nderilirken hata olu�tu", Error = ex.Message });
        }
    }

    [HttpGet("conversations/{userId:guid}")]
    public async Task<IActionResult> GetConversations(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            // Kullan�c�n�n t�m konu�malar�n� al (son mesajlarla birlikte)
            var conversations = await _context.Chats
                .Where(c => c.UserId == userId || c.ToUserId == userId)
                .GroupBy(c => c.UserId == userId ? c.ToUserId : c.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.Date).First(),
                    UnreadCount = g.Count(m => m.ToUserId == userId && !m.IsRead)
                })
                .ToListAsync(cancellationToken);

            return Ok(conversations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Konu�malar al�n�rken hata olu�tu", Error = ex.Message });
        }
    }

    [HttpPut("mark-read/{chatId:guid}")]
    public async Task<IActionResult> MarkAsRead(Guid chatId, CancellationToken cancellationToken)
    {
        try
        {
            var chat = await _context.Chats.FindAsync(chatId);

            if (chat == null)
            {
                return NotFound(new { Message = "Mesaj bulunamad�" });
            }

            chat.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Mesaj okundu olarak i�aretlendi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Mesaj g�ncellenirken hata olu�tu", Error = ex.Message });
        }
    }
}