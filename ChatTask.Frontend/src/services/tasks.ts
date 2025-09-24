import { taskService } from './api';
import { Task, CreateTaskRequest } from '../types';

export const taskApiService = {
  async getAllTasks(userId?: string): Promise<Task[]> {
    const url = userId ? `/api/tasks?userId=${userId}` : '/api/tasks';
    const response = await taskService.get(url);
    return response.data;
  },

  async getTaskById(id: string): Promise<Task> {
    const response = await taskService.get(`/api/tasks/${id}`);
    return response.data;
  },

  async createTask(task: CreateTaskRequest): Promise<Task> {
    const response = await taskService.post('/api/tasks', task);
    return response.data;
  },

  async updateTask(id: string, task: Partial<Task>): Promise<Task> {
    const response = await taskService.put(`/api/tasks/${id}`, task);
    return response.data;
  },

  async updateTaskStatus(id: string, status: string): Promise<Task> {
    const response = await taskService.put(`/api/tasks/${id}/status`, { status });
    return response.data;
  },

  async getTasksByUserId(userId: string): Promise<Task[]> {
    const response = await taskService.get(`/api/tasks/user/${userId}`);
    return response.data;
  },

  // Delete task
  async deleteTask(taskId: string): Promise<void> {
    await taskService.delete(`/api/tasks/${taskId}`);
  },

  // Assign task to users
  async assignTaskToUsers(taskId: string, userIds: string[]): Promise<void> {
    await taskService.post(`/api/tasks/${taskId}/assign`, userIds);
  },

  // Get tasks by channel
  async getTasksByChannel(channelId: string): Promise<Task[]> {
    const response = await taskService.get(`/api/tasks/channel/${channelId}`);
    return response.data;
  },

  // Get tasks by user (assigned to user)
  async getTasksByUser(userId: string): Promise<Task[]> {
    const response = await taskService.get(`/api/tasks/user/${userId}`);
    return response.data;
  }
};
