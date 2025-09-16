// User Types
export interface User {
  id: string;
  name: string;
  email: string;
  avatar: string;
  status: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

// Workspace Types
export interface Workspace {
  id: string;
  name: string;
  description: string;
  memberCount: number;
  conversations: Conversation[];
}

// Conversation Types
export interface Conversation {
  id: string;
  name: string;
  description: string;
  type: 'Channel' | 'DirectMessage' | 'Group';
  isPrivate: boolean;
  memberCount: number;
  lastMessage: string;
}

// Message Types
export interface Message {
  id: string;
  conversationId: string;
  senderId: string;
  content: string;
  createdAt: string;
  isRead: boolean;
  threadId?: string;
  senderName: string;
}

export interface SendMessageRequest {
  senderId: string;
  content: string;
  threadId?: string;
}

// Task Types
export interface Task {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  createdAt: string;
  updatedAt: string;
  dueDate: string;
  taskGroupId?: string;
  assignmentCount: number;
  assignments: TaskAssignment[];
}

export interface TaskAssignment {
  taskId: string;
  userId: string;
  assignedAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate: string;
  taskGroupId?: string;
  assigneeIds?: string[];
}

// Enums
export type TaskStatus = 'New' | 'InProgress' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Urgent';

// API Response Types
export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  total: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

// DTO Types
export interface CreateDirectMessageDto {
  name: string;
  description: string;
  isPrivate: boolean;
  participantIds: string[];
}

export interface MarkMessageAsReadDto {
  userId: string;
}
