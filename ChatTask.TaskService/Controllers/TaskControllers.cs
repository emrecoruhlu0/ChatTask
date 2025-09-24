using ChatTask.TaskService.Context;
using ChatTask.TaskService.Models;
using ChatTask.TaskService.Services;
using ChatTask.Shared.DTOs;
using ChatTask.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ChatTask.TaskService.Controllers;

[ApiController]
[Route("api/tasks")]
// [Authorize] - Geçici olarak kaldırıldı
public class TaskController : ControllerBase
{
    private readonly TaskDbContext _context;
    private readonly TaskMappingService _mappingService;
    private readonly IHttpClientFactory _httpClientFactory;

    public TaskController(TaskDbContext context, TaskMappingService mappingService, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _mappingService = mappingService;
        _httpClientFactory = httpClientFactory;
}
     
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[CreateTask] Request received: Title={request.Title}, CreatedById={request.CreatedById}, IsPrivate={request.IsPrivate}, ChannelId={request.ChannelId}");
            Console.WriteLine($"[CreateTask] AssigneeIds: {request.AssigneeIds?.Count ?? 0} items");
            if (request.AssigneeIds != null)
            {
                foreach (var id in request.AssigneeIds)
                {
                    Console.WriteLine($"[CreateTask] AssigneeId: {id}");
                }
            }
            try
            {
                Console.WriteLine($"[CreateTask] Full request: {System.Text.Json.JsonSerializer.Serialize(request)}");
            }
            catch (Exception jsonEx)
            {
                Console.WriteLine($"[CreateTask] JSON serialization error: {jsonEx.Message}");
            }
            
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { Message = "Title is required and cannot be empty" });
            }
            
            if (string.IsNullOrWhiteSpace(request.CreatedById))
            {
                return BadRequest(new { Message = "CreatedById is required and cannot be empty" });
            }
            
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { Message = "Status is required and cannot be empty" });
            }
            
            if (string.IsNullOrWhiteSpace(request.Priority))
            {
                return BadRequest(new { Message = "Priority is required and cannot be empty" });
            }
            
            // Parse enums with error handling
            if (!Enum.TryParse<ChatTask.Shared.Enums.TaskStatus>(request.Status, out var taskStatus))
            {
                return BadRequest(new { Message = $"Invalid status: {request.Status}" });
            }
            
            if (!Enum.TryParse<ChatTask.Shared.Enums.TaskPriority>(request.Priority, out var taskPriority))
            {
                return BadRequest(new { Message = $"Invalid priority: {request.Priority}" });
            }
            
            if (!Guid.TryParse(request.CreatedById, out var createdById))
            {
                return BadRequest(new { Message = $"Invalid CreatedById format: {request.CreatedById}" });
            }
            
            var task = new ProjectTask // Task yerine ProjectTask
        {
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            DueDate = request.DueDate,
            Status = taskStatus,
            Priority = taskPriority,
            IsPrivate = request.IsPrivate,
            CreatedById = createdById,
            TaskGroupId = request.TaskGroupId
        };

        // Önce task'ı kaydet (ID oluşsun)
        await _context.Tasks.AddAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Şimdi assignee ID'leri varsa, TaskAssignment'lar oluştur
        if (request.AssigneeIds != null && request.AssigneeIds.Any())
        {
            Console.WriteLine($"[CreateTask] Processing {request.AssigneeIds.Count} assignee IDs");
            foreach (var assigneeId in request.AssigneeIds)
            {
                if (string.IsNullOrWhiteSpace(assigneeId))
                {
                    Console.WriteLine($"[CreateTask] Skipping empty assignee ID");
                    continue;
                }
                
                if (!Guid.TryParse(assigneeId, out var userId))
                {
                    Console.WriteLine($"[CreateTask] Invalid assignee ID format: {assigneeId}");
                    continue;
                }
                
                var assignment = new TaskAssignment
                {
                    TaskId = task.Id,
                    UserId = userId,
                    Status = ChatTask.Shared.Enums.AssignmentStatus.Assigned
                };
                await _context.TaskAssignments.AddAsync(assignment, cancellationToken);
                Console.WriteLine($"[CreateTask] Added assignment for user {userId}");
            }
            await _context.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"[CreateTask] Saved {request.AssigneeIds.Count} assignments");
        }
        else
        {
            Console.WriteLine($"[CreateTask] No assignee IDs provided, skipping assignments");
        }

        // TaskGroup oluştur (her task için)
        Console.WriteLine($"[CreateTask] ChannelId check: HasValue={request.ChannelId.HasValue}, Value={request.ChannelId}");
        Console.WriteLine($"[CreateTask] About to create TaskGroup for task {task.Id}");
        try
        {
            await CreateTaskGroupForTask(task.Id, request.ChannelId, request.AssigneeIds, cancellationToken);
            Console.WriteLine($"[CreateTask] TaskGroup created successfully for task {task.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateTask] TaskGroup creation failed: {ex.Message}");
            // TaskGroup oluşturulamazsa task'ı sil
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync(cancellationToken);
            return StatusCode(500, new { Message = "TaskGroup oluşturulurken hata oluştu", Error = ex.Message });
        }

        // Task entity'sini DTO'ya çevir (JSON cycle'ı önlemek için)
        var taskDto = _mappingService.ToTaskDto(task);
        Console.WriteLine($"[CreateTask] Task created successfully: {taskDto.Id}");
        return Ok(taskDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateTask] Error: {ex.Message}");
            Console.WriteLine($"[CreateTask] Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { Message = "Task oluşturulurken hata oluştu", Error = ex.Message });
        }
    }

    private async Task CreateTaskGroupForTask(Guid taskId, Guid? channelId, List<string>? assigneeIds, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[CreateTaskGroupForTask] Starting - TaskId: {taskId}, ChannelId: {channelId}, AssigneeIds: {assigneeIds?.Count ?? 0}");
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30); // Add timeout
            var chatServiceUrl = "http://chattask-chatservice:5002";

            // Task'ı al
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                throw new Exception($"Task {taskId} not found");
            }

            // Workspace ID'yi al
            string workspaceId;
            string? channelName = null;
            if (channelId.HasValue)
            {
                // Channel'dan workspace'i al
                var channelResponse = await httpClient.GetAsync($"{chatServiceUrl}/api/conversations/{channelId}", cancellationToken);
                if (!channelResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to get channel info: {channelResponse.StatusCode} - {await channelResponse.Content.ReadAsStringAsync(cancellationToken)}");
                }
                
                var channelContent = await channelResponse.Content.ReadAsStringAsync(cancellationToken);
                var channel = JsonSerializer.Deserialize<JsonElement>(channelContent);
                workspaceId = channel.GetProperty("workspaceId").GetString()!;
                channelName = channel.GetProperty("name").GetString();
            }
            else
            {
                // Task creator'ın workspace'ini al (UserService'den)
                Console.WriteLine($"[CreateTaskGroupForTask] Getting workspaces for user: {task.CreatedById}");
                var userResponse = await httpClient.GetAsync($"http://chattask-userservice:5001/api/users/{task.CreatedById}/workspaces", cancellationToken);
                if (!userResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to get user workspaces: {userResponse.StatusCode} - {await userResponse.Content.ReadAsStringAsync(cancellationToken)}");
                }
                
                var userContent = await userResponse.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"[CreateTaskGroupForTask] User workspaces response: {userContent}");
                var workspaces = JsonSerializer.Deserialize<JsonElement[]>(userContent);
                if (workspaces == null || workspaces.Length == 0)
                {
                    throw new Exception("User has no workspaces");
                }
                
                // İlk workspace'i kullan
                var userServiceWorkspaceId = workspaces[0].GetProperty("id").GetString()!;
                Console.WriteLine($"[CreateTaskGroupForTask] Selected workspace ID from UserService: {userServiceWorkspaceId}");
                
                // ChatService'de bu workspace'in var olup olmadığını kontrol et
                // Önce kullanıcının ChatService'deki workspace'lerini al
                Console.WriteLine($"[CreateTaskGroupForTask] Getting user's ChatService workspaces to check if UserService workspace exists");
                var userChatWorkspacesResponse = await httpClient.GetAsync($"{chatServiceUrl}/api/conversations/workspaces?userId={task.CreatedById}", cancellationToken);
                Console.WriteLine($"[CreateTaskGroupForTask] User's ChatService workspaces response status: {userChatWorkspacesResponse.StatusCode}");
                
                if (userChatWorkspacesResponse.IsSuccessStatusCode)
                {
                    var userChatWorkspacesContent = await userChatWorkspacesResponse.Content.ReadAsStringAsync(cancellationToken);
                    Console.WriteLine($"[CreateTaskGroupForTask] User's ChatService workspaces: {userChatWorkspacesContent}");
                    var userChatWorkspaces = JsonSerializer.Deserialize<JsonElement[]>(userChatWorkspacesContent);
                    
                    if (userChatWorkspaces != null && userChatWorkspaces.Length > 0)
                    {
                        // UserService'deki workspace ID'nin ChatService'de var olup olmadığını kontrol et
                        var workspaceExists = userChatWorkspaces.Any(w => w.GetProperty("id").GetString() == userServiceWorkspaceId);
                        Console.WriteLine($"[CreateTaskGroupForTask] UserService workspace {userServiceWorkspaceId} exists in ChatService: {workspaceExists}");
                        
                        if (workspaceExists)
                        {
                            // UserService'deki workspace ChatService'de de var
                            workspaceId = userServiceWorkspaceId;
                            Console.WriteLine($"[CreateTaskGroupForTask] Using UserService workspace ID: {workspaceId}");
                        }
                        else
                        {
                            // UserService'deki workspace ChatService'de yok, ChatService'deki ilk workspace'i kullan
                            workspaceId = userChatWorkspaces[0].GetProperty("id").GetString()!;
                            Console.WriteLine($"[CreateTaskGroupForTask] Using ChatService workspace ID: {workspaceId}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[CreateTaskGroupForTask] No workspaces found in ChatService for user, skipping TaskGroup creation");
                        return; // Skip TaskGroup creation if no workspaces exist
                    }
                }
                else
                {
                    Console.WriteLine($"[CreateTaskGroupForTask] Failed to get user's ChatService workspaces, skipping TaskGroup creation");
                    return; // Skip TaskGroup creation if we can't get workspaces
                }
            }

            // TaskGroup oluştur
            var initialMemberIds = assigneeIds?.Where(id => !string.IsNullOrWhiteSpace(id)).ToList() ?? new List<string>();
            if (initialMemberIds.Count == 0)
            {
                initialMemberIds = new List<string> { task.CreatedById.ToString() };
            }

            var taskGroupData = new
            {
                name = channelId.HasValue ? 
                    $"Task: {task.Title} (Channel: {channelName})" : 
                    $"Task: {task.Title}",
                description = channelId.HasValue ? 
                    $"Task group for '{task.Title}' from channel '{channelName}'" : 
                    $"Task group for '{task.Title}'",
                isPrivate = false,
                createdById = task.CreatedById.ToString(),
                workspaceId = workspaceId,
                initialMemberIds = initialMemberIds
            };

            Console.WriteLine($"[CreateTaskGroupForTask] TaskGroup data: Name={taskGroupData.name}, CreatedById={taskGroupData.createdById}, WorkspaceId={taskGroupData.workspaceId}");
            Console.WriteLine($"[CreateTaskGroupForTask] InitialMemberIds: {string.Join(", ", taskGroupData.initialMemberIds)}");
            Console.WriteLine($"[CreateTaskGroupForTask] About to call ChatService: POST {chatServiceUrl}/api/conversations/workspaces/{workspaceId}/taskgroups");

            var response = await httpClient.PostAsJsonAsync($"{chatServiceUrl}/api/conversations/workspaces/{workspaceId}/taskgroups", taskGroupData, cancellationToken);
            Console.WriteLine($"[CreateTaskGroupForTask] ChatService response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var taskGroup = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var taskGroupId = taskGroup.GetProperty("id").GetString();
                
                Console.WriteLine($"[CreateTaskGroupForTask] TaskGroup created: {taskGroupId}");
                
                // Task'ı TaskGroup ile ilişkilendir
                task.TaskGroupId = Guid.Parse(taskGroupId!);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"TaskGroup creation failed: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateTaskGroupForTask] Error: {ex.Message}");
            Console.WriteLine($"[CreateTaskGroupForTask] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTasks([FromQuery] Guid? userId, CancellationToken cancellationToken)
    {
        var tasks = await _context.Tasks
            .Include(t => t.Assignments)
            .ToListAsync(cancellationToken);

        // Private task filtreleme: Sadece oluşturan kişi ve atanan kişiler görebilir
        if (userId.HasValue)
        {
            tasks = tasks.Where(t => 
                !t.IsPrivate || // Public task'ler herkes görebilir
                t.CreatedById == userId.Value || // Oluşturan kişi görebilir
                t.Assignments.Any(a => a.UserId == userId.Value) // Atanan kişiler görebilir
            ).ToList();
        }
        
        // Task entity'lerini DTO'lara çevir (JSON cycle'ı önlemek için)
        var taskDtos = tasks.Select(_mappingService.ToTaskDto).ToList();
        return Ok(taskDtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTask(Guid id, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            
        if (task == null)
            return NotFound("Görev bulunamadı");
            
        // Task entity'sini DTO'ya çevir (JSON cycle'ı önlemek için)
        var taskDto = _mappingService.ToTaskDto(task);
        return Ok(taskDto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskDto request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            
        if (task == null)
            return NotFound("Görev bulunamadı");
            
        task.Title = request.Title;
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        // Task entity'sini DTO'ya çevir (JSON cycle'ı önlemek için)
        var taskDto = _mappingService.ToTaskDto(task);
        return Ok(taskDto);
    }
    
    [HttpDelete("{id:guid}")]   
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (task == null)
            return NotFound("Görev bulunamadı");
            
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok("Görev silindi");
    }
    
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusDto dto, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (task == null)
            return NotFound("Görev bulunamadı");
            
        task.Status = Enum.Parse<ChatTask.Shared.Enums.TaskStatus>(dto.Status);
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        
        // Task entity'sini DTO'ya çevir (JSON cycle'ı önlemek için)
        var taskDto = _mappingService.ToTaskDto(task);
        return Ok(taskDto);
    }
    
    
    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> AssignTaskToUsers(Guid id, [FromBody] List<Guid> userIds, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            
        if (task == null)
            return NotFound("Görev bulunamadı");

        // Mevcut atamaları kaldır
        _context.TaskAssignments.RemoveRange(task.Assignments);

        // Yeni atamaları ekle
        foreach (var userId in userIds)
        {
            task.Assignments.Add(new TaskAssignment
            {
                TaskId = task.Id,
                UserId = userId,
                Status = ChatTask.Shared.Enums.AssignmentStatus.Assigned
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok($"Görev {userIds.Count} kişiye atandı");
    }

    // YENİ: Channel'a ait task'ları getir
    [HttpGet("channel/{channelId:guid}")]
    public async Task<IActionResult> GetTasksByChannel(Guid channelId, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[GetTasksByChannel] Starting - ChannelId: {channelId}");
            
            // TaskGroup'ları bul (channel'dan oluşturulan)
            var httpClient = _httpClientFactory.CreateClient();
            var chatServiceUrl = "http://chattask-chatservice:5002";
            
            // Channel'ın workspace'ini al
            var channelResponse = await httpClient.GetAsync($"{chatServiceUrl}/api/conversations/{channelId}", cancellationToken);
            if (!channelResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"[GetTasksByChannel] Channel not found: {channelId}");
                return Ok(new List<object>()); // Channel bulunamazsa boş liste döndür
            }
            
            var channelContent = await channelResponse.Content.ReadAsStringAsync(cancellationToken);
            var channel = JsonSerializer.Deserialize<JsonElement>(channelContent);
            var workspaceId = channel.GetProperty("workspaceId").GetString();
            Console.WriteLine($"[GetTasksByChannel] Channel workspace: {workspaceId}");

            // Workspace'teki TaskGroup'ları bul
            var taskGroupsResponse = await httpClient.GetAsync($"{chatServiceUrl}/api/conversations/workspaces/{workspaceId}?type=taskgroup", cancellationToken);
            if (!taskGroupsResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"[GetTasksByChannel] Failed to get TaskGroups: {taskGroupsResponse.StatusCode}");
                return Ok(new List<object>());
            }
            
            var taskGroupsContent = await taskGroupsResponse.Content.ReadAsStringAsync(cancellationToken);
            var taskGroups = JsonSerializer.Deserialize<JsonElement[]>(taskGroupsContent);
            if (taskGroups == null)
            {
                Console.WriteLine($"[GetTasksByChannel] No TaskGroups found");
                return Ok(new List<object>());
            }
            Console.WriteLine($"[GetTasksByChannel] Found {taskGroups.Length} TaskGroups");
            
            // Channel'a özel TaskGroup'ları filtrele (name'inde channel ID'si olanlar)
            var channelTaskGroups = taskGroups.Where(tg => 
            {
                var name = tg.GetProperty("name").GetString() ?? "";
                return name.Contains($"Channel: {channelId}");
            }).ToList();
            
            Console.WriteLine($"[GetTasksByChannel] Found {channelTaskGroups.Count} TaskGroups for channel {channelId}");
            
            var taskGroupIds = channelTaskGroups.Select(tg => Guid.Parse(tg.GetProperty("id").GetString()!)).ToList();
            Console.WriteLine($"[GetTasksByChannel] TaskGroup IDs: {string.Join(", ", taskGroupIds)}");

            // Bu TaskGroup'lara ait task'ları getir
            var tasks = await _context.Tasks
                .Include(t => t.Assignments)
                .Where(t => taskGroupIds.Contains(t.TaskGroupId ?? Guid.Empty))
                .ToListAsync(cancellationToken);

            Console.WriteLine($"[GetTasksByChannel] Found {tasks.Count} tasks for TaskGroups");

            // Task entity'lerini DTO'lara çevir
            var taskDtos = tasks.Select(_mappingService.ToTaskDto).ToList();
            Console.WriteLine($"[GetTasksByChannel] Returning {taskDtos.Count} task DTOs");
            return Ok(taskDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetTasksByChannel] Error: {ex.Message}");
            Console.WriteLine($"[GetTasksByChannel] Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { Message = "Channel task'ları alınırken hata oluştu", Error = ex.Message });
        }
    }

    // YENİ: Kullanıcıya atanmış task'ları getir
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetTasksByUser(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[GetTasksByUser] Starting - UserId: {userId}");
            
            // Kullanıcıya atanmış task'ları getir
            var tasks = await _context.Tasks
                .Include(t => t.Assignments)
                .Where(t => t.Assignments.Any(a => a.UserId == userId))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);

            Console.WriteLine($"[GetTasksByUser] Found {tasks.Count} tasks for user {userId}");

            // Task entity'lerini DTO'lara çevir
            var taskDtos = tasks.Select(_mappingService.ToTaskDto).ToList();
            Console.WriteLine($"[GetTasksByUser] Returning {taskDtos.Count} task DTOs");
            return Ok(taskDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetTasksByUser] Error: {ex.Message}");
            Console.WriteLine($"[GetTasksByUser] Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { Message = "Kullanıcı task'ları alınırken hata oluştu", Error = ex.Message });
        }
    }

    // TaskGroup metodları geçici olarak kaldırıldı
}
