using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public static Dictionary<string, Guid> Users = new();

    // Mevcut hub kodlarýnýzý buraya taþýyýn
    public async Task Connect(Guid userId)
    {
        Users.Add(Context.ConnectionId, userId);
        await Clients.All.SendAsync("Users", new { userId, status = "online" });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Users.TryGetValue(Context.ConnectionId, out Guid userId))
        {
            Users.Remove(Context.ConnectionId);
            await Clients.All.SendAsync("Users", new { userId, status = "offline" });
        }
        await base.OnDisconnectedAsync(exception);
    }
}