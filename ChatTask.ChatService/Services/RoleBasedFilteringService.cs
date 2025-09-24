using ChatTask.Shared.Enums;
using ChatTask.ChatService.Models;
using ChatTask.ChatService.Context;
using Microsoft.EntityFrameworkCore;

namespace ChatTask.ChatService.Services;

public class RoleBasedFilteringService
{
    private readonly ChatDbContext _context;

    public RoleBasedFilteringService(ChatDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Kullanıcının workspace'teki rolüne göre conversation'ları filtreler
    /// </summary>
    public async Task<List<Conversation>> GetConversationsByRole(Guid workspaceId, Guid userId)
    {
        // Kullanıcının workspace'teki rolünü al
        var userWorkspaceRole = await GetUserWorkspaceRole(workspaceId, userId);
        
        if (userWorkspaceRole == null)
            return new List<Conversation>(); // Workspace üyesi değil

        return userWorkspaceRole.Role switch
        {
            MemberRole.Owner => await GetConversationsForOwner(workspaceId, userId),
            MemberRole.Admin => await GetConversationsForAdmin(workspaceId, userId),
            MemberRole.Member => await GetConversationsForMember(workspaceId, userId),
            _ => await GetConversationsForMember(workspaceId, userId) // Default
        };
    }

    /// <summary>
    /// Kullanıcının workspace'teki rolünü getirir
    /// </summary>
    private async Task<Member?> GetUserWorkspaceRole(Guid workspaceId, Guid userId)
    {
        return await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == userId && 
                                    m.ParentId == workspaceId && 
                                    m.ParentType == ParentType.Workspace &&
                                    m.IsActive);
    }

    /// <summary>
    /// Owner: Tüm conversation'ları görebilir (üye olmasa bile)
    /// </summary>
    private async Task<List<Conversation>> GetConversationsForOwner(Guid workspaceId, Guid userId)
    {
        return await _context.Conversations
            .Where(c => c.WorkspaceId == workspaceId && !c.IsArchived)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Admin: Tüm conversation'ları görebilir (üye olmasa bile)
    /// </summary>
    private async Task<List<Conversation>> GetConversationsForAdmin(Guid workspaceId, Guid userId)
    {
        return await _context.Conversations
            .Where(c => c.WorkspaceId == workspaceId && !c.IsArchived)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Member: Üye olduğu conversation'ları + public channel'ları görebilir
    /// </summary>
    private async Task<List<Conversation>> GetConversationsForMember(Guid workspaceId, Guid userId)
    {
        // Kullanıcının üye olduğu conversation'ları al
        var userConversationIds = await _context.Members
            .Where(m => m.UserId == userId && 
                       m.ParentType == ParentType.Conversation &&
                       m.IsActive)
            .Select(m => m.ParentId)
            .ToListAsync();

        // Üye olduğu conversation'ları getir
        var memberConversations = await _context.Conversations
            .Where(c => c.WorkspaceId == workspaceId && 
                       userConversationIds.Contains(c.Id) &&
                       !c.IsArchived)
            .ToListAsync();

        // Public channel'ları da ekle (üye olmasa bile)
        var publicChannels = await _context.Conversations
            .OfType<Channel>()
            .Where(c => c.WorkspaceId == workspaceId && 
                       !userConversationIds.Contains(c.Id) &&
                       !c.IsArchived &&
                       c.IsPublic)
            .ToListAsync();

        var result = memberConversations.Concat(publicChannels.Cast<Conversation>())
            .DistinctBy(c => c.Id)
            .OrderBy(c => c.Name)
            .ToList();

        return result;
    }

    /// <summary>
    /// Kullanıcının conversation'a katılma yetkisi var mı?
    /// </summary>
    public async Task<bool> CanUserJoinConversation(Guid conversationId, Guid userId)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null) return false;

        var userWorkspaceRole = await GetUserWorkspaceRole(conversation.WorkspaceId, userId);
        if (userWorkspaceRole == null) return false;

        return userWorkspaceRole.Role switch
        {
            MemberRole.Owner => true, // Owner her yere katılabilir
            MemberRole.Admin => true, // Admin her yere katılabilir
            MemberRole.Member => await CanMemberJoinConversation(conversation, userId),
            _ => false
        };
    }

    private async Task<bool> CanMemberJoinConversation(Conversation conversation, Guid userId)
    {
        // Zaten üye mi?
        var isAlreadyMember = await _context.Members
            .AnyAsync(m => m.UserId == userId && 
                          m.ParentId == conversation.Id && 
                          m.ParentType == ParentType.Conversation);

        if (isAlreadyMember) return true;

        // Channel ise public mi kontrol et
        if (conversation is Channel channel)
        {
            return channel.IsPublic; // Channel'da IsPublic property'si olmalı
        }

        // Diğer conversation type'lar için false (sadece davet)
        return false;
    }
}
