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
    private readonly RoleBasedFilteringService _roleBasedFiltering;
    private readonly IUserService _userService;
    private readonly ChatMappingService _mappingService;
    private readonly IHttpClientFactory _httpClientFactory;
    
    // Standart GUID oluşturma - EntityType bilgisi artık ParentType ile saklanıyor

    public ConversationController(ChatDbContext context, IHubContext<ChatHub> hubContext, IUserService userService, ChatMappingService mappingService, IHttpClientFactory httpClientFactory, RoleBasedFilteringService roleBasedFiltering)
    {
        _context = context;
        _hubContext = hubContext;
        _userService = userService;
        _mappingService = mappingService;
        _httpClientFactory = httpClientFactory;
        _roleBasedFiltering = roleBasedFiltering;
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
        // YENİ: Kullanıcının üye olduğu workspace'leri getir
    [HttpGet("workspaces")]
    public async Task<IActionResult> GetWorkspaces([FromQuery] Guid userId)
    {
        try
        {
            // Kullanıcının üye olduğu workspace ID'lerini bul
            var userWorkspaceIds = await _context.Members
                .Where(m => m.UserId == userId && 
                           m.ParentType == ChatTask.Shared.Enums.ParentType.Workspace &&
                           m.IsActive)
                .Select(m => m.ParentId)
                .ToListAsync();

            // Sadece bu workspace'leri getir
            var workspaces = await _context.Workspaces
                .Where(w => userWorkspaceIds.Contains(w.Id))
                .Include(w => w.Conversations)
                .ToListAsync();

            // Member count'ları ayrı ayrı hesapla
            var workspaceDtos = new List<WorkspaceDto>();
            foreach (var workspace in workspaces)
            {
                var memberCount = await _context.Members
                    .CountAsync(m => m.ParentId == workspace.Id && 
                                   m.ParentType == ChatTask.Shared.Enums.ParentType.Workspace);

                workspaceDtos.Add(new WorkspaceDto
                {
                    Id = workspace.Id,
                    Name = workspace.Name,
                    Description = workspace.Description,
                    CreatedById = workspace.CreatedById,
                    IsActive = workspace.IsActive,
                    CreatedAt = workspace.CreatedAt,
                    MemberCount = memberCount,
                    ConversationCount = workspace.Conversations.Count
                });
            }

            return Ok(workspaceDtos);
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
                .FirstOrDefaultAsync(w => w.Id == workspaceId);

            if (workspace == null)
            {
                Console.WriteLine("ERROR: Workspace not found");
                return NotFound("Workspace bulunamadı");
            }

            Console.WriteLine($"Workspace found: {workspace.Name}");

            // Members'ı ayrı query ile al (çünkü navigation property [NotMapped])
            var workspaceMembers = await _context.Members
                .Where(m => m.ParentId == workspaceId && 
                           m.ParentType == ChatTask.Shared.Enums.ParentType.Workspace &&
                           m.IsActive)
                .ToListAsync();

            Console.WriteLine($"Workspace members count: {workspaceMembers.Count}");
            foreach (var member in workspaceMembers)
            {
                Console.WriteLine($"Member: UserId={member.UserId}, Role={member.Role}");
            }

            // Sadece owner veya admin üye ekleyebilir
            var addedByGuid = Guid.Parse(dto.AddedById);
            var canAddMember = workspaceMembers.Any(m => m.UserId == addedByGuid && 
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

            // Kullanıcı zaten üye mi kontrol et
            var userGuid = Guid.Parse(dto.UserId);
            if (workspaceMembers.Any(m => m.UserId == userGuid))
                return BadRequest("Kullanıcı zaten workspace'e üye");

            var role = Enum.TryParse<ChatTask.Shared.Enums.MemberRole>(dto.Role, out var parsedRole) 
                ? parsedRole 
                : ChatTask.Shared.Enums.MemberRole.Member;
            
            var newMember = new Member
            {
                UserId = userGuid,
                ParentId = workspaceId,
                ParentType = ChatTask.Shared.Enums.ParentType.Workspace,
                Role = role,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Members.AddAsync(newMember);

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

    // YENİ: Workspace member'larını getir
    [HttpGet("workspaces/{workspaceId:guid}/members")]
    public async Task<IActionResult> GetWorkspaceMembers(Guid workspaceId)
    {
        try
        {
            // Workspace member'larını ve user bilgilerini getir
            var members = await _context.Members
                .Where(m => m.ParentId == workspaceId && 
                           m.ParentType == ChatTask.Shared.Enums.ParentType.Workspace &&
                           m.IsActive)
                .Select(m => new {
                    UserId = m.UserId,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt
                })
                .ToListAsync();

            if (!members.Any())
                return Ok(new List<object>());

            // User bilgilerini UserService'den al
            var userIds = members.Select(m => m.UserId).ToList();
            var users = new List<object>();
            
            foreach (var member in members)
            {
                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var userServiceUrl = "http://chattask-userservice:5001";
                    var response = await httpClient.GetAsync($"{userServiceUrl}/api/users/{member.UserId}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var userJson = await response.Content.ReadAsStringAsync();
                        var user = System.Text.Json.JsonSerializer.Deserialize<dynamic>(userJson);
                        
                        users.Add(new {
                            id = member.UserId.ToString(),
                            name = user?.GetProperty("name").GetString() ?? "Unknown",
                            email = user?.GetProperty("email").GetString() ?? "unknown@email.com",
                            role = member.Role.ToString(),
                            joinedAt = member.JoinedAt
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to get user {member.UserId}: {ex.Message}");
                }
            }

            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Workspace member'ları alınırken hata oluştu", Error = ex.Message });
        }
    }

    // YENİ: Conversation member'larını getir
    [HttpGet("{conversationId:guid}/members")]
    public async Task<IActionResult> GetConversationMembers(Guid conversationId)
    {
        try
        {
            // Conversation member'larını ve user bilgilerini getir
            var members = await _context.Members
                .Where(m => m.ParentId == conversationId && 
                           m.ParentType == ChatTask.Shared.Enums.ParentType.Conversation &&
                           m.IsActive)
                .Select(m => new {
                    UserId = m.UserId,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt
                })
                .ToListAsync();

            if (!members.Any())
                return Ok(new List<object>());

            // User bilgilerini UserService'den al
            var users = new List<object>();
            
            foreach (var member in members)
            {
                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var userServiceUrl = "http://chattask-userservice:5001";
                    var response = await httpClient.GetAsync($"{userServiceUrl}/api/users/{member.UserId}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var userJson = await response.Content.ReadAsStringAsync();
                        var user = System.Text.Json.JsonSerializer.Deserialize<dynamic>(userJson);
                        
                        users.Add(new {
                            id = member.UserId.ToString(),
                            name = user?.GetProperty("name").GetString() ?? "Unknown",
                            email = user?.GetProperty("email").GetString() ?? "unknown@email.com",
                            role = member.Role.ToString(),
                            joinedAt = member.JoinedAt
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to get user {member.UserId}: {ex.Message}");
                }
            }

            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Conversation member'ları alınırken hata oluştu", Error = ex.Message });
        }
    }

    // YENİ: Conversation'ı ID ile getir
    [HttpGet("{conversationId:guid}")]
    public async Task<IActionResult> GetConversation(Guid conversationId)
    {
        try
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                return NotFound("Conversation bulunamadı");

            var response = new
            {
                id = conversation.Id,
                name = conversation.Name,
                description = conversation.Description,
                type = conversation.Type,
                workspaceId = conversation.WorkspaceId,
                createdById = conversation.CreatedById,
                createdAt = conversation.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetConversation] ERROR: {ex.Message}");
            return StatusCode(500, new { Message = "Conversation alınırken hata oluştu", Error = ex.Message });
        }
    }

    // YENÄ°: Workspace'deki kullanıcının üye olduğu conversation'larÄ± getir
    [HttpGet("workspaces/{workspaceId:guid}")]
    public async Task<IActionResult> GetConversations(Guid workspaceId, [FromQuery] Guid userId, string? type = null)
    {
        try
        {
            Console.WriteLine($"[GetConversations] Starting - WorkspaceId: {workspaceId}, UserId: {userId}, Type: {type}");
            
            // Rol tabanlı filtreleme kullan
            var conversations = await _roleBasedFiltering.GetConversationsByRole(workspaceId, userId);
            
            Console.WriteLine($"[GetConversations] Found {conversations.Count} conversations based on user role");

            // Type filtreleme
            if (!string.IsNullOrWhiteSpace(type))
            {
                var lowered = type.ToLowerInvariant();
                conversations = lowered switch
                {
                    "channel" => conversations.OfType<Channel>().Cast<Conversation>().ToList(),
                    "group" => conversations.OfType<Group>().Cast<Conversation>().ToList(),
                    "dm" or "direct" or "directmessage" => conversations.OfType<DirectMessage>().Cast<Conversation>().ToList(),
                    "task" or "taskgroup" => conversations.OfType<TaskGroup>().Cast<Conversation>().ToList(),
                    _ => conversations
                };
            }

            var conversationDtos = new List<ConversationDto>();
            
            Console.WriteLine($"[GetConversations] Processing {conversations.Count} conversations");

            foreach (var c in conversations)
            {
                // Member count'u ayrı ayrı hesapla
                var memberCount = await _context.Members
                    .CountAsync(m => m.ParentId == c.Id && 
                                    m.ParentType == ChatTask.Shared.Enums.ParentType.Conversation);

                // Last message'ı ayrı ayrı al
                var lastMessage = await _context.Messages
                    .Where(m => m.ConversationId == c.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Content)
                    .FirstOrDefaultAsync() ?? "No messages";

                conversationDtos.Add(new ConversationDto
            {
                Id = c.Id,
                WorkspaceId = c.WorkspaceId,
                Name = c.Name,
                Description = c.Description,
                Type = c.Type,
                IsArchived = c.IsArchived,
                CreatedAt = c.CreatedAt,
                CreatedById = c.CreatedById,
                    DisplayName = c.Name,
                    MemberCount = memberCount,
                    LastMessage = lastMessage
                });
            }

            Console.WriteLine($"[GetConversations] Returning {conversationDtos.Count} conversations");
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

            // Creator'ı otomatik olarak Owner olarak ekle
            var creatorMember = new Member
            {
                UserId = dto.CreatedById,
                ParentId = channel.Id,
                ParentType = ChatTask.Shared.Enums.ParentType.Conversation,
                Role = ChatTask.Shared.Enums.MemberRole.Owner,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };
            Console.WriteLine($"[CreateChannel] Adding creator as Owner: UserId={dto.CreatedById}");
            await _context.Members.AddAsync(creatorMember);

            // Şimdi diğer üyeleri ekle (Channel'a özel member'lar)
            Console.WriteLine($"[CreateChannel] Adding {dto.InitialMemberIds.Count} additional members to channel");
            foreach (var userId in dto.InitialMemberIds)
            {
                // Creator zaten eklendi, tekrar ekleme
                if (userId == dto.CreatedById) continue;

                var member = new Member
                {
                    UserId = userId,
                    ParentId = channel.Id, // Artık ID var
                    ParentType = ChatTask.Shared.Enums.ParentType.Conversation,
                    Role = ChatTask.Shared.Enums.MemberRole.Member,
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
            Console.WriteLine($"[CreateGroup] Starting - WorkspaceId: {workspaceId}, Name: {dto.Name}, CreatedById: {dto.CreatedById}");
            Console.WriteLine($"[CreateGroup] InitialMemberIds: {string.Join(", ", dto.InitialMemberIds)}");

            // Creator'ın var olduğunu kontrol et
            bool creatorExists = await _userService.UserExistsAsync(dto.CreatedById);
            if (!creatorExists)
            {
                Console.WriteLine($"[CreateGroup] ERROR: Creator {dto.CreatedById} not found");
                return BadRequest(new { Message = $"Creator {dto.CreatedById} not found" });
            }
            Console.WriteLine($"[CreateGroup] Creator validation passed: {dto.CreatedById}");

            // InitialMemberIds'deki kullanıcıların var olduğunu kontrol et
            var validMemberIds = new List<Guid>();
            foreach (var memberId in dto.InitialMemberIds)
            {
                if (memberId == Guid.Empty)
                {
                    Console.WriteLine($"[CreateGroup] Skipping empty member ID");
                    continue;
                }

                bool memberExists = await _userService.UserExistsAsync(memberId);
                if (memberExists)
                {
                    validMemberIds.Add(memberId);
                    Console.WriteLine($"[CreateGroup] Valid member: {memberId}");
                }
                else
                {
                    Console.WriteLine($"[CreateGroup] Invalid member (not found): {memberId}");
                }
            }

            Console.WriteLine($"[CreateGroup] Valid members: {validMemberIds.Count}/{dto.InitialMemberIds.Count}");

            var group = new Group
            {
                WorkspaceId = workspaceId,
                Name = dto.Name,
                Description = dto.Description,
                Purpose = dto.GroupPurpose ?? GroupPurpose.Project,
                ExpiresAt = dto.ExpiresAt,
                CreatedById = dto.CreatedById,
                Type = ChatTask.Shared.Enums.ConversationType.Group
            };

            // Önce Group'u kaydet (ID oluşsun)
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[CreateGroup] Group saved with ID: {group.Id}");

            // Creator'ı otomatik olarak Owner olarak ekle
            var creatorMember = new Member
            {
                UserId = dto.CreatedById,
                ParentId = group.Id,
                ParentType = ChatTask.Shared.Enums.ParentType.Conversation,
                Role = ChatTask.Shared.Enums.MemberRole.Owner,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };
            Console.WriteLine($"[CreateGroup] Adding creator as Owner: UserId={dto.CreatedById}");
            await _context.Members.AddAsync(creatorMember);

            // Şimdi diğer üyeleri ekle (sadece geçerli olanları)
            Console.WriteLine($"[CreateGroup] Adding {validMemberIds.Count} additional members to group");
            foreach (var userId in validMemberIds)
            {
                // Creator zaten eklendi, tekrar ekleme
                if (userId == dto.CreatedById) continue;

                var member = new Member
                {
                    UserId = userId,
                    ParentId = group.Id,
                    ParentType = ChatTask.Shared.Enums.ParentType.Conversation,
                    Role = ChatTask.Shared.Enums.MemberRole.Member,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };
                Console.WriteLine($"[CreateGroup] Adding group member: UserId={userId}, ParentId={group.Id}, Role={member.Role}");
                await _context.Members.AddAsync(member);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"[CreateGroup] Group created successfully: {group.Id}");

            // JSON cycle'ı önlemek için sadece gerekli alanları döndür
            var response = new
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                Purpose = group.Purpose,
                ExpiresAt = group.ExpiresAt,
                CreatedById = group.CreatedById,
                Type = group.Type,
                WorkspaceId = group.WorkspaceId,
                CreatedAt = group.CreatedAt,
                MemberCount = validMemberIds.Count + 1 // Creator + valid members
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateGroup] ERROR: {ex.Message}");
            return StatusCode(500, new { Message = "Group oluÅŸturulurken hata oluÅŸtu", Error = ex.Message });
        }
    }

    // YENİ: TaskGroup oluştur
    [HttpPost("workspaces/{workspaceId:guid}/taskgroups")]
    public async Task<IActionResult> CreateTaskGroup(Guid workspaceId, [FromBody] CreateConversationDto dto)
    {
        try
        {
            Console.WriteLine($"[CreateTaskGroup] Starting - WorkspaceId: {workspaceId}, Name: {dto.Name}, CreatedById: {dto.CreatedById}");
            Console.WriteLine($"[CreateTaskGroup] InitialMemberIds: {string.Join(", ", dto.InitialMemberIds)}");
            
            // Workspace'in var olduğunu kontrol et
            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                Console.WriteLine($"[CreateTaskGroup] ERROR: Workspace {workspaceId} not found in database");
                return BadRequest(new { Message = $"Workspace {workspaceId} not found" });
            }
            Console.WriteLine($"[CreateTaskGroup] Workspace found: {workspace.Name}");

            if (!Guid.TryParse(dto.CreatedById.ToString(), out var createdById))
            {
                return BadRequest(new { Message = "Invalid CreatedById format" });
            }

            // Creator'ın var olduğunu kontrol et
            bool creatorExists = await _userService.UserExistsAsync(createdById);
            if (!creatorExists)
            {
                Console.WriteLine($"[CreateTaskGroup] ERROR: Creator {createdById} not found");
                return BadRequest(new { Message = $"Creator {createdById} not found" });
            }
            Console.WriteLine($"[CreateTaskGroup] Creator validation passed: {createdById}");

            // InitialMemberIds'deki kullanıcıların var olduğunu kontrol et
            var validMemberIds = new List<Guid>();
            foreach (var memberId in dto.InitialMemberIds)
            {
                if (memberId == Guid.Empty)
                {
                    Console.WriteLine($"[CreateTaskGroup] Skipping empty member ID");
                    continue;
                }

                bool memberExists = await _userService.UserExistsAsync(memberId);
                if (memberExists)
                {
                    validMemberIds.Add(memberId);
                    Console.WriteLine($"[CreateTaskGroup] Valid member: {memberId}");
                }
                else
                {
                    Console.WriteLine($"[CreateTaskGroup] Invalid member (not found): {memberId}");
                }
            }

            Console.WriteLine($"[CreateTaskGroup] Valid members: {validMemberIds.Count}/{dto.InitialMemberIds.Count}");

            var taskGroup = new TaskGroup
            {
                WorkspaceId = workspaceId,
                Name = dto.Name,
                Description = dto.Description,
                CreatedById = createdById,
                Type = ChatTask.Shared.Enums.ConversationType.TaskGroup
            };

            // Önce TaskGroup'u kaydet (ID oluşsun)
            await _context.TaskGroups.AddAsync(taskGroup);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[CreateTaskGroup] TaskGroup saved with ID: {taskGroup.Id}");

            // Creator'ı otomatik olarak Owner olarak ekle
            var creatorMember = new Member
            {
                UserId = createdById,
                ParentId = taskGroup.Id,
                ParentType = ChatTask.Shared.Enums.ParentType.Conversation,
                Role = ChatTask.Shared.Enums.MemberRole.Owner,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };
            Console.WriteLine($"[CreateTaskGroup] Adding creator as Owner: UserId={dto.CreatedById}");
            await _context.Members.AddAsync(creatorMember);

            // Şimdi diğer üyeleri ekle (sadece geçerli olanları)
            Console.WriteLine($"[CreateTaskGroup] Adding {validMemberIds.Count} additional members to taskgroup");
            foreach (var userId in validMemberIds)
            {
                // Creator zaten eklendi, tekrar ekleme
                if (userId == createdById) continue;

                var member = new Member
                {
                    UserId = userId,
                    ParentId = taskGroup.Id,
                    ParentType = ChatTask.Shared.Enums.ParentType.Conversation,
                    Role = ChatTask.Shared.Enums.MemberRole.Member,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };
                Console.WriteLine($"[CreateTaskGroup] Adding taskgroup member: UserId={userId}, ParentId={taskGroup.Id}, Role={member.Role}");
                await _context.Members.AddAsync(member);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"[CreateTaskGroup] TaskGroup created successfully: {taskGroup.Id}");

            // JSON cycle'ı önlemek için sadece gerekli alanları döndür
            var response = new
            {
                id = taskGroup.Id,
                name = taskGroup.Name,
                description = taskGroup.Description,
                createdById = taskGroup.CreatedById,
                type = taskGroup.Type,
                workspaceId = taskGroup.WorkspaceId,
                createdAt = taskGroup.CreatedAt,
                memberCount = validMemberIds.Count + 1 // Creator + valid members
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateTaskGroup] ERROR: {ex.Message}");
            return StatusCode(500, new { Message = "TaskGroup oluşturulurken hata oluştu", Error = ex.Message });
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
                Type = ChatTask.Shared.Enums.ConversationType.DirectMessage
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
                    Role = participantId == dto.ParticipantIds.First() ? 
                           ChatTask.Shared.Enums.MemberRole.Owner : 
                           ChatTask.Shared.Enums.MemberRole.Member, // İlk katılımcı Owner
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
    [HttpPost("{conversationId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendMessageDto request)
    {
        try
        {
            // Conversation kontrolÃ¼
            // Conversation kontrolü
            var conversation = await _context.Conversations
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

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
                ConversationId = conversationId,
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

            await _hubContext.Clients.Group(conversationId.ToString())
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

    // YENİ: Browse Conversations - Kullanıcının dahil olmadığı conversation'ları göster
    [HttpGet("workspaces/{workspaceId:guid}/browse")]
    public async Task<IActionResult> BrowseConversations(Guid workspaceId, [FromQuery] Guid userId, string? type = null)
    {
        try
        {
            Console.WriteLine($"[BrowseConversations] Starting - WorkspaceId: {workspaceId}, UserId: {userId}, Type: {type}");
            
            // Kullanıcının workspace'teki rolünü kontrol et
            var userWorkspaceRole = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == userId && 
                                        m.ParentId == workspaceId && 
                                        m.ParentType == ParentType.Workspace &&
                                        m.IsActive);

            if (userWorkspaceRole == null)
                return Forbid("Bu workspace'e erişim yetkiniz yok");

            // Kullanıcının üye olduğu conversation'ları al
            var userConversationIds = await _context.Members
                .Where(m => m.UserId == userId && 
                           m.ParentType == ParentType.Conversation &&
                           m.IsActive)
                .Select(m => m.ParentId)
                .ToListAsync();

            // Tüm conversation'ları al (üye olmadığı)
            var allConversations = await _context.Conversations
                .Where(c => c.WorkspaceId == workspaceId && 
                           !userConversationIds.Contains(c.Id) &&
                           !c.IsArchived)
                .ToListAsync();

            // Type filtreleme
            if (!string.IsNullOrWhiteSpace(type))
            {
                var lowered = type.ToLowerInvariant();
                allConversations = lowered switch
                {
                    "channel" => allConversations.OfType<Channel>().Cast<Conversation>().ToList(),
                    "group" => allConversations.OfType<Group>().Cast<Conversation>().ToList(),
                    "dm" or "direct" or "directmessage" => allConversations.OfType<DirectMessage>().Cast<Conversation>().ToList(),
                    "task" or "taskgroup" => allConversations.OfType<TaskGroup>().Cast<Conversation>().ToList(),
                    _ => allConversations
                };
            }

            // Rol bazlı filtreleme
            var browseableConversations = userWorkspaceRole.Role switch
            {
                MemberRole.Owner => allConversations, // Owner her şeyi görebilir
                MemberRole.Admin => allConversations, // Admin her şeyi görebilir
                MemberRole.Member => allConversations.OfType<Channel>().Where(c => ((Channel)c).IsPublic).Cast<Conversation>().ToList(), // Sadece public channel'lar
                _ => new List<Conversation>()
            };

            var conversationDtos = new List<ConversationDto>();
            
            foreach (var c in browseableConversations)
            {
                // Member count'u hesapla
                var memberCount = await _context.Members
                    .CountAsync(m => m.ParentId == c.Id && 
                                    m.ParentType == ParentType.Conversation);

                // Last message'ı al
                var lastMessage = await _context.Messages
                    .Where(m => m.ConversationId == c.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Content)
                    .FirstOrDefaultAsync() ?? "No messages";

                conversationDtos.Add(new ConversationDto
                {
                    Id = c.Id,
                    WorkspaceId = c.WorkspaceId,
                    Name = c.Name,
                    Description = c.Description,
                    Type = c.Type,
                    IsArchived = c.IsArchived,
                    CreatedAt = c.CreatedAt,
                    CreatedById = c.CreatedById,
                    DisplayName = c.Name,
                    MemberCount = memberCount,
                    LastMessage = lastMessage
                });
            }

            Console.WriteLine($"[BrowseConversations] Returning {conversationDtos.Count} browseable conversations");
            return Ok(conversationDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowseConversations] Error: {ex.Message}");
            return StatusCode(500, new { Message = "Conversation'lar alınırken hata oluştu", Error = ex.Message });
        }
    }

    // YENİ: Üye rolünü değiştir (Owner tarafından)
    [HttpPut("conversations/{conversationId:guid}/members/{userId:guid}/role")]
    public async Task<IActionResult> ChangeMemberRole(Guid conversationId, Guid userId, [FromBody] ChangeRoleDto dto)
    {
        try
        {
            Console.WriteLine($"[ChangeMemberRole] Starting - ConversationId: {conversationId}, UserId: {userId}, NewRole: {dto.Role}, RequestedBy: {dto.RequestedBy}");

            // Conversation'ı bul
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                return NotFound("Conversation bulunamadı");

            // İstek yapan kişinin conversation'daki rolünü kontrol et
            var requesterMember = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == dto.RequestedBy && 
                                        m.ParentId == conversationId && 
                                        m.ParentType == ParentType.Conversation &&
                                        m.IsActive);

            if (requesterMember == null)
                return Forbid("Bu conversation'a erişim yetkiniz yok");

            if (requesterMember.Role != MemberRole.Owner)
                return Forbid("Sadece conversation sahibi rol değiştirebilir");

            // Rolü değiştirilecek üyeyi bul
            var targetMember = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == userId && 
                                        m.ParentId == conversationId && 
                                        m.ParentType == ParentType.Conversation &&
                                        m.IsActive);

            if (targetMember == null)
                return NotFound("Üye bulunamadı");

            // Owner'ın kendi rolünü değiştirmesini engelle
            if (userId == dto.RequestedBy)
                return BadRequest("Kendi rolünüzü değiştiremezsiniz");

            // Geçerli rol kontrolü
            if (!Enum.IsDefined(typeof(MemberRole), dto.Role))
                return BadRequest("Geçersiz rol");

            // Rolü güncelle
            var oldRole = targetMember.Role;
            targetMember.Role = dto.Role;

            await _context.SaveChangesAsync();

            Console.WriteLine($"[ChangeMemberRole] Role changed successfully: UserId={userId}, OldRole={oldRole}, NewRole={dto.Role}");

            return Ok(new
            {
                Message = "Rol başarıyla değiştirildi",
                UserId = userId,
                OldRole = oldRole.ToString(),
                NewRole = dto.Role.ToString()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChangeMemberRole] Error: {ex.Message}");
            return StatusCode(500, new { Message = "Rol değiştirilirken hata oluştu", Error = ex.Message });
        }
    }

    // YENİ: Conversation üyelerini getir
    [HttpGet("conversations/{conversationId:guid}/members")]
    public async Task<IActionResult> GetConversationMembers(Guid conversationId, [FromQuery] Guid userId)
    {
        try
        {
            Console.WriteLine($"[GetConversationMembers] Starting - ConversationId: {conversationId}, UserId: {userId}");

            // Conversation'ı bul
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                return NotFound("Conversation bulunamadı");

            // Kullanıcının conversation'a erişim yetkisi var mı?
            var userMember = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == userId && 
                                        m.ParentId == conversationId && 
                                        m.ParentType == ParentType.Conversation &&
                                        m.IsActive);

            if (userMember == null)
                return Forbid("Bu conversation'a erişim yetkiniz yok");

            // Conversation üyelerini getir
            var members = await _context.Members
                .Where(m => m.ParentId == conversationId && 
                           m.ParentType == ParentType.Conversation &&
                           m.IsActive)
                .OrderBy(m => m.Role) // Owner, Admin, Member sırası
                .ThenBy(m => m.JoinedAt)
                .ToListAsync();

            // Kullanıcı bilgilerini al
            var memberDtos = new List<object>();
            foreach (var member in members)
            {
                // UserService'den kullanıcı bilgilerini al
                try
                {
                    var userInfo = await _userService.GetUserByIdAsync(member.UserId);
                    if (userInfo != null)
                    {
                        memberDtos.Add(new
                        {
                            UserId = member.UserId,
                            Name = userInfo.Name,
                            Email = userInfo.Email,
                            Avatar = userInfo.Avatar,
                            Role = member.Role.ToString(),
                            JoinedAt = member.JoinedAt,
                            IsCurrentUser = member.UserId == userId,
                            CanChangeRole = userMember.Role == MemberRole.Owner && member.UserId != userId
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GetConversationMembers] Error getting user info for {member.UserId}: {ex.Message}");
                    // Kullanıcı bilgisi alınamazsa sadece temel bilgileri ekle
                    memberDtos.Add(new
                    {
                        UserId = member.UserId,
                        Name = "Unknown User",
                        Email = "",
                        Avatar = "",
                        Role = member.Role.ToString(),
                        JoinedAt = member.JoinedAt,
                        IsCurrentUser = member.UserId == userId,
                        CanChangeRole = userMember.Role == MemberRole.Owner && member.UserId != userId
                    });
                }
            }

            Console.WriteLine($"[GetConversationMembers] Returning {memberDtos.Count} members");
            return Ok(memberDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetConversationMembers] Error: {ex.Message}");
            return StatusCode(500, new { Message = "Üyeler alınırken hata oluştu", Error = ex.Message });
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
                            MemberCount = _context.Members
                                .Count(m => m.ParentId == c.Id && 
                                           m.ParentType == ChatTask.Shared.Enums.ParentType.Conversation),
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


