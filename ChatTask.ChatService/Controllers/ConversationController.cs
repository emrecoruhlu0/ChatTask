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
    
    // Standart GUID oluşturma - EntityType bilgisi artık ParentType ile saklanıyor

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
            Console.WriteLine($"=== WORKSPACE CREATION STARTED ===");
            Console.WriteLine($"Request DTO: Name='{dto.Name}', CreatedById='{dto.CreatedById}', Description='{dto.Description}'");

            // Input validation
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                Console.WriteLine("ERROR: Workspace name is empty");
                return BadRequest(new { 
                    Message = "Workspace adı boş olamaz", 
                    Code = "INVALID_WORKSPACE_NAME",
                    Field = "name",
                    Suggestion = "Geçerli bir workspace adı girin"
                });
            }

            if (dto.CreatedById == Guid.Empty)
            {
                Console.WriteLine("ERROR: CreatedById is empty GUID");
                return BadRequest(new { 
                    Message = "Geçerli bir kullanıcı ID'si gerekli", 
                    Code = "INVALID_USER_ID",
                    Field = "createdById",
                    Suggestion = "Geçerli bir kullanıcı ID'si girin"
                });
            }

            Console.WriteLine("Input validation passed");

            // User'ın var olup olmadığını kontrol et (UserService'ten)
            Console.WriteLine("Checking if user exists...");
            bool userExists;
            try
            {
                userExists = await _userService.UserExistsAsync(dto.CreatedById);
                Console.WriteLine($"User exists check result: {userExists}");
            }
            catch (Exception userCheckEx)
            {
                Console.WriteLine($"ERROR: User existence check failed: {userCheckEx.Message}");
                return StatusCode(500, new { 
                    Message = "Kullanıcı kontrolü sırasında hata oluştu", 
                    Code = "USER_CHECK_ERROR",
                    Error = userCheckEx.Message
                });
            }

            if (!userExists)
            {
                Console.WriteLine("ERROR: User not found");
                return BadRequest(new { 
                    Message = "Belirtilen kullanıcı bulunamadı", 
                    Code = "USER_NOT_FOUND",
                    Field = "createdById",
                    Suggestion = "Geçerli bir kullanıcı ID'si girin"
                });
            }

            Console.WriteLine("User validation passed");

            // Domain oluştur (Name'den)
            Console.WriteLine("Creating domain...");
            string domain;
            try
            {
                domain = dto.Name.ToLowerInvariant()
                    .Replace(" ", "-")
                    .Replace("ğ", "g")
                    .Replace("ü", "u")
                    .Replace("ş", "s")
                    .Replace("ı", "i")
                    .Replace("ö", "o")
                    .Replace("ç", "c")
                    + "-" + Guid.NewGuid().ToString("N")[..8];
                Console.WriteLine($"Domain created: {domain}");
            }
            catch (Exception domainEx)
            {
                Console.WriteLine($"ERROR: Domain creation failed: {domainEx.Message}");
                return StatusCode(500, new { 
                    Message = "Domain oluşturulurken hata oluştu", 
                    Code = "DOMAIN_CREATION_ERROR",
                    Error = domainEx.Message
                });
            }

            // Workspace object oluştur
            Console.WriteLine("Creating workspace object...");
            Workspace workspace;
            try
            {
                workspace = new Workspace
                {
                    Id = Guid.NewGuid(), // Standart GUID oluştur
                    Name = dto.Name,
                    Description = dto.Description,
                    Domain = domain,
                    CreatedById = dto.CreatedById,
                    IsActive = true
                };
                Console.WriteLine($"Workspace object created: Id={workspace.Id}, Name={workspace.Name}");
            }
            catch (Exception workspaceObjEx)
            {
                Console.WriteLine($"ERROR: Workspace object creation failed: {workspaceObjEx.Message}");
                return StatusCode(500, new { 
                    Message = "Workspace nesnesi oluşturulurken hata oluştu", 
                    Code = "WORKSPACE_OBJECT_ERROR",
                    Error = workspaceObjEx.Message
                });
            }

            // Workspace'i veritabanına kaydet
            Console.WriteLine("Saving workspace to database...");
            try
            {
                await _context.Workspaces.AddAsync(workspace);
                Console.WriteLine("Workspace added to context");
                
                await _context.SaveChangesAsync();
                Console.WriteLine("Workspace saved to database successfully");
            }
            catch (Exception workspaceSaveEx)
            {
                Console.WriteLine($"ERROR: Workspace save failed: {workspaceSaveEx.Message}");
                Console.WriteLine($"Workspace save stack trace: {workspaceSaveEx.StackTrace}");
                if (workspaceSaveEx.InnerException != null)
                {
                    Console.WriteLine($"Workspace save inner exception: {workspaceSaveEx.InnerException.Message}");
                }
                return StatusCode(500, new { 
                    Message = "Workspace kaydedilirken hata oluştu", 
                    Code = "WORKSPACE_SAVE_ERROR",
                    Error = workspaceSaveEx.Message,
                    InnerError = workspaceSaveEx.InnerException?.Message
                });
            }

            // Creator'ı workspace'e owner olarak ekle
            Console.WriteLine("Creating member for workspace owner...");
            try
            {
                Console.WriteLine($"Creating Member for UserId: {dto.CreatedById}, WorkspaceId: {workspace.Id}");
                
                Member member;
                try
                {
                    member = new Member
                    {
                        UserId = dto.CreatedById,
                        ParentId = workspace.Id,
                        ParentType = ChatTask.Shared.Enums.ParentType.Workspace,
                        Role = ChatTask.Shared.Enums.MemberRole.Owner,
                        JoinedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    Console.WriteLine($"Member object created: UserId={member.UserId}, ParentId={member.ParentId}, ParentType={member.ParentType}, Role={member.Role}");
                }
                catch (Exception memberObjEx)
                {
                    Console.WriteLine($"ERROR: Member object creation failed: {memberObjEx.Message}");
                    return StatusCode(500, new { 
                        Message = "Member nesnesi oluşturulurken hata oluştu", 
                        Code = "MEMBER_OBJECT_ERROR",
                        Error = memberObjEx.Message
                    });
                }

                // Member'ı veritabanına ekle
                try
                {
                    await _context.Members.AddAsync(member);
                    Console.WriteLine("Member added to context");
                    
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Member saved to database successfully");
                }
                catch (Exception memberSaveEx)
                {
                    Console.WriteLine($"ERROR: Member save failed: {memberSaveEx.Message}");
                    Console.WriteLine($"Member save stack trace: {memberSaveEx.StackTrace}");
                    if (memberSaveEx.InnerException != null)
                    {
                        Console.WriteLine($"Member save inner exception: {memberSaveEx.InnerException.Message}");
                    }
                    
                    return StatusCode(500, new { 
                        Message = "Member kaydedilirken hata oluştu", 
                        Code = "MEMBER_SAVE_ERROR",
                        Error = memberSaveEx.Message,
                        InnerError = memberSaveEx.InnerException?.Message
                    });
                }
            }
            catch (Exception memberEx)
            {
                // Genel member oluşturma hatası
                Console.WriteLine($"ERROR: General member creation error: {memberEx.Message}");
                Console.WriteLine($"Member creation stack trace: {memberEx.StackTrace}");
                if (memberEx.InnerException != null)
                {
                    Console.WriteLine($"Member creation inner exception: {memberEx.InnerException.Message}");
                }
                
                return StatusCode(500, new { 
                    Message = "Workspace oluşturuldu ama üye eklenirken hata oluştu", 
                    Code = "MEMBER_CREATION_ERROR",
                    Field = "member",
                    Suggestion = "Lütfen daha sonra tekrar deneyin",
                    Timestamp = DateTime.UtcNow,
                    Error = memberEx.Message,
                    InnerError = memberEx.InnerException?.Message
                });
            }

            Console.WriteLine("=== WORKSPACE CREATION COMPLETED SUCCESSFULLY ===");
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
            var isOwner = workspace.Members.Any(m => m.UserId == dto.UpdatedById && m.Role == ChatTask.Shared.Enums.MemberRole.Owner);
            if (!isOwner)
                return BadRequest("Workspace'i güncelleme yetkiniz yok");

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
            var isOwner = workspace.Members.Any(m => m.UserId == deletedById && m.Role == ChatTask.Shared.Enums.MemberRole.Owner);
            if (!isOwner)
                return BadRequest("Workspace'i silme yetkiniz yok");

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
            Console.WriteLine($"=== ADD WORKSPACE MEMBER STARTED ===");
            Console.WriteLine($"WorkspaceId: {workspaceId}");
            Console.WriteLine($"DTO: UserId={dto.UserId}, AddedById={dto.AddedById}, Role={dto.Role}");

            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);

            if (workspace == null)
            {
                Console.WriteLine("ERROR: Workspace not found");
                return NotFound("Workspace bulunamadı");
            }

            Console.WriteLine($"Workspace found: {workspace.Name}");
            Console.WriteLine($"Workspace members count: {workspace.Members.Count}");
            foreach (var member in workspace.Members)
            {
                Console.WriteLine($"Member: UserId={member.UserId}, Role={member.Role}");
            }

            // Sadece owner veya admin üye ekleyebilir
            var canAddMember = workspace.Members.Any(m => m.UserId == dto.AddedById && 
                (m.Role == ChatTask.Shared.Enums.MemberRole.Owner || m.Role == ChatTask.Shared.Enums.MemberRole.Admin));
            
            Console.WriteLine($"Can add member: {canAddMember}");
            
            if (!canAddMember)
            {
                Console.WriteLine("ERROR: User does not have permission to add members");
                return BadRequest(new { 
                    Message = "Workspace'e üye ekleme yetkiniz yok", 
                    Code = "NO_PERMISSION",
                    Details = $"AddedById: {dto.AddedById}, WorkspaceId: {workspaceId}"
                });
            }

            if (workspace.Members.Any(m => m.UserId == dto.UserId))
                return BadRequest("Kullanıcı zaten workspace'e üye");

            var role = dto.Role != MemberRole.Member ? dto.Role : MemberRole.Member;
            
            workspace.Members.Add(new Member
            {
                UserId = dto.UserId,
                ParentId = workspaceId,
                ParentType = ChatTask.Shared.Enums.ParentType.Workspace,
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
            var canRemoveMember = workspace.Members.Any(m => m.UserId == removedById && 
                (m.Role == ChatTask.Shared.Enums.MemberRole.Owner || m.Role == ChatTask.Shared.Enums.MemberRole.Admin));
            
            if (!canRemoveMember)
                return BadRequest("Workspace'den üye çıkarma yetkiniz yok");

            var member = workspace.Members.FirstOrDefault(m => m.UserId == userId);
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
            var baseQuery = _context.Conversations
                .AsNoTracking()
                .Where(c => c.WorkspaceId == workspaceId);

            if (!string.IsNullOrWhiteSpace(type))
            {
                var lowered = type.ToLowerInvariant();
                baseQuery = lowered switch
                {
                    "channel" => baseQuery.OfType<Channel>(),
                    "group" => baseQuery.OfType<Group>(),
                    "dm" or "direct" or "directmessage" => baseQuery.OfType<DirectMessage>(),
                    "task" or "taskgroup" => baseQuery.OfType<TaskGroup>(),
                    _ => baseQuery
                };
            }

            var conversationDtos = await baseQuery
                .Select(c => new ConversationDto
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
                    DisplayName = c.Name,
                    MemberCount = c.Members.Count,
                    LastMessage = c.Messages
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => m.Content)
                        .FirstOrDefault() ?? "No messages"
                })
                .ToListAsync();

            return Ok(conversationDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Conversation'lar alınırken hata oluştu", Error = ex.Message });
        }
    }

    // YENÄ°: Channel oluÅŸtur
    [HttpPost("workspaces/{workspaceId:guid}/channels")]
    public async Task<IActionResult> CreateChannel(Guid workspaceId, [FromBody] CreateConversationDto dto)
    {
        try
        {
            Console.WriteLine($"[CreateChannel] Starting - WorkspaceId: {workspaceId}, Name: {dto.Name}, CreatedById: {dto.CreatedById}");
            Console.WriteLine($"[CreateChannel] InitialMemberIds: {string.Join(", ", dto.InitialMemberIds)}");

            // Workspace var mı kontrol et
            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                Console.WriteLine($"[CreateChannel] ERROR: Workspace not found: {workspaceId}");
                return NotFound(new { Message = "Workspace bulunamadı" });
            }
            Console.WriteLine($"[CreateChannel] Workspace found: {workspace.Name}");

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
            Console.WriteLine($"[CreateChannel] Channel object created: {channel.Name}");

            // Önce Channel'ı kaydet (ID oluşsun)
            Console.WriteLine($"[CreateChannel] Adding channel to context");
            await _context.Channels.AddAsync(channel);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[CreateChannel] Channel saved with ID: {channel.Id}");

            // Şimdi üyeleri ekle (Channel'a özel member'lar)
            Console.WriteLine($"[CreateChannel] Adding {dto.InitialMemberIds.Count} members to channel");
            foreach (var userId in dto.InitialMemberIds)
            {
                // Channel member'ı oluştur (Conversation'a bağlı)
                var member = new Member
                {
                    UserId = userId,
                    ParentId = channel.Id, // Artık ID var
                    ParentType = ChatTask.Shared.Enums.ParentType.Conversation,
                    Role = userId == dto.CreatedById ? ChatTask.Shared.Enums.MemberRole.Owner : ChatTask.Shared.Enums.MemberRole.Member,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };
                Console.WriteLine($"[CreateChannel] Adding channel member: UserId={userId}, ParentId={channel.Id}, Role={member.Role}");
                await _context.Members.AddAsync(member);
            }
            
            Console.WriteLine($"[CreateChannel] Saving member changes to database");
            await _context.SaveChangesAsync();
            Console.WriteLine($"[CreateChannel] Channel created successfully: {channel.Id}");

            // JSON cycle'ı önlemek için sadece gerekli alanları döndür
            var response = new
            {
                Id = channel.Id,
                Name = channel.Name,
                Description = channel.Description,
                Topic = channel.Topic,
                Purpose = channel.Purpose,
                IsPrivate = channel.IsPrivate,
                AutoAddNewMembers = channel.AutoAddNewMembers,
                CreatedById = channel.CreatedById,
                Type = channel.Type,
                WorkspaceId = channel.WorkspaceId,
                CreatedAt = channel.CreatedAt,
                MemberCount = channel.Members.Count
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateChannel] ERROR: {ex.Message}");
            Console.WriteLine($"[CreateChannel] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[CreateChannel] Inner exception: {ex.InnerException.Message}");
            }
            return StatusCode(500, new { 
                Message = "Channel oluşturulurken hata oluştu", 
                Error = ex.Message,
                InnerError = ex.InnerException?.Message
            });
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

            // Üyeleri ekle (Workspace FK ile bağla; Conversation bağı navigation ile kurulur)
            foreach (var userId in dto.MemberIds)
            {
                group.Members.Add(new Member
                {
                    UserId = userId,
                    ParentId = workspaceId,
                    ParentType = ChatTask.Shared.Enums.ParentType.Workspace,
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
    public async Task<IActionResult> CreateDirectMessage(Guid workspaceId, [FromBody] CreateDirectMessageDto dto)
    {
        try
        {
            if (dto.ParticipantIds.Count != 2)
                return BadRequest("Direct message sadece 2 kiÅŸi arasÄ±nda olabilir");

            // Zaten var olan DM kontrolü
            var existingDM = await _context.DirectMessages
                .Where(dm => dm.WorkspaceId == workspaceId)
                .Where(dm => dm.Members.Count == 2 &&
                           dm.Members.All(m => dto.ParticipantIds.Contains(m.UserId)))
                .FirstOrDefaultAsync();

            if (existingDM != null)
                return Ok(existingDM);

            var directMessage = new DirectMessage
            {
                WorkspaceId = workspaceId,
                Name = dto.Name ?? "Direct Message",
                Description = dto.Description,
                CreatedById = dto.ParticipantIds.First(),
                Type = ChatTask.Shared.Enums.ConversationType.DirectMessage,
                IsPrivate = dto.IsPrivate
            };

            // Önce DirectMessage'ı kaydet (ID oluşsun)
            await _context.DirectMessages.AddAsync(directMessage);
            await _context.SaveChangesAsync();

            // Şimdi katılımcıları ekle (Direct Message'a özel member'lar)
            foreach (var participantId in dto.ParticipantIds)
            {
                var member = new Member
                {
                    UserId = participantId,
                    ParentId = directMessage.Id, // Artık ID var
                    ParentType = ChatTask.Shared.Enums.ParentType.Conversation,
                    Role = ChatTask.Shared.Enums.MemberRole.Member,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await _context.Members.AddAsync(member);
            }

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
            // Conversation kontrolü
            var conversation = await _context.Conversations
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

            if (conversation == null)
                return NotFound("Conversation bulunamadÄ±");

            // Kullanıcının conversation'a üye olup olmadığını kontrol et
            bool isMember = false;
            
            if (conversation.Type == ChatTask.Shared.Enums.ConversationType.DirectMessage)
            {
                // Direct Message için: Sadece conversation member'ı kontrol et
                var dmMember = await _context.Members
                    .FirstOrDefaultAsync(m => m.UserId == request.SenderId && 
                                            m.ParentId == conversation.Id && 
                                            m.ParentType == ChatTask.Shared.Enums.ParentType.Conversation);
                isMember = dmMember != null;
            }
            else if (conversation.Type == ChatTask.Shared.Enums.ConversationType.Channel)
            {
                // Channel için: Channel member'ı olmalı
                var channelMember = await _context.Members
                    .FirstOrDefaultAsync(m => m.UserId == request.SenderId && 
                                            m.ParentId == conversation.Id && 
                                            m.ParentType == ChatTask.Shared.Enums.ParentType.Conversation);
                isMember = channelMember != null;
            }
            else
            {
                // Diğer conversation type'lar için: Workspace member'ı kontrol et
                var workspaceMember = await _context.Members
                    .FirstOrDefaultAsync(m => m.UserId == request.SenderId && 
                                            m.ParentId == conversation.WorkspaceId && 
                                            m.ParentType == ChatTask.Shared.Enums.ParentType.Workspace);
                isMember = workspaceMember != null;
            }
            
            if (!isMember)
                return BadRequest("Bu conversation'a mesaj gönderme yetkiniz yok");

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

            if (conversation.Members.Any(m => m.UserId == userId))
                return BadRequest("Kullanıcı zaten üye");

            conversation.Members.Add(new Member
            {
                UserId = userId,
                ParentId = conversation.Id,
                ParentType = ChatTask.Shared.Enums.ParentType.Conversation,
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
                .FirstOrDefaultAsync(m => m.ParentId == conversationId && m.UserId == userId);

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

    // YENÄ°: MesajÄ± okundu olarak iÅŸaretle
    [HttpPut("messages/{messageId:guid}/read")]
    public async Task<IActionResult> MarkMessageAsRead(Guid messageId, [FromBody] MarkMessageAsReadDto dto)
    {
        try
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
                return NotFound("Mesaj bulunamadı");

            // Kullanıcının bu conversation'a üye olup olmadığını kontrol et
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == message.ConversationId);

            if (conversation == null)
                return NotFound("Conversation bulunamadı");

            // Kullanıcının conversation'a üye olup olmadığını kontrol et
            var isMember = await _context.Members
                .AnyAsync(m => m.UserId == dto.UserId && 
                               m.ParentId == conversation.Id && 
                               m.ParentType == ChatTask.Shared.Enums.ParentType.Conversation);

            if (!isMember)
                return BadRequest("Bu mesajı okuma yetkiniz yok");

            // Mesajı okundu olarak işaretle
            message.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok("Mesaj okundu olarak işaretlendi");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Mesaj işaretlenirken hata oluştu", Error = ex.Message });
        }
    }

    // YENİ: Kullanıcı login olduğunda workspace'leri gönder
    [HttpPost("users/{userId:guid}/login")]
    public async Task<IActionResult> NotifyUserLogin(Guid userId)
    {
        try
        {
            Console.WriteLine($"[NotifyUserLogin] User {userId} logged in, getting workspaces...");

            // Kullanıcının workspace'lerini getir
            var userWorkspaces = await GetUserWorkspaces(userId);
            
            Console.WriteLine($"[NotifyUserLogin] Found {userWorkspaces.Count} workspaces for user {userId}");

            // SignalR ile frontend'e gönder
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("UserWorkspaces", userWorkspaces);
            
            Console.WriteLine($"[NotifyUserLogin] Sent workspaces to user {userId} via SignalR");

            return Ok(new { 
                Message = "User workspaces sent to frontend", 
                WorkspaceCount = userWorkspaces.Count 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotifyUserLogin] Error: {ex.Message}");
            return StatusCode(500, new { 
                Message = "Failed to get user workspaces", 
                Error = ex.Message 
            });
        }
    }

    private async Task<List<object>> GetUserWorkspaces(Guid userId)
    {
        try
        {
            // Kullanıcının üye olduğu workspace'leri getir
            var workspaceIds = await _context.Members
                .Where(m => m.UserId == userId && m.ParentType == ChatTask.Shared.Enums.ParentType.Workspace)
                .Select(m => m.ParentId)
                .ToListAsync();
            
            Console.WriteLine($"[GetUserWorkspaces] User {userId} is member of {workspaceIds.Count} workspaces");

            var result = new List<object>();
            
            foreach (var workspaceId in workspaceIds)
            {
                var workspace = await _context.Workspaces.FindAsync(workspaceId);
                if (workspace != null)
                {
                    // Workspace'deki conversation'ları getir
                    var conversations = await _context.Conversations
                        .Where(c => c.WorkspaceId == workspaceId)
                        .Select(c => new
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Description = c.Description,
                            Type = c.Type.ToString(),
                            IsPrivate = c.IsPrivate,
                            MemberCount = c.Members.Count,
                            LastMessage = c.Messages
                                .OrderByDescending(m => m.CreatedAt)
                                .Select(m => m.Content)
                                .FirstOrDefault() ?? "No messages"
                        })
                        .ToListAsync();
                    
                    result.Add(new
                    {
                        Id = workspace.Id,
                        Name = workspace.Name,
                        Description = workspace.Description,
                        CreatedById = workspace.CreatedById,
                        IsActive = workspace.IsActive,
                        CreatedAt = workspace.CreatedAt,
                        MemberCount = workspace.Members.Count,
                        Conversations = conversations
                    });
                }
            }
            
            Console.WriteLine($"[GetUserWorkspaces] Returning {result.Count} workspaces with conversations");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetUserWorkspaces] Error: {ex.Message}");
            throw;
        }
    }
}


