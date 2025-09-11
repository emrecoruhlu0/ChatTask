using ChatTask.Shared.Enums;

namespace ChatTask.Shared.DTOs;

public class CreateConversationDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ConversationType Type { get; set; }
    public bool IsPrivate { get; set; } = false;
    public List<Guid> MemberIds { get; set; } = new();
    public List<Guid> InitialMemberIds { get; set; } = new(); // Channel için
    public Guid CreatedById { get; set; }
    
    // Spesifik özellikler (opsiyonel)
    public string? Topic { get; set; }           // Channel için
    public ChannelPurpose? ChannelPurpose { get; set; }
    public GroupPurpose? GroupPurpose { get; set; }
    public DateTime? ExpiresAt { get; set; }     // Group için
    public bool? AutoAddNewMembers { get; set; } // Channel için
}

