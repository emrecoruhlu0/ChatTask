namespace ChatTask.Shared.DTOs;

public class CreateWorkspaceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CreatedById { get; set; }
}
