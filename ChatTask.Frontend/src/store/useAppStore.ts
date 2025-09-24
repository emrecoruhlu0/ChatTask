import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { User, Workspace, Conversation, Message, Task } from '../types';
import { authService } from '../services/auth';
import { signalRService } from '../services/signalr';

interface AppState {
  // User state
  user: User | null;
  isAuthenticated: boolean;

  // Workspace state
  workspaces: Workspace[];
  currentWorkspace: Workspace | null;

  // Conversation state
  currentConversation: Conversation | null;
  messages: Message[];

  // Task state
  tasks: Task[];
  userTasks: Task[];

  // UI state
  isLoading: boolean;
  isLoadingMessages: boolean;
  isLoadingTasks: boolean;
  sidebarCollapsed: boolean;
  activeView: 'chat' | 'tasks';

  // Actions
  setUser: (user: User | null) => void;
  setWorkspaces: (workspaces: Workspace[]) => void;
  setCurrentWorkspace: (workspace: Workspace | null) => void;
  setCurrentConversation: (conversation: Conversation | null) => void;
  setMessages: (messages: Message[]) => void;
  addMessage: (message: Message) => void;
  updateMessage: (messageId: string, updates: Partial<Message>) => void;
  setTasks: (tasks: Task[]) => void;
  setUserTasks: (tasks: Task[]) => void;
  addTask: (task: Task) => void;
  updateTask: (taskId: string, updates: Partial<Task>) => void;
  setLoading: (loading: boolean) => void;
  setLoadingMessages: (loading: boolean) => void;
  setLoadingTasks: (loading: boolean) => void;
  setSidebarCollapsed: (collapsed: boolean) => void;
  setActiveView: (view: 'chat' | 'tasks') => void;
  logout: () => void;
}

export const useAppStore = create<AppState>()(
  persist(
    (set) => ({
      // Initial state - localStorage'dan user bilgisini yükle
      user: (() => {
        try {
          const storedUser = localStorage.getItem('user');
          return storedUser ? JSON.parse(storedUser) : null;
        } catch {
          return null;
        }
      })(),
      isAuthenticated: (() => {
        try {
          const storedUser = localStorage.getItem('user');
          return !!storedUser;
        } catch {
          return false;
        }
      })(),
      workspaces: [],
      currentWorkspace: null,
      currentConversation: null,
      messages: [],
      isLoadingMessages: false,
      tasks: [],
      userTasks: [],
      isLoadingTasks: false,
      isLoading: false,
      sidebarCollapsed: false,
      activeView: 'chat',

      // Actions
      setUser: (user) => set({ 
        user, 
        isAuthenticated: !!user 
      }),

      setWorkspaces: (workspaces) => set({ 
        workspaces,
        currentWorkspace: workspaces.length > 0 ? workspaces[0] : null
      }),

      setCurrentWorkspace: (workspace) => set({ 
        currentWorkspace: workspace,
        currentConversation: null,
        messages: []
      }),

      setCurrentConversation: (conversation) => set({ 
        currentConversation: conversation,
        messages: []
      }),

      setMessages: (messages) => set({ messages }),

      addMessage: (message) => set((state) => ({
        messages: [...state.messages, message].sort(
          (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
        )
      })),

      updateMessage: (messageId, updates) => set((state) => ({
        messages: state.messages.map(msg => 
          msg.id === messageId ? { ...msg, ...updates } : msg
        )
      })),

      setTasks: (tasks) => set({ tasks }),

      setUserTasks: (userTasks) => set({ userTasks }),

      addTask: (task) => set((state) => ({
        tasks: [task, ...state.tasks],
        userTasks: task.assignments.some(a => a.userId === state.user?.id) 
          ? [task, ...state.userTasks] 
          : state.userTasks
      })),

      updateTask: (taskId, updates) => set((state) => ({
        tasks: state.tasks.map(task => 
          task.id === taskId ? { ...task, ...updates } : task
        ),
        userTasks: state.userTasks.map(task => 
          task.id === taskId ? { ...task, ...updates } : task
        )
      })),

      setLoading: (loading) => set({ isLoading: loading }),
      setLoadingMessages: (loading) => set({ isLoadingMessages: loading }),
      setLoadingTasks: (loading) => set({ isLoadingTasks: loading }),
      setSidebarCollapsed: (collapsed) => set({ sidebarCollapsed: collapsed }),
      setActiveView: (view) => set({ activeView: view }),

      logout: async () => {
        try {
          // SignalR bağlantısını kapat
          await signalRService.disconnect();
          
          // localStorage'dan kullanıcı bilgilerini temizle
          authService.logout();
          
          // Zustand store'u da temizle
          localStorage.removeItem('app-storage');
          
          // Store'u sıfırla
          set({
            user: null,
            isAuthenticated: false,
            workspaces: [],
            currentWorkspace: null,
            currentConversation: null,
            messages: [],
            tasks: [],
            userTasks: [],
            activeView: 'chat'
          });
        } catch (error) {
          console.error('Logout error:', error);
          // Hata olsa bile her şeyi temizle
          authService.logout();
          localStorage.removeItem('app-storage');
          set({
            user: null,
            isAuthenticated: false,
            workspaces: [],
            currentWorkspace: null,
            currentConversation: null,
            messages: [],
            tasks: [],
            userTasks: [],
            activeView: 'chat'
          });
        }
      },
    }),
    {
      name: 'app-storage',
      partialize: (state) => ({
        // Sadece user null değilse kaydet (logout durumunda kaydetme)
        ...(state.user && {
          user: state.user,
          isAuthenticated: state.isAuthenticated,
          currentWorkspace: state.currentWorkspace,
          currentConversation: state.currentConversation,
          workspaces: state.workspaces,
        }),
        // UI ayarları her zaman kaydet
        sidebarCollapsed: state.sidebarCollapsed,
        activeView: state.activeView,
      }),
    }
  )
);
