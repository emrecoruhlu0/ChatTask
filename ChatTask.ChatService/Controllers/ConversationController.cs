using ChatTask.ChatService.Context;
using ChatTask.ChatService.Hubs;
using ChatTask.ChatService.Services;
using ChatTask.ChatService.Models;
using ChatTask.Shared.DTOs;
using ChatTask.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatTask.ChatService.Controllers;

[ApiController]
[Route("api/conversations")]
// [Authorize] - GeÃ§ici olarak kaldÄ±rÄ±ldÄ±
public class ConversationController : ControllerBase
{
    private readonly ChatDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IUserService _userService;
    private readonly ChatMappingService _mappingService;
    
    // EntityType'a uygun ID oluşturma helper metodu
    private static Guid CreateEntityId(EntityType entityType)
    {
        var bytes = new byte[16];
        bytes[0] = (byte)entityType; // İlk byte = EntityType
        var random = new Random();
        // NextBytes sadece 2 parametre alır: (byte[], int)
        random.NextBytes(bytes.AsSpan(1, 15)); // Geri kalan 15 byte random
        return new Guid(bytes);
    }

    public ConversationController(ChatDbContext context, IHubContext<ChatHub> hubContext, IUserService userService, ChatMappingService mappingService)
    {
        _context = context;
        _hubContext = hubContext;
        _userService = userService;
        _mappingService = mappingService;
    }





    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "KullanÄ±cÄ±lar alÄ±nÄ±rken hata oluÅŸtu", Error = ex.Message });
        }
    }
    // YENİ: Workspace oluştur
    [HttpPost("workspaces")]
    public async Task<IActionResult> CreateWorkspace([FromBody] CreateWorkspaceDto dto)
    {
        try
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { 
                    Message = "Workspace adı boş olamaz", 
                    Code = "INVALID_WORKSPACE_NAME",
                    Field = "name",
                    Suggestion = "Geçerli bir workspace adı girin"
                });
            }

            if (dto.CreatedById == Guid.Empty)
            {
                return BadRequest(new { 
                    Message = "Geçerli bir kullanıcı ID'si gerekli", 
                    Code = "INVALID_USER_ID",
                    Field = "createdById",
                    Suggestion = "Geçerli bir kullanıcı ID'si girin"
                });
            }

            // User'ın var olup olmadığını kontrol et (UserService'ten)
            var userExists = await _userService.UserExistsAsync(dto.CreatedById);
            if (!userExists)
            {
                return BadRequest(new { 
                    Message = "Belirtilen kullanıcı bulunamadı", 
                    Code = "USER_NOT_FOUND",
                    Field = "createdById",
                    Suggestion = "Geçerli bir kullanıcı ID'si girin"
                });
            }

            // Domain oluştur (Name'den)
            var domain = dto.Name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ı", "i")
                .Replace("ö", "o")
                .Replace("ç", "c")
                + "-" + Guid.NewGuid().ToString("N")[..8];

            var workspace = new Workspace
            {
                Id = CreateEntityId(EntityType.Workspace), // EntityType'a uygun ID oluştur
                Name = dto.Name,
                Description = dto.Description,
                Domain = domain,
                CreatedById = dto.CreatedById,
                IsActive = true
            };

            await _context.Workspaces.AddAsync(workspace);
            await _context.SaveChangesAsync();

            // Creator'ı workspace'e owner olarak ekle
            try
            {
                Console.WriteLine($"Creating Member for UserId: {dto.CreatedById}, WorkspaceId: {workspace.Id}");
                
                var memberId = ChatTask.Shared.Helpers.MemberIdHelper.CreateMemberId(
                    dto.CreatedById, 
                    workspace.Id, 
                    ChatTask.Shared.Enums.MemberRole.Owner
                );
                
                Console.WriteLine($"Generated MemberId: {memberId}");
                
                var member = new Member
                {
                    Id = memberId,
                    UserId = dto.CreatedById,
                    ParentId = workspace.Id,
                    Role = ChatTask.Shared.Enums.MemberRole.Owner,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };

                Console.WriteLine($"Member object created: Id={member.Id}, UserId={member.UserId}, ParentId={member.ParentId}, Role={member.Role}");

                // Member'ı doğrudan database'e ekle
                await _context.Members.AddAsync(member);
                Console.WriteLine("Member added to context");
                
                await _context.SaveChangesAsync();
                Console.WriteLine("Member saved to database successfully");
            }
            catch (Exception memberEx)
            {
                // Console log - geliştirici için detaylı debug
                Console.WriteLine($"Member creation error: {memberEx.Message}");
                Console.WriteLine($"Member creation stack trace: {memberEx.StackTrace}");
                if (memberEx.InnerException != null)
                {
                    Console.WriteLine($"Member creation inner exception: {memberEx.InnerException.Message}");
                }
                
                // Return response - kullanıcı için güvenli hata mesajı
                return StatusCode(500, new { 
                    Message = "Workspace oluşturuldu ama üye eklenirken hata oluştu", 
                    Code = "MEMBER_CREATION_ERROR",
                    Field = "member",
                    Suggestion = "Lütfen daha sonra tekrar deneyin",
                    Timestamp = DateTime.UtcNow,
                    // Sadece development ortamında detaylı hata göster
                    Error = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                        ? memberEx.Message 
                        : "Üye ekleme işlemi başarısız"
                });
            }

            return Ok(_mappingService.ToWorkspaceDto(workspace));
        }
        catch (DbUpdateException dbEx)
        {
            // Database constraint violations
            if (dbEx.InnerException?.Message.Contains("UNIQUE") == true)
            {
                return Conflict(new { 
                    Message = "Bu workspace adı veya domain zaten kullanılıyor", 
                    Code = "WORKSPACE_ALREADY_EXISTS",
                    Field = "name",
                    Suggestion = "Farklı bir workspace adı deneyin"
                });
            }
            
            return StatusCode(500, new { 
                Message = "Veritabanı hatası oluştu", 
                Code = "DATABASE_ERROR",
                Error = dbEx.InnerException?.Message ?? dbEx.Message,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            // Console log - geliştirici için detaylı debug
            Console.WriteLine($"CreateWorkspace Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            
            // Return response - kullanıcı için güvenli hata mesajı
            return StatusCode(500, new { 
                Message = "Workspace oluşturulurken beklenmeyen hata oluştu", 
                Code = "INTERNAL_SERVER_ERROR",
                Field = "workspace",
                Suggestion = "Lütfen daha sonra tekrar deneyin",
                Timestamp = DateTime.UtcNow,
                // Sadece development ortamında detaylı hata göster
                Error = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                    ? ex.Message 
                    : "Workspace oluşturma işlemi başarısız",
                InnerError = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                    ? ex.InnerException?.Message 
                    : null
            });
        }
    }
        // YENİ: Tüm workspace'leri getir
    [HttpGet("workspaces")]
    public async Task<IActionResult> GetWorkspaces()
    {
        try
        {
            var workspaces = await _context.Workspaces
                .Include(w => w.Members)
                .Include(w => w.Conversations)
                .ToListAsync();

            var workspaceDtos = workspaces.Select(w => new WorkspaceDto
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description,
                CreatedById = w.CreatedById,
                IsActive = w.IsActive,
                CreatedAt = w.CreatedAt,
                MemberCount = w.Members.Count,
                ConversationCount = w.Conversations.Count
            }).ToList();

            return Ok(_mappingService.ToWorkspaceDtoList(workspaces));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Workspace'ler alınırken hata oluştu", Error = ex.Message });
        }
    }
        // YENİ: Workspace güncelle
    [HttpPut("workspaces/{workspaceId:guid}")]
    public async Task<IActionResult> UpdateWorkspace(Guid workspaceId, [FromBody] UpdateWorkspaceDto dto)
    {
        try
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);

            if (workspace == null)
                return NotFound("Workspace bulunamadı");

            // Sadece owner güncelleyebilir
            var isOwner = workspace.Members.Any(m => m.GetUserId() == dto.UpdatedById && m.Role == ChatTask.Shared.Enums.MemberRole.Owner);
            if (!isOwner)
                return Forbid("Workspace'i güncelleme yetkiniz yok");

            workspace.Name = dto.Name ?? workspace.Name;
            workspace.Description = dto.Description ?? workspace.Description;
            workspace.IsActive = dto.IsActive ?? workspace.IsActive;

            await _context.SaveChangesAsync();
            return Ok(workspace);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Workspace güncellenirken hata oluştu", Error = ex.Message });
        }
    }
        // YENİ: Workspace sil
    [HttpDelete("workspaces/{workspaceId:guid}")]
    public async Task<IActionResult> DeleteWorkspace(Guid workspaceId, [FromBody] Guid deletedById)
    {
        try
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);

            if (workspace == null)
                return NotFound("Workspace bulunamadı");

            // Sadece owner silebilir
            var isOwner = workspace.Members.Any(m => m.GetUserId() == deletedById && m.Role == ChatTask.Shared.Enums.MemberRole.Owner);
            if (!isOwner)
                return Forbid("Workspace'i silme yetkiniz yok");

            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();

            return Ok("Workspace başarıyla silindi");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Workspace silinirken hata oluştu", Error = ex.Message });
        }
    }
        // YENİ: Workspace'e üye ekle
    [HttpPost("workspaces/{workspaceId:guid}/members")]
    public async Task<IActionResult> AddWorkspaceMember(Guid workspaceId, [FromBody] CreateMemberDto dto)
    {
        try
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);

            if (workspace == null)
                return NotFound("Workspace bulunamadı");

            // Sadece owner veya admin üye ekleyebilir
            var canAddMember = workspace.Members.Any(m => m.GetUserId() == dto.AddedById && 
                (m.Role == ChatTask.Shared.Enums.MemberRole.Owner || m.Role == ChatTask.Shared.Enums.MemberRole.Admin));
            
            if (!canAddMember)
                return Forbid("Workspace'e üye ekleme yetkiniz yok");

            if (workspace.Members.Any(m => m.GetUserId() == dto.UserId))
                return BadRequest("Kullanıcı zaten workspace'e üye");

            var role = dto.Role != MemberRole.Member ? dto.Role : MemberRole.Member;
            
            var memberId = ChatTask.Shared.Helpers.MemberIdHelper.CreateMemberId(
                dto.UserId, 
                workspaceId, 
                role
            );
            
            workspace.Members.Add(new Member
            {
                Id = memberId,
                UserId = dto.UserId,
                ParentId = workspaceId,
                Role = role,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            });

            await _context.SaveChangesAsync();
            return Ok("Üye başarıyla eklendi");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Üye eklenirken hata oluştu", Error = ex.Message });
        }
    }
        // YENİ: Workspace'den üye çıkar
    [HttpDelete("workspaces/{workspaceId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveWorkspaceMember(Guid workspaceId, Guid userId, [FromBody] Guid removedById)
    {
        try
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);

            if (workspace == null)
                return NotFound("Workspace bulunamadı");

            // Sadece owner veya admin üye çıkarabilir
            var canRemoveMember = workspace.Members.Any(m => m.GetUserId() == removedById && 
                (m.Role == ChatTask.Shared.Enums.MemberRole.Owner || m.Role == ChatTask.Shared.Enums.MemberRole.Admin));
            
            if (!canRemoveMember)
                return Forbid("Workspace'den üye çıkarma yetkiniz yok");

            var member = workspace.Members.FirstOrDefault(m => m.GetUserId() == userId);
            if (member == null)
                return NotFound("Üye bulunamadı");

            // Owner kendini çıkaramaz
            if (member.Role == ChatTask.Shared.Enums.MemberRole.Owner)
                return BadRequest("Owner kendini workspace'den çıkaramaz");

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return Ok("Üye başarıyla çıkarıldı");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Üye çıkarılırken hata oluştu", Error = ex.Message });
        }
    }
    // YENÄ°: Workspace'deki tÃ¼m conversation'larÄ± getir
    [HttpGet("workspaces/{workspaceId:guid}")]
    public async Task<IActionResult> GetConversations(Guid workspaceId, string? type = null)
    {
        try
        {
            var query = _context.Conversations
                .Where(c => c.WorkspaceId == workspaceId)
                .Include(c => c.Members)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1));

            // Type filtreleme - Type casting'i kaldÄ±rÄ±ldÄ±
            if (!string.IsNullOrEmpty(type))
            {
                // Type filtreleme ÅŸimdilik devre dÄ±ÅŸÄ±
                // query = type.ToLower() switch
                // {
                //     "channel" => query.OfType<Channel>(),
                //     "group" => query.OfType<Group>(),
                //     "direct" => query.OfType<DirectMessage>(),
                //     "task" => query.OfType<TaskGroup>(),
                //     _ => query
                // };
            }

            var conversations = await query.ToListAsync();

            var conversationDtos = conversations.Select(c => new ConversationDto
            {
                Id = c.Id,
                WorkspaceId = c.WorkspaceId,
                Name = c.Name,
                Description = c.Description,
                Type = c.Type,
                IsPrivate = c.IsPrivate,
                IsArchived = c.IsArchived,
                CreatedAt = c.CreatedAt,
                CreatedById = c.CreatedById,
                DisplayName = c.GetDisplayName(),
                MemberCount = c.Members.Count,
                LastMessage = c.Messages.FirstOrDefault()?.Content ?? "No messages"
            }).ToList();

            return Ok(conversationDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Conversation'lar alÄ±nÄ±rken hata oluÅŸtu", Error = ex.Message });
        }
    }

    // YENÄ°: Channel oluÅŸtur
    [HttpPost("workspaces/{workspaceId:guid}/channels")]
    public async Task<IActionResult> CreateChannel(Guid workspaceId, [FromBody] CreateConversationDto dto)
    {
        try
        {
            var channel = new Channel
            {
                WorkspaceId = workspaceId,
                Name = dto.Name,
                Description = dto.Description,
                Topic = dto.Topic ?? string.Empty,
                Purpose = dto.ChannelPurpose ?? ChannelPurpose.General,
                IsPrivate = dto.IsPrivate,
                AutoAddNewMembers = dto.AutoAddNewMembers ?? true,
                CreatedById = dto.CreatedById,
                Type = ChatTask.Shared.Enums.ConversationType.Channel
            };

            // Üyeleri ekle
            foreach (var userId in dto.InitialMemberIds)
            {
                var memberId = ChatTask.Shared.Helpers.MemberIdHelper.CreateMemberId(
                    userId, 
                    channel.Id, 
                    userId == dto.CreatedById ? ChatTask.Shared.Enums.MemberRole.Owner : ChatTask.Shared.Enums.MemberRole.Member
                );
                
                channel.Members.Add(new Member
                {
                    Id = memberId,
                    UserId = userId,
                    ParentId = channel.Id,
                    Role = userId == dto.CreatedById ? ChatTask.Shared.Enums.MemberRole.Owner : ChatTask.Shared.Enums.MemberRole.Member,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            await _context.Channels.AddAsync(channel);
            await _context.SaveChangesAsync();

            return Ok(channel);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Channel oluÅŸturulurken hata oluÅŸtu", Error = ex.Message });
        }
    }

    // YENÄ°: Group oluÅŸtur
    [HttpPost("workspaces/{workspaceId:guid}/groups")]
    public async Task<IActionResult> CreateGroup(Guid workspaceId, [FromBody] CreateConversationDto dto)
    {
        try
        {
            var group = new Group
            {
                WorkspaceId = workspaceId,
                Name = dto.Name,
                Description = dto.Description,
                Purpose = dto.GroupPurpose ?? GroupPurpose.Project,
                IsPrivate = dto.IsPrivate,
                ExpiresAt = dto.ExpiresAt,
                CreatedById = dto.CreatedById,
                Type = ChatTask.Shared.Enums.ConversationType.Group
            };

            // Üyeleri ekle
            foreach (var userId in dto.MemberIds)
            {
                var memberId = ChatTask.Shared.Helpers.MemberIdHelper.CreateMemberId(
                    userId, 
                    group.Id, 
                    userId == dto.CreatedById ? ChatTask.Shared.Enums.MemberRole.Owner : ChatTask.Shared.Enums.MemberRole.Member
                );
                
                group.Members.Add(new Member
                {
                    Id = memberId,
                    UserId = userId,
                    ParentId = group.Id,
                    Role = userId == dto.CreatedById ? ChatTask.Shared.Enums.MemberRole.Owner : ChatTask.Shared.Enums.MemberRole.Member,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();

            return Ok(group);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Group oluÅŸturulurken hata oluÅŸtu", Error = ex.Message });
        }
    }

    // YENÄ°: Direct message oluÅŸtur
    [HttpPost("workspaces/{workspaceId:guid}/direct-messages")]
    public async Task<IActionResult> CreateDirectMessage(Guid workspaceId, [FromBody] List<Guid> participantIds)
    {
        try
        {
            if (participantIds.Count != 2)
                return BadRequest("Direct message sadece 2 kiÅŸi arasÄ±nda olabilir");

            // Zaten var olan DM kontrolü
            var existingDM = await _context.DirectMessages
                .Where(dm => dm.WorkspaceId == workspaceId)
                .Where(dm => dm.Members.Count == 2 &&
                           dm.Members.All(m => participantIds.Contains(m.GetUserId())))
                .FirstOrDefaultAsync();

            if (existingDM != null)
                return Ok(existingDM);

            var directMessage = new DirectMessage
            {
                WorkspaceId = workspaceId,
                Name = "Direct Message",
                CreatedById = participantIds.First(),
                Type = ChatTask.Shared.Enums.ConversationType.DirectMessage,
                IsPrivate = true
            };

            // Katılımcıları ekle
            foreach (var participantId in participantIds)
            {
                var memberId = ChatTask.Shared.Helpers.MemberIdHelper.CreateMemberId(
                    participantId, 
                    directMessage.Id, 
                    ChatTask.Shared.Enums.MemberRole.Member
                );
                
                directMessage.Members.Add(new Member
                {
                    Id = memberId,
                    UserId = participantId,
                    ParentId = directMessage.Id,
                    Role = ChatTask.Shared.Enums.MemberRole.Member,
                    JoinedAt = DateTime.UtcNow,
                IsActive = true
                });
            }

            await _context.DirectMessages.AddAsync(directMessage);
            await _context.SaveChangesAsync();

            return Ok(directMessage);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Direct message oluÅŸturulurken hata oluÅŸtu", Error = ex.Message });
        }
    }

    // YENÄ°: Conversation'daki mesajlarÄ± getir
    [HttpGet("{conversationId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid conversationId, int page = 1, int pageSize = 50)
    {
        try
        {
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var messageDtos = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                IsRead = m.IsRead,
                ThreadId = m.ThreadId,
                SenderName = "User" // UserService'den alınacak
            }).ToList();

            return Ok(messageDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Mesajlar alÄ±nÄ±rken hata oluÅŸtu", Error = ex.Message });
        }
    }

    // YENÄ°: Mesaj gÃ¶nder
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request)
    {
        try
        {
            // Conversation kontrolÃ¼
            var conversation = await _context.Conversations
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

            if (conversation == null)
                return NotFound("Conversation bulunamadÄ±");

            // Kullanıcının conversation'a üye olup olmadığını kontrol et
            if (!conversation.Members.Any(m => m.GetUserId() == request.SenderId))
                return Forbid("Bu conversation'a mesaj gönderme yetkiniz yok");

            var message = new Message
            {
                ConversationId = request.ConversationId,
                SenderId = request.SenderId,
                Content = request.Content,
                ThreadId = request.ThreadId
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            // SignalR ile real-time gÃ¶nderim
            var messageDto = new MessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                IsRead = message.IsRead,
                ThreadId = message.ThreadId
            };

            await _hubContext.Clients.Group(request.ConversationId.ToString())
                .SendAsync("NewMessage", messageDto);

            return Ok(messageDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Mesaj gÃ¶nderilirken hata oluÅŸtu", Error = ex.Message });
        }
    }

    // YENÄ°: Conversation'a Ã¼ye ekle
    [HttpPost("{conversationId:guid}/members")]
    public async Task<IActionResult> AddMember(Guid conversationId, [FromBody] Guid userId)
    {
        try
        {
            var conversation = await _context.Conversations
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                return NotFound("Conversation bulunamadÄ±");

            if (conversation.Members.Any(m => m.GetUserId() == userId))
                return BadRequest("Kullanıcı zaten üye");

            var memberId = ChatTask.Shared.Helpers.MemberIdHelper.CreateMemberId(
                userId, 
                conversationId, 
                MemberRole.Member
            );
            
            conversation.Members.Add(new Member
            {
                Id = memberId,
                UserId = userId,
                ParentId = conversation.Id,
                Role = MemberRole.Member,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            });

            await _context.SaveChangesAsync();

            return Ok("Ãœye eklendi");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Ãœye eklenirken hata oluÅŸtu", Error = ex.Message });
        }
    }

    // YENÄ°: Conversation'dan Ã¼ye Ã§Ä±kar
    [HttpDelete("{conversationId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid conversationId, Guid userId)
    {
        try
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.GetParentId() == conversationId && m.GetUserId() == userId);

            if (member == null)
                return NotFound("Üye bulunamadı");

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return Ok("Ãœye Ã§Ä±karÄ±ldÄ±");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Ãœye Ã§Ä±karÄ±lÄ±rken hata oluÅŸtu", Error = ex.Message });
        }
    }
}


