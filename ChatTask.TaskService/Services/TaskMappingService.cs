using ChatTask.TaskService.Models;
using ChatTask.Shared.DTOs;

namespace ChatTask.TaskService.Services;

public class TaskMappingService
{
    // ProjectTask Model → DTO
    public TaskDto ToTaskDto(ProjectTask task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            DueDate = task.DueDate,
            TaskGroupId = task.TaskGroupId,
            AssignmentCount = task.Assignments.Count,
            Assignments = task.Assignments.Select(ToTaskAssignmentDto).ToList()
        };
    }

    // Task DTO → Model
    public ProjectTask ToTaskModel(CreateTaskDto dto)
    {
        return new ProjectTask
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            TaskGroupId = dto.TaskGroupId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // TaskAssignment Model → DTO
    public TaskAssignmentDto ToTaskAssignmentDto(TaskAssignment assignment)
    {
        return new TaskAssignmentDto
        {
            Id = assignment.Id,
            TaskId = assignment.TaskId,
            UserId = assignment.UserId,
            AssignedAt = assignment.AssignedAt,
            CompletedAt = assignment.CompletedAt,
            Status = assignment.Status
        };
    }

    // TaskAssignment DTO → Model
    public TaskAssignment ToTaskAssignmentModel(CreateTaskAssignmentDto dto)
    {
        return new TaskAssignment
        {
            Id = Guid.NewGuid(),
            TaskId = dto.TaskId,
            UserId = dto.UserId,
            AssignedAt = DateTime.UtcNow,
            Status = dto.Status
        };
    }

    // Update Task Model
    public void UpdateTaskModel(ProjectTask task, UpdateTaskDto dto)
    {
        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.UpdatedAt = DateTime.UtcNow;
    }

    // Update TaskAssignment Model
    public void UpdateTaskAssignmentModel(TaskAssignment assignment, UpdateTaskAssignmentDto dto)
    {
        assignment.Status = dto.Status;
        assignment.CompletedAt = dto.CompletedAt;
    }

    // List mappings
    public List<TaskDto> ToTaskDtoList(IEnumerable<ProjectTask> tasks)
    {
        return tasks.Select(ToTaskDto).ToList();
    }

    public List<TaskAssignmentDto> ToTaskAssignmentDtoList(IEnumerable<TaskAssignment> assignments)
    {
        return assignments.Select(ToTaskAssignmentDto).ToList();
    }
}
