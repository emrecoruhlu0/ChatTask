import React from 'react';
import { Button } from '../ui/button';
import { useAppStore } from '../../store/useAppStore';
import { 
  MessageSquare, 
  CheckSquare, 
  Menu, 
  X, 
  Hash, 
  User, 
  Plus,
  Settings,
  LogOut,
  Users
} from 'lucide-react';
import { cn } from '../../lib/utils';

export const Sidebar: React.FC = () => {
  const {
    workspaces,
    currentWorkspace,
    currentConversation,
    activeView,
    sidebarCollapsed,
    setCurrentWorkspace,
    setCurrentConversation,
    setActiveView,
    setSidebarCollapsed,
    logout
  } = useAppStore();

  const handleWorkspaceSelect = (workspace: any) => {
    setCurrentWorkspace(workspace);
    if (workspace.conversations.length > 0) {
      setCurrentConversation(workspace.conversations[0]);
    }
  };

  const handleConversationSelect = (conversation: any) => {
    setCurrentConversation(conversation);
    setActiveView('chat');
  };

  return (
    <div className={cn(
      "flex flex-col h-full bg-sidebar-background border-r border-sidebar-border transition-all duration-300",
      sidebarCollapsed ? "w-16" : "w-80"
    )}>
      {/* Header */}
      <div className="flex items-center justify-between p-4 border-b border-sidebar-border">
        {!sidebarCollapsed && (
          <div className="flex items-center space-x-2">
            <MessageSquare className="h-6 w-6 text-sidebar-primary" />
            <span className="font-semibold text-sidebar-foreground">ChatTask</span>
          </div>
        )}
        <Button
          variant="ghost"
          size="icon"
          onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
          className="text-sidebar-foreground hover:bg-sidebar-accent"
        >
          {sidebarCollapsed ? <Menu className="h-4 w-4" /> : <X className="h-4 w-4" />}
        </Button>
      </div>

      {/* Navigation */}
      {!sidebarCollapsed && (
        <div className="p-4 space-y-2">
          <Button
            variant={activeView === 'chat' ? 'default' : 'ghost'}
            className="w-full justify-start"
            onClick={() => setActiveView('chat')}
          >
            <MessageSquare className="mr-2 h-4 w-4" />
            Chat
          </Button>
          <Button
            variant={activeView === 'tasks' ? 'default' : 'ghost'}
            className="w-full justify-start"
            onClick={() => setActiveView('tasks')}
          >
            <CheckSquare className="mr-2 h-4 w-4" />
            Tasks
          </Button>
        </div>
      )}

      {/* Workspaces & Conversations */}
      <div className="flex-1 overflow-y-auto p-4">
        {!sidebarCollapsed && (
          <div className="space-y-4">
            {workspaces.length === 0 ? (
              <div className="text-center py-8">
                <Users className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                <h3 className="font-medium text-sidebar-foreground mb-2">
                  No workspaces found
                </h3>
                <p className="text-sm text-muted-foreground">
                  Connect to see your teams
                </p>
              </div>
            ) : (
              workspaces.map((workspace) => (
                <div key={workspace.id} className="space-y-2">
                  <Button
                    variant="ghost"
                    onClick={() => handleWorkspaceSelect(workspace)}
                    className={cn(
                      "w-full text-left p-3 rounded-lg transition-colors workspace-item",
                      currentWorkspace?.id === workspace.id 
                        ? "bg-sidebar-primary text-sidebar-primary-foreground" 
                        : "hover:bg-sidebar-accent text-sidebar-foreground"
                    )}
                  >
                    <div className="flex items-center justify-between w-full">
                      <div className="flex-1 min-w-0">
                        <div className="font-medium truncate">{workspace.name}</div>
                        {workspace.description && (
                          <div className="text-xs opacity-75 truncate">{workspace.description}</div>
                        )}
                      </div>
                      <div className="text-xs bg-sidebar-accent px-2 py-1 rounded-full">
                        {workspace.memberCount}
                      </div>
                    </div>
                  </Button>

                  {/* Conversations for current workspace */}
                  {currentWorkspace?.id === workspace.id && workspace.conversations && (
                    <div className="ml-4 space-y-1">
                      {workspace.conversations.map((conversation) => (
                        <Button
                          key={conversation.id}
                          variant="ghost"
                          onClick={() => handleConversationSelect(conversation)}
                          className={cn(
                            "w-full text-left p-2 rounded-md transition-colors flex items-center space-x-2",
                            currentConversation?.id === conversation.id
                              ? "bg-sidebar-primary/20 text-sidebar-primary"
                              : "hover:bg-sidebar-accent/50 text-sidebar-foreground/80"
                          )}
                        >
                          {conversation.type === 'Channel' ? (
                            <Hash className="h-4 w-4 flex-shrink-0" />
                          ) : (
                            <User className="h-4 w-4 flex-shrink-0" />
                          )}
                          <span className="truncate">{conversation.name}</span>
                          {conversation.memberCount && (
                            <span className="text-xs bg-sidebar-accent/50 px-1.5 py-0.5 rounded-full ml-auto">
                              {conversation.memberCount}
                            </span>
                          )}
                        </Button>
                      ))}
                      <Button
                        variant="ghost"
                        size="sm"
                        className="w-full text-left p-2 text-muted-foreground hover:text-sidebar-foreground"
                      >
                        <Plus className="h-4 w-4 mr-2" />
                        Add Channel
                      </Button>
                    </div>
                  )}
                </div>
              ))
            )}
          </div>
        )}
      </div>

      {/* Footer */}
      {!sidebarCollapsed && (
        <>
          <div className="border-t border-sidebar-border p-4 space-y-2">
            <Button variant="ghost" className="w-full justify-start">
              <Settings className="mr-2 h-4 w-4" />
              Settings
            </Button>
            <Button variant="ghost" className="w-full justify-start text-destructive hover:text-destructive" onClick={logout}>
              <LogOut className="mr-2 h-4 w-4" />
              Sign Out
            </Button>
          </div>
        </>
      )}
    </div>
  );
};
