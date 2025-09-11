using ChatTask.TaskService.Context;
using ChatTask.Shared.Models;
using ChatTask.Shared.Models.Conversations;
using ChatTask.Shared.DTOs;
using ChatTask.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatTask.TaskService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly TaskDbContext _context;

    public TaskController(TaskDbContext context)
    {
        _context = context;
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

        // Eğer assignee ID'leri varsa, TaskAssignment'lar oluştur
        if (request.AssigneeIds?.Any() == true)
        {
            foreach (var assigneeId in request.AssigneeIds)
            {
                            task.Assignments.Add(new TaskAssignment
            {
                TaskId = task.Id,
                UserId = assigneeId,
                Status = ChatTask.Shared.Enums.AssignmentStatus.Assigned
            });
            }
        }

        await _context.Tasks.AddAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(task);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTasks(CancellationToken cancellationToken)
    {
        var tasks = await _context.Tasks
            .Include(t => t.Assignments)
            .ThenInclude(a => a.User)
            .Include(t => t.TaskGroup)
            .ToListAsync(cancellationToken);
        
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTask(Guid id, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignments)
            .ThenInclude(a => a.User)
            .Include(t => t.TaskGroup)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            
        if (task == null)
            return NotFound("Görev bulunamadı");
            
        return Ok(task);
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
        return Ok(task);
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
        var task = await _context.Tasks.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (task == null)
            return NotFound("Görev bulunamadı");
            
        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(task);
    }
    
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetTasksByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var tasks = await _context.Tasks
            .Include(t => t.Assignments)
            .ThenInclude(a => a.User)
            .Include(t => t.TaskGroup)
            .Where(t => t.Assignments.Any(a => a.UserId == userId))
            .ToListAsync(cancellationToken);
            
        return Ok(tasks);
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
