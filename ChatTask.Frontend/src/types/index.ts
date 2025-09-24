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
  type: 'Channel' | 'DirectMessage' | 'Group' | 'TaskGroup';
  memberCount: number;
  lastMessage: string;
  createdById: string;
  isPublic?: boolean; // Channel için
}

// Member Types
export interface ConversationMember {
  userId: string;
  name: string;
  email: string;
  avatar: string;
  role: 'Owner' | 'Admin' | 'Member';
  joinedAt: string;
  isCurrentUser: boolean;
  canChangeRole: boolean;
}

// Role Types
export type MemberRole = 'Owner' | 'Admin' | 'Member';

// Create Request Types
export interface CreateDirectMessageDto {
  name: string;
  description: string;
  workspaceId: string;
  participantIds: string[];
}

// Role Change Types
export interface ChangeRoleDto {
  requestedBy: string;
  role: MemberRole;
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
  isPrivate: boolean;
  createdById: string;
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
  isPrivate: boolean;
  createdById: string;
  taskGroupId?: string;
  channelId?: string;
  AssigneeIds?: string[];
}

// Enums
export type TaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled' | 'OnHold';
export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Critical';

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

// DTO Types (duplicate removed)

export interface MarkMessageAsReadDto {
  userId: string;
}
