using ChatTask.ChatService.Services;
using Microsoft.AspNetCore.SignalR;

namespace ChatTask.ChatService.Hubs;

public class ChatHub : Hub
{
    public static Dictionary<string, Guid> Users = new();
    private readonly IUserService _userService;

    public ChatHub(IUserService userService)
    {
        _userService = userService;
    }

    public async Task Connect(Guid userId)
    {
        try
        {
            // User'ýn var olduðunu kontrol et
            if (!await _userService.UserExistsAsync(userId))
            {
                await Clients.Caller.SendAsync("Error", "Geçersiz kullanýcý ID'si");
                return;
            }

            // Connection mapping ekle
            Users.Add(Context.ConnectionId, userId);

            // Diðer kullanýcýlara online durumunu bildir
            await Clients.All.SendAsync("UserStatusChanged", new
            {
                UserId = userId,
                Status = "online",
                Timestamp = DateTime.UtcNow
            });

            Console.WriteLine($"User {userId} connected with connection {Context.ConnectionId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connect Error: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Baðlantý sýrasýnda hata oluþtu");
        }
    }

    public async Task JoinRoom(string roomId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserJoinedRoom", Context.ConnectionId, roomId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JoinRoom Error: {ex.Message}");
        }
    }

    public async Task LeaveRoom(string roomId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserLeftRoom", Context.ConnectionId, roomId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LeaveRoom Error: {ex.Message}");
        }
    }

    public async Task SendTypingIndicator(Guid toUserId, bool isTyping)
    {
        try
        {
            var connectionId = Users.FirstOrDefault(p => p.Value == toUserId).Key;

            if (!string.IsNullOrEmpty(connectionId))
            {
                await Clients.Client(connectionId).SendAsync("TypingIndicator", new
                {
                    FromUserId = Users.GetValueOrDefault(Context.ConnectionId),
                    IsTyping = isTyping,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SendTypingIndicator Error: {ex.Message}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (Users.TryGetValue(Context.ConnectionId, out Guid userId))
            {
                Users.Remove(Context.ConnectionId);

                // Diðer kullanýcýlara offline durumunu bildir
                await Clients.All.SendAsync("UserStatusChanged", new
                {
                    UserId = userId,
                    Status = "offline",
                    Timestamp = DateTime.UtcNow
                });

                Console.WriteLine($"User {userId} disconnected from connection {Context.ConnectionId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OnDisconnectedAsync Error: {ex.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<object> GetOnlineUsers()
    {
        try
        {
            var onlineUserIds = Users.Values.Distinct().ToList();
            return new { OnlineUsers = onlineUserIds, Count = onlineUserIds.Count };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetOnlineUsers Error: {ex.Message}");
            return new { OnlineUsers = new List<Guid>(), Count = 0 };
        }
    }
}