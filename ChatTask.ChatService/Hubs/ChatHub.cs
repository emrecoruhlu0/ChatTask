using ChatTask.ChatService.Services;
using ChatTask.Shared.DTOs;
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
            // User'�n var oldu�unu kontrol et
            if (!await _userService.UserExistsAsync(userId))
            {
                await Clients.Caller.SendAsync("Error", "Ge�ersiz kullan�c� ID'si");
                return;
            }

            // Connection mapping ekle
            Users.Add(Context.ConnectionId, userId);

            // Di�er kullan�c�lara online durumunu bildir
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
            await Clients.Caller.SendAsync("Error", "Ba�lant� s�ras�nda hata olu�tu");
        }
    }

    public async Task JoinConversation(Guid conversationId)
    {
        try
        {
            var userId = Users.GetValueOrDefault(Context.ConnectionId);
            
            // Yetki kontrolü burada yapılabilir
            // var canJoin = await CanJoinConversation(userId, conversationId);
            // if (!canJoin) return;
            
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
            await Clients.Group(conversationId.ToString()).SendAsync("UserJoinedConversation", new
            {
                UserId = userId,
                ConversationId = conversationId,
                ConnectionId = Context.ConnectionId,
                Timestamp = DateTime.UtcNow
            });
            
            Console.WriteLine($"User {userId} joined conversation {conversationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JoinConversation Error: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Conversation'a katılırken hata oluştu");
        }
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        try
        {
            var userId = Users.GetValueOrDefault(Context.ConnectionId);
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
            await Clients.Group(conversationId.ToString()).SendAsync("UserLeftConversation", new
            {
                UserId = userId,
                ConversationId = conversationId,
                ConnectionId = Context.ConnectionId,
                Timestamp = DateTime.UtcNow
            });
            
            Console.WriteLine($"User {userId} left conversation {conversationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LeaveConversation Error: {ex.Message}");
        }
    }

    // YENİ: Conversation'da typing indicator
    public async Task SendTypingIndicator(Guid conversationId, bool isTyping)
    {
        try
        {
            var userId = Users.GetValueOrDefault(Context.ConnectionId);
            
            await Clients.Group(conversationId.ToString()).SendAsync("TypingIndicator", new
            {
                ConversationId = conversationId,
                UserId = userId,
                IsTyping = isTyping,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SendTypingIndicator Error: {ex.Message}");
        }
    }

    // YENİ: Mesaj gönderme (real-time için)
    public async Task SendMessageToConversation(MessageDto message)
    {
        try
        {
            var userId = Users.GetValueOrDefault(Context.ConnectionId);
            
            // Güvenlik: Sadece kendi mesajlarını gönderebilir
            if (message.SenderId != userId)
            {
                await Clients.Caller.SendAsync("Error", "Sadece kendi mesajlarınızı gönderebilirsiniz");
                return;
            }
            
            // Conversation'daki tüm üyelere gönder (kendisi hariç)
            await Clients.GroupExcept(message.ConversationId.ToString(), Context.ConnectionId)
                .SendAsync("NewMessage", message);
                
            Console.WriteLine($"Message sent to conversation {message.ConversationId} by user {userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SendMessageToConversation Error: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Mesaj gönderilirken hata oluştu");
        }
    }

    // DEPRECATED: Eski metodlar uyumluluk için kalsın
    public async Task JoinRoom(string roomId)
    {
        if (Guid.TryParse(roomId, out Guid conversationId))
        {
            await JoinConversation(conversationId);
        }
    }

    public async Task LeaveRoom(string roomId)
    {
        if (Guid.TryParse(roomId, out Guid conversationId))
        {
            await LeaveConversation(conversationId);
        }
    }

    // DEPRECATED: Eski direct typing indicator
    public async Task SendTypingIndicatorToUser(Guid toUserId, bool isTyping)
    {
        try
        {
            var connectionId = Users.FirstOrDefault(p => p.Value == toUserId).Key;

            if (!string.IsNullOrEmpty(connectionId))
            {
                await Clients.Client(connectionId).SendAsync("TypingIndicator", new
                {
                    FromUserId = Users.GetValueOrDefault(Context.ConnectionId),
                    ToUserId = toUserId,
                    IsTyping = isTyping,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SendTypingIndicatorToUser Error: {ex.Message}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (Users.TryGetValue(Context.ConnectionId, out Guid userId))
            {
                Users.Remove(Context.ConnectionId);

                // Di�er kullan�c�lara offline durumunu bildir
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

    public async Task JoinGroup(string groupId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            Console.WriteLine($"Connection {Context.ConnectionId} joined group {groupId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JoinGroup Error: {ex.Message}");
        }
    }

    public async Task LeaveGroup(string groupId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
            Console.WriteLine($"Connection {Context.ConnectionId} left group {groupId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LeaveGroup Error: {ex.Message}");
        }
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