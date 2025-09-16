import { userService } from './api';
import { User, LoginRequest, RegisterRequest } from '../types';

export const authService = {
  async login(credentials: LoginRequest): Promise<User> {
    const response = await userService.post('/api/users/validate', credentials);
    return response.data;
  },

  async register(userData: RegisterRequest): Promise<User> {
    const response = await userService.post('/api/users/register', userData);
    return response.data;
  },

  async getUserById(id: string): Promise<User> {
    const response = await userService.get(`/api/users/${id}`);
    return response.data;
  }
};
