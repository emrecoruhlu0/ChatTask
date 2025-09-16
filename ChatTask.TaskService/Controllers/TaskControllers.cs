using ChatTask.TaskService.Context;
using ChatTask.TaskService.Models;
using ChatTask.TaskService.Services;
using ChatTask.Shared.DTOs;
using ChatTask.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatTask.TaskService.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] - Geçici olarak kaldırıldı
public class TaskController : ControllerBase
{
    private readonly TaskDbContext _context;
    private readonly TaskMappingService _mappingService;

    public TaskController(TaskDbContext context, TaskMappingService mappingService)
    {
        _context = context;
        _mappingService = mappingService;
    }
     
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto request, CancellationToken cancellationToken)
    {
        var task = new ProjectTask // Task yerine ProjectTask
        {
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Status = request.Status,
            Priority = request.Priority,
            TaskGroupId = request.TaskGroupId
        };

        // Önce task'ı kaydet (ID oluşsun)
        await _context.Tasks.AddAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Şimdi assignee ID'leri varsa, TaskAssignment'lar oluştur
        if (request.AssigneeIds?.Any() == true)
        {
            foreach (var assigneeId in request.AssigneeIds)
            {
                var assignment = new TaskAssignment
                {
                    TaskId = task.Id, // Artık ID var
                    UserId = assigneeId,
                    Status = ChatTask.Shared.Enums.AssignmentStatus.Assigned
                };
                await _context.TaskAssignments.AddAsync(assignment, cancellationToken);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Task entity'sini DTO'ya çevir (JSON cycle'ı önlemek için)
        var taskDto = _mappingService.ToTaskDto(task);
        return Ok(taskDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTasks(CancellationToken cancellationToken)
    {
        var tasks = await _context.Tasks
            .Include(t => t.Assignments)
            .ToListAsync(cancellationToken);
        
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
    public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] ChatTask.Shared.Enums.TaskStatus status, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (task == null)
            return NotFound("Görev bulunamadı");
            
        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        
        // Task entity'sini DTO'ya çevir (JSON cycle'ı önlemek için)
        var taskDto = _mappingService.ToTaskDto(task);
        return Ok(taskDto);
    }
    
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetTasksByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var tasks = await _context.Tasks
            .Include(t => t.Assignments)
            .Where(t => t.Assignments.Any(a => a.UserId == userId))
            .ToListAsync(cancellationToken);
            
        // Task entity'lerini DTO'lara çevir (JSON cycle'ı önlemek için)
        var taskDtos = tasks.Select(_mappingService.ToTaskDto).ToList();
        return Ok(taskDtos);
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

    // TaskGroup metodları geçici olarak kaldırıldı
}
