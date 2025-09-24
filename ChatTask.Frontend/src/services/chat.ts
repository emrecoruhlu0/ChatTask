import { chatService } from './api';
import { Message, SendMessageRequest, CreateDirectMessageDto, ConversationMember, ChangeRoleDto } from '../types';

export const chatApiService = {
  // User login notification
  async notifyUserLogin(userId: string): Promise<any> {
    const response = await chatService.post(`/api/conversations/users/${userId}/login`);
    return response.data;
  },

  // Messages
  async getMessages(conversationId: string, page = 1, pageSize = 50): Promise<Message[]> {
    const response = await chatService.get(
      `/api/conversations/${conversationId}/messages?page=${page}&pageSize=${pageSize}`
    );
    return response.data;
  },

  async sendMessage(conversationId: string, message: SendMessageRequest): Promise<Message> {
    const response = await chatService.post(`/api/conversations/${conversationId}/messages`, message);
    return response.data;
  },

  async markMessageAsRead(messageId: string, userId: string): Promise<void> {
    await chatService.put(`/api/conversations/messages/${messageId}/read`, { userId });
  },

  // Workspace management
  async createWorkspace(workspace: {
    name: string;
    description: string;
    createdById: string;
  }): Promise<any> {
    const response = await chatService.post('/api/conversations/workspaces', workspace);
    return response.data;
  },

  async getAllWorkspaces(userId: string): Promise<any[]> {
    const response = await chatService.get(`/api/conversations/workspaces?userId=${userId}`);
    return response.data;
  },

  async updateWorkspace(workspaceId: string, data: {
    name?: string;
    description?: string;
    isActive?: boolean;
    updatedById: string;
  }): Promise<any> {
    const response = await chatService.put(`/api/conversations/workspaces/${workspaceId}`, data);
    return response.data;
  },

  async deleteWorkspace(workspaceId: string, deletedById: string): Promise<void> {
    await chatService.delete(`/api/conversations/workspaces/${workspaceId}`, { data: deletedById });
  },

  async addWorkspaceMember(workspaceId: string, data: {
    userId: string;
    addedById: string;
    role?: string;
  }): Promise<void> {
    await chatService.post(`/api/conversations/workspaces/${workspaceId}/members`, data);
  },

  async removeWorkspaceMember(workspaceId: string, userId: string, removedById: string): Promise<void> {
    await chatService.delete(`/api/conversations/workspaces/${workspaceId}/members/${userId}`, { data: removedById });
  },

  // Channel management
  async createChannel(channel: {
    name: string;
    description: string;
    isPublic: boolean;
    workspaceId: string;
    createdById: string;
    memberIds: string[];
  }): Promise<any> {
    // Backend'de InitialMemberIds bekleniyor
    const payload = {
      name: channel.name,
      description: channel.description,
      isPublic: channel.isPublic,
      createdById: channel.createdById,
      initialMemberIds: channel.memberIds // memberIds -> initialMemberIds
    };
    const response = await chatService.post(`/api/conversations/workspaces/${channel.workspaceId}/channels`, payload);
    return response.data;
  },

  // Direct Messages
  async createDirectMessage(dm: CreateDirectMessageDto): Promise<any> {
    const response = await chatService.post(`/api/conversations/workspaces/${dm.workspaceId}/direct-messages`, dm);
    return response.data;
  },

  // User management
  async getAllUsers(): Promise<any[]> {
    const response = await chatService.get('/api/conversations/users');
    return response.data;
  },

  // Get workspace members
  async getWorkspaceMembers(workspaceId: string): Promise<any[]> {
    const response = await chatService.get(`/api/conversations/workspaces/${workspaceId}/members`);
    return response.data;
  },

  // Get conversation members
  async getConversationMembers(conversationId: string, userId: string): Promise<ConversationMember[]> {
    const response = await chatService.get(`/api/conversations/conversations/${conversationId}/members?userId=${userId}`);
    return response.data;
  },

  // Change member role
  async changeMemberRole(conversationId: string, userId: string, roleChange: ChangeRoleDto): Promise<any> {
    const response = await chatService.put(`/api/conversations/conversations/${conversationId}/members/${userId}/role`, roleChange);
    return response.data;
  },

  // Browse conversations (not member of)
  async browseConversations(workspaceId: string, userId: string, type?: string): Promise<any[]> {
    const url = `/api/conversations/workspaces/${workspaceId}/browse?userId=${userId}${type ? `&type=${type}` : ''}`;
    const response = await chatService.get(url);
    return response.data;
  },

  // Get workspace conversations
  async getWorkspaceConversations(workspaceId: string, userId: string): Promise<any[]> {
    const response = await chatService.get(`/api/conversations/workspaces/${workspaceId}?userId=${userId}`);
    return response.data;
  },

  // Create group
  async createGroup(group: {
    name: string;
    description: string;
    workspaceId: string;
    createdById: string;
    memberIds: string[];
  }): Promise<any> {
    // Backend'de InitialMemberIds bekleniyor
    const payload = {
      name: group.name,
      description: group.description,
      createdById: group.createdById,
      initialMemberIds: group.memberIds // memberIds -> initialMemberIds
    };
    const response = await chatService.post(`/api/conversations/workspaces/${group.workspaceId}/groups`, payload);
    return response.data;
  }
};
