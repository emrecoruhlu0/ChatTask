import { taskService } from './api';
import { Task, CreateTaskRequest } from '../types';

export const taskApiService = {
  async getAllTasks(): Promise<Task[]> {
    const response = await taskService.get('/api/tasks');
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
  }
};
