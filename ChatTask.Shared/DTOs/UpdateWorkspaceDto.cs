

namespace ChatTask.Shared.DTOs;

public class UpdateWorkspaceDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public Guid UpdatedById { get; set; }
}