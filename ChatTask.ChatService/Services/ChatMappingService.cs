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
            IsPrivate = conversation.IsPrivate,
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
                IsPrivate = dto.IsPrivate,
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
                IsPrivate = dto.IsPrivate,
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
                IsPrivate = dto.IsPrivate,
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
                IsPrivate = dto.IsPrivate,
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
            Id = member.Id,
            UserId = member.UserId,
            ParentId = member.ParentId,
            Role = member.Role,
            JoinedAt = member.JoinedAt,
            IsActive = member.IsActive
        };
    }

    // Member DTO → Model
    public Member ToMemberModel(CreateMemberDto dto)
    {
        return new Member
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            ParentId = dto.ParentId,
            Role = dto.Role,
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
