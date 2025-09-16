import React, { useEffect } from 'react';
import { Toaster } from "react-hot-toast";
import { TooltipProvider } from "@radix-ui/react-tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Login } from './components/auth/Login';
import { Sidebar } from './components/layout/Sidebar';
import { ChatArea } from './components/chat/ChatArea';
import { TaskManager } from './components/tasks/TaskManager';
import { useAppStore } from './store/useAppStore';
import { signalRService } from './services/signalr';
import { chatApiService } from './services/chat';
import NotFound from "./pages/NotFound";
import toast from 'react-hot-toast';

const queryClient = new QueryClient();

const MainLayout: React.FC = () => {
  const { 
    user, 
    activeView, 
    setWorkspaces,
    addMessage
  } = useAppStore();

  useEffect(() => {
    if (user) {
      initializeConnections();
    }

    return () => {
      signalRService.disconnect();
    };
  }, [user]);

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
      <Sidebar />
      <div className="flex-1 flex flex-col">
        {activeView === 'chat' ? <ChatArea /> : <TaskManager />}
      </div>
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
      </BrowserRouter>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
