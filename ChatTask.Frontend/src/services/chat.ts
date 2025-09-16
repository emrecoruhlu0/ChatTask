import { chatService } from './api';
import { Message, SendMessageRequest, CreateDirectMessageDto } from '../types';

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

  // Channel management
  async createChannel(channel: {
    name: string;
    description: string;
    isPrivate: boolean;
    workspaceId: string;
    memberIds: string[];
  }): Promise<any> {
    const response = await chatService.post('/api/conversations/channels', channel);
    return response.data;
  },

  // Direct Messages
  async createDirectMessage(dm: CreateDirectMessageDto): Promise<any> {
    const response = await chatService.post('/api/conversations/direct-messages', dm);
    return response.data;
  }
};
