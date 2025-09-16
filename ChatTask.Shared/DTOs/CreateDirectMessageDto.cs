namespace ChatTask.Shared.DTOs;

public class CreateDirectMessageDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPrivate { get; set; } = true;
    public List<Guid> ParticipantIds { get; set; } = new();
}
