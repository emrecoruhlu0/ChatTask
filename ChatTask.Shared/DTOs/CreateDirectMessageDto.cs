namespace ChatTask.Shared.DTOs;

public class CreateDirectMessageDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; }
    public Guid CreatedById { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}
