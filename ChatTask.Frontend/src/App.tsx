import React, { useEffect, useState } from 'react';
import { Toaster } from "react-hot-toast";
import { TooltipProvider } from "@radix-ui/react-tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Login } from './components/auth/Login';
// import { Sidebar } from './components/layout/Sidebar';
import { ChatArea } from './components/chat/ChatArea';
import { TaskManager } from './components/tasks/TaskManager';
import { UserTaskPanel } from './components/tasks/UserTaskPanel';
import { WorkspaceManager } from './components/workspace/WorkspaceManager';
import { ConversationManager } from './components/conversation/ConversationManager';
import { Button } from './components/ui/button';
import { MessageSquare, CheckSquare, User, LogOut } from 'lucide-react';
import { useAppStore } from './store/useAppStore';
import { signalRService } from './services/signalr';
import { chatApiService } from './services/chat';
import { authService } from './services/auth';
import NotFound from "./pages/NotFound";
import toast from 'react-hot-toast';
import './utils/emergencyLogout';

const queryClient = new QueryClient();

const MainLayout: React.FC = () => {
  const { 
    user, 
    setUser,
    activeView, 
    setActiveView,
    setWorkspaces,
    addMessage,
    currentWorkspace,
    currentConversation,
    logout
  } = useAppStore();

  const [isTaskDialogOpen, setIsTaskDialogOpen] = useState(false);
  const [isUserTaskPanelOpen, setIsUserTaskPanelOpen] = useState(false);
  const [channelMembers, setChannelMembers] = useState<any[]>([]);
  const [workspaceConversations, setWorkspaceConversations] = useState<any[]>([]);

  // Sayfa yüklendiğinde localStorage'dan kullanıcı bilgilerini geri yükle
  useEffect(() => {
    const storedUser = authService.getStoredUser();
    if (storedUser && !user) {
      setUser(storedUser);
    }
  }, []);

  useEffect(() => {
    if (user) {
      initializeConnections();
    }

    return () => {
      signalRService.disconnect();
    };
  }, [user]);

  // Channel member'larını yükle
  useEffect(() => {
    const loadChannelMembers = async () => {
      console.log('App: loadChannelMembers called, currentConversation:', currentConversation);
      if (currentConversation && (
        currentConversation.type === 'Channel' || 
        String(currentConversation.type) === '1'
      )) {
        console.log('App: Loading members for channel:', currentConversation.id);
        try {
          const members = await chatApiService.getConversationMembers(currentConversation.id, user?.id || '');
          console.log('App: Loaded channel members:', members);
          setChannelMembers(members);
        } catch (error) {
          console.error('Failed to load channel members:', error);
          setChannelMembers([]);
        }
      } else {
        console.log('App: Not a channel, clearing members');
        setChannelMembers([]);
      }
    };

    loadChannelMembers();
  }, [currentConversation]);

  useEffect(() => {
    const loadWorkspaceConversations = async () => {
      if (currentWorkspace && user) {
        try {
          const conversations = await chatApiService.getWorkspaceConversations(currentWorkspace.id, user.id);
          // Sadece channel'ları filtrele
          const channels = conversations.filter((conv: any) => 
            conv.type === 'Channel' || conv.type === 1
          );
          setWorkspaceConversations(channels);
        } catch (error) {
          console.error('Failed to load workspace conversations:', error);
          setWorkspaceConversations([]);
        }
      } else {
        setWorkspaceConversations([]);
      }
    };

    loadWorkspaceConversations();
  }, [currentWorkspace, user]);

  const initializeConnections = async () => {
    try {
      // Connect to SignalR
      await signalRService.connect();
      
      // Listen for workspace updates  
      signalRService.onUserWorkspaces((workspaces) => {
        console.log('📨 Received workspaces:', workspaces);
        setWorkspaces(workspaces);
        if (workspaces.length > 0) {
          toast.success(`Connected! Found ${workspaces.length} workspaces`);
        }
      });

      // Listen for new messages
      signalRService.onNewMessage((message) => {
        console.log('📨 New message received:', message);
        addMessage(message);
        toast.success('New message received');
      });

      // Listen for message read events
      signalRService.onMessageRead((data) => {
        console.log('📨 Message read event:', data);
      });

      // Trigger login notification to get workspaces
      await chatApiService.notifyUserLogin(user!.id);
      
    } catch (error) {
      console.error('Failed to initialize connections:', error);
      toast.error('Failed to connect to chat services');
    }
  };

  if (!user) {
    return <Login />;
  }

  return (
    <div className="flex h-screen bg-background">
      {/* Left Panel - Navigation */}
      <div className="w-16 bg-slate-900 flex flex-col items-center py-4 space-y-4 overflow-y-auto">
        <Button
          variant={activeView === 'chat' ? 'default' : 'ghost'}
          size="sm"
          onClick={() => setActiveView('chat')}
          className={`w-12 h-12 p-0 ${activeView === 'chat' ? '' : 'text-blue-300 hover:text-blue-100 hover:bg-blue-800/30'}`}
        >
          <MessageSquare className="w-5 h-5" />
        </Button>
        <Button
          variant={activeView === 'tasks' ? 'default' : 'ghost'}
          size="sm"
          onClick={() => setActiveView('tasks')}
          className={`w-12 h-12 p-0 ${activeView === 'tasks' ? '' : 'text-green-300 hover:text-green-100 hover:bg-green-800/30'}`}
        >
          <CheckSquare className="w-5 h-5" />
        </Button>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => setIsUserTaskPanelOpen(true)}
          className="w-12 h-12 p-0 text-purple-300 hover:text-purple-100 hover:bg-purple-800/30"
          title="My Tasks"
        >
          <User className="w-5 h-5" />
        </Button>
        
        {/* Spacer */}
        <div className="flex-1" />
        
        {/* User Info & Logout */}
        <div className="flex flex-col items-center space-y-2">
          <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center text-white text-xs font-semibold">
            {user?.name?.charAt(0).toUpperCase() || 'U'}
          </div>
          <Button
            variant="ghost"
            size="sm"
            onClick={async () => {
              try {
                await logout();
                toast.success('Logged out successfully');
                // Sayfayı yenile ki temiz state ile başlasın
                window.location.reload();
              } catch (error) {
                console.error('Logout failed:', error);
                toast.error('Logout failed, please try again');
              }
            }}
            className="w-12 h-12 p-0 text-red-400 hover:text-red-300 hover:bg-red-900/20"
            title="Logout"
          >
            <LogOut className="w-5 h-5" />
          </Button>
        </div>
      </div>

      {/* Middle Panel - Lists */}
      <div className="w-80 bg-slate-50 border-r border-border flex flex-col overflow-hidden">
        <div className="flex-1 overflow-y-auto">
          {activeView === 'chat' ? (
            currentWorkspace ? (
              <ConversationManager 
                onCreateTaskFromChannel={() => {
                  setActiveView('tasks');
                  setIsTaskDialogOpen(true);
                }}
              />
            ) : <WorkspaceManager />
          ) : (
            <div className="p-4">
              <h2 className="text-lg font-semibold mb-4">Task Lists</h2>
              {/* Task categories could go here */}
            </div>
          )}
        </div>
        
        {/* User Info Panel - Bottom */}
        {user && (
          <div className="border-t border-border p-4 bg-white">
            <div className="flex items-center space-x-3">
              <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center text-white text-sm font-medium">
                {user.name?.charAt(0).toUpperCase() || 'U'}
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-gray-900 truncate">{user.name}</p>
                <p className="text-xs text-gray-500 truncate">{user.email}</p>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Right Panel - Content */}
      <div className="flex-1 flex flex-col">
        {activeView === 'chat' ? (
          currentConversation ? <ChatArea /> : (
            <div className="flex-1 flex items-center justify-center text-muted-foreground">
              <div className="text-center">
                <MessageSquare className="w-16 h-16 mx-auto mb-4 opacity-50" />
                <p className="text-lg">Select a conversation to start chatting</p>
              </div>
            </div>
          )
        ) : (
          <TaskManager 
            channelContext={(currentConversation?.type === 'Channel' || String(currentConversation?.type) === '1') ? {
              id: currentConversation!.id,
              name: currentConversation!.name,
              members: channelMembers
            } : undefined}
            availableChannels={workspaceConversations}
            openCreateDialog={isTaskDialogOpen}
            onCreateDialogClose={() => setIsTaskDialogOpen(false)}
          />
        )}
      </div>

      {/* User Task Panel */}
      <UserTaskPanel 
        isOpen={isUserTaskPanelOpen}
        onClose={() => setIsUserTaskPanelOpen(false)}
      />
    </div>
  );
};

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<MainLayout />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
        <Toaster position="top-right" />
        
        {/* Development: User Switching Panel */}
        {import.meta.env.DEV && (
          <div className="fixed top-4 right-4 bg-white border border-gray-300 rounded-lg p-4 shadow-lg z-50 max-w-xs">
            <h3 className="font-semibold mb-2">Dev: Multi-User Test</h3>
            <div className="space-y-2">
              <button
                onClick={() => {
                  localStorage.clear();
                  sessionStorage.clear();
                  window.location.reload();
                }}
                className="w-full bg-red-500 text-white px-3 py-1 rounded text-sm hover:bg-red-600"
              >
                Clear & Reload
              </button>
              <button
                onClick={() => {
                  window.open(window.location.href, '_blank');
                }}
                className="w-full bg-blue-500 text-white px-3 py-1 rounded text-sm hover:bg-blue-600"
              >
                New Tab
              </button>
              <div className="text-xs text-gray-600">
                <p>• Use different browsers</p>
                <p>• Use Chrome profiles</p>
                <p>• Manual localStorage clear</p>
              </div>
            </div>
          </div>
        )}
      </BrowserRouter>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
