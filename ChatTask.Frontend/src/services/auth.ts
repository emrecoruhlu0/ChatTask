import { userService } from './api';
import { User, LoginRequest, RegisterRequest } from '../types';

export const authService = {
  async login(credentials: LoginRequest): Promise<User> {
    const response = await userService.post('/api/users/validate', credentials);
    const user = response.data;
    
    // Kullanıcı bilgilerini localStorage'a kaydet
    localStorage.setItem('user', JSON.stringify(user));
    
    return user;
  },

  async register(userData: RegisterRequest): Promise<User> {
    const response = await userService.post('/api/users/register', userData);
    const user = response.data;
    
    // Kullanıcı bilgilerini localStorage'a kaydet
    localStorage.setItem('user', JSON.stringify(user));
    
    return user;
  },

  async getUserById(id: string): Promise<User> {
    const response = await userService.get(`/api/users/${id}`);
    return response.data;
  },

  // localStorage'dan kullanıcı bilgilerini al
  getStoredUser(): User | null {
    try {
      const storedUser = localStorage.getItem('user');
      return storedUser ? JSON.parse(storedUser) : null;
    } catch (error) {
      console.error('Error parsing stored user:', error);
      return null;
    }
  },

  // Kullanıcıyı localStorage'dan sil (logout)
  logout(): void {
    localStorage.removeItem('user');
  },

  // Kullanıcı giriş yapmış mı kontrol et
  isAuthenticated(): boolean {
    return this.getStoredUser() !== null;
  }
};
