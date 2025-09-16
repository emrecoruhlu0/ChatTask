import axios from 'axios';
import toast from 'react-hot-toast';

// API Configuration
const config = {
  userServiceUrl: import.meta.env.VITE_USER_SERVICE_URL || 'http://localhost:5001',
  chatServiceUrl: import.meta.env.VITE_CHAT_SERVICE_URL || 'http://localhost:5002',
  taskServiceUrl: import.meta.env.VITE_TASK_SERVICE_URL || 'http://localhost:5004',
  signalRUrl: import.meta.env.VITE_SIGNALR_URL || 'http://localhost:5002/chat-hub',
};

// Create axios instances
export const userService = axios.create({
  baseURL: config.userServiceUrl,
  timeout: 10000,
});

export const chatService = axios.create({
  baseURL: config.chatServiceUrl,
  timeout: 10000,
});

export const taskService = axios.create({
  baseURL: config.taskServiceUrl,
  timeout: 10000,
});

// Setup interceptors for logging and error handling
const setupInterceptors = (instance: any, serviceName: string) => {
  instance.interceptors.request.use(
    (config: any) => {
      console.log(`🚀 ${serviceName} Request:`, config.method?.toUpperCase(), config.url);
      return config;
    },
    (error: any) => {
      console.error(`❌ ${serviceName} Request Error:`, error);
      return Promise.reject(error);
    }
  );

  instance.interceptors.response.use(
    (response: any) => {
      console.log(`✅ ${serviceName} Response:`, response.status, response.config.url);
      return response;
    },
    (error: any) => {
      console.error(`❌ ${serviceName} Response Error:`, error.response?.status, error.response?.data);
      
      // Handle common errors
      if (error.response?.status === 401) {
        toast.error('Authentication failed');
      } else if (error.response?.status === 403) {
        toast.error('Access denied');
      } else if (error.response?.status === 404) {
        toast.error('Resource not found');
      } else if (error.response?.status >= 500) {
        toast.error('Server error occurred');
      } else if (error.code === 'NETWORK_ERROR') {
        toast.error(`Cannot connect to ${serviceName}`);
      }
      
      return Promise.reject(error);
    }
  );
};

// Setup interceptors for all services
setupInterceptors(userService, 'UserService');
setupInterceptors(chatService, 'ChatService');
setupInterceptors(taskService, 'TaskService');

export { config };
