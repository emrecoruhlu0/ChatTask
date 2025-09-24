using ChatTask.ChatService.Models;
using ChatTask.Shared.DTOs;

namespace ChatTask.ChatService.Services;

public class ChatMappingService
{
    // Workspace Model → DTO
    public WorkspaceDto ToWorkspaceDto(Workspace workspace)
    {
        return new WorkspaceDto
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Description = workspace.Description,
            Domain = workspace.Domain,
            CreatedById = workspace.CreatedById,
            IsActive = workspace.IsActive,
            CreatedAt = workspace.CreatedAt,
            MemberCount = workspace.Members.Count,
            ConversationCount = workspace.Conversations.Count
        };
    }

    // Workspace DTO → Model
    public Workspace ToWorkspaceModel(CreateWorkspaceDto dto)
    {
        return new Workspace
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Domain = dto.Domain,
            CreatedById = dto.CreatedById,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Conversation Model → DTO
    public ConversationDto ToConversationDto(Conversation conversation)
    {
        return new ConversationDto
        {
            Id = conversation.Id,
            WorkspaceId = conversation.WorkspaceId,
            Name = conversation.Name,
            Description = conversation.Description,
            Type = conversation.Type,
            CreatedById = conversation.CreatedById,
            CreatedAt = conversation.CreatedAt,
            IsArchived = conversation.IsArchived,
            MemberCount = conversation.Members.Count,
            MessageCount = conversation.Messages.Count
        };
    }

    // Conversation DTO → Model
    public Conversation ToConversationModel(CreateConversationDto dto)
    {
        return dto.Type switch
        {
            Shared.Enums.ConversationType.Channel => new Channel
            {
                Id = Guid.NewGuid(),
                WorkspaceId = dto.WorkspaceId,
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                CreatedById = dto.CreatedById,
                CreatedAt = DateTime.UtcNow,
                IsArchived = false
            },
            Shared.Enums.ConversationType.Group => new Group
            {
                Id = Guid.NewGuid(),
                WorkspaceId = dto.WorkspaceId,
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                CreatedById = dto.CreatedById,
                CreatedAt = DateTime.UtcNow,
                IsArchived = false
            },
            Shared.Enums.ConversationType.DirectMessage => new DirectMessage
            {
                Id = Guid.NewGuid(),
                WorkspaceId = dto.WorkspaceId,
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                CreatedById = dto.CreatedById,
                CreatedAt = DateTime.UtcNow,
                IsArchived = false
            },
            Shared.Enums.ConversationType.TaskGroup => new TaskGroup
            {
                Id = Guid.NewGuid(),
                WorkspaceId = dto.WorkspaceId,
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                CreatedById = dto.CreatedById,
                CreatedAt = DateTime.UtcNow,
                IsArchived = false,
                TaskCount = 0
            },
            _ => throw new ArgumentException($"Unsupported conversation type: {dto.Type}")
        };
    }

    // Member Model → DTO
    public MemberDto ToMemberDto(Member member)
    {
        return new MemberDto
        {
            UserId = member.UserId,
            ParentId = member.ParentId,
            ParentType = member.ParentType,
            Role = member.Role,
            JoinedAt = member.JoinedAt,
            IsActive = member.IsActive
        };
    }

    // Member DTO → Model (Artık kullanılmıyor - controller'da manuel oluşturuluyor)
    public Member ToMemberModel(CreateMemberDto dto, Guid parentId, ChatTask.Shared.Enums.ParentType parentType)
    {
        return new Member
        {
            UserId = Guid.Parse(dto.UserId),
            ParentId = parentId,
            ParentType = parentType,
            Role = Enum.TryParse<ChatTask.Shared.Enums.MemberRole>(dto.Role, out var role) ? role : ChatTask.Shared.Enums.MemberRole.Member,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    // List mappings
    public List<WorkspaceDto> ToWorkspaceDtoList(IEnumerable<Workspace> workspaces)
    {
        return workspaces.Select(ToWorkspaceDto).ToList();
    }

    public List<ConversationDto> ToConversationDtoList(IEnumerable<Conversation> conversations)
    {
        return conversations.Select(ToConversationDto).ToList();
    }

    public List<MemberDto> ToMemberDtoList(IEnumerable<Member> members)
    {
        return members.Select(ToMemberDto).ToList();
    }
}
