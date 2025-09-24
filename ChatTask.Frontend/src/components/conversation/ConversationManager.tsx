import React, { useState, useEffect } from 'react';
import { Button } from '../ui/button';
import { UserSelector } from '../common/UserSelector';
// import { ChatArea } from '../chat/ChatArea';
import { useAppStore } from '../../store/useAppStore';
import { chatApiService } from '../../services/chat';
import { taskApiService } from '../../services/tasks';
import { Plus, Hash, Users, MessageSquare, Info, Trash2, UserPlus, CheckSquare, ArrowLeft } from 'lucide-react';
import toast from 'react-hot-toast';

interface Conversation {
  id: string;
  name: string;
  description: string;
  type: string | number; // Backend'den enum gelebilir
  isPrivate: boolean;
  memberCount: number;
  lastMessage?: string;
}

interface CreateConversationDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConversationCreated: (conversation: Conversation) => void;
  workspaceId: string;
  type: 'channel' | 'group' | 'direct-message';
}

const CreateConversationDialog: React.FC<CreateConversationDialogProps> = ({
  isOpen,
  onClose,
  onConversationCreated,
  workspaceId,
  type
}) => {
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    isPrivate: false
  });
  const [selectedUsers, setSelectedUsers] = useState<any[]>([]);
  const [isUserSelectorOpen, setIsUserSelectorOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const { user } = useAppStore();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;

    setIsLoading(true);
    try {
      let conversation;
      
      if (type === 'channel') {
        conversation = await chatApiService.createChannel({
          name: formData.name,
          description: formData.description,
          isPublic: !formData.isPrivate,
          workspaceId,
          createdById: user.id,
          memberIds: selectedUsers.map(u => u.id)
        });
      } else if (type === 'group') {
        conversation = await chatApiService.createGroup({
          name: formData.name,
          description: formData.description,
          workspaceId,
          createdById: user.id,
          memberIds: selectedUsers.map(u => u.id)
        });
      } else if (type === 'direct-message') {
        if (selectedUsers.length !== 1) {
          toast.error('Direct message requires exactly one user');
          return;
        }
        conversation = await chatApiService.createDirectMessage({
          name: formData.name,
          description: formData.description,
          workspaceId: workspaceId,
          participantIds: [user.id, selectedUsers[0].id]
        });
      }
      
      onConversationCreated(conversation);
      setFormData({ name: '', description: '', isPrivate: false });
      setSelectedUsers([]);
      onClose();
      toast.success(`${type.charAt(0).toUpperCase() + type.slice(1)} created successfully`);
    } catch (error) {
      console.error(`Failed to create ${type}:`, error);
      toast.error(`Failed to create ${type}`);
    } finally {
      setIsLoading(false);
    }
  };

  const getTypeIcon = () => {
    switch (type) {
      case 'channel': return <Hash className="w-4 h-4" />;
      case 'group': return <Users className="w-4 h-4" />;
      case 'direct-message': return <MessageSquare className="w-4 h-4" />;
      default: return <MessageSquare className="w-4 h-4" />;
    }
  };

  const getTypeTitle = () => {
    switch (type) {
      case 'channel': return 'Create Channel';
      case 'group': return 'Create Group';
      case 'direct-message': return 'Create Direct Message';
      default: return 'Create Conversation';
    }
  };

  if (!isOpen) return null;

  return (
    <>
      <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
        <div className="bg-white rounded-lg p-6 w-full max-w-md">
          <h2 className="text-xl font-semibold mb-4 flex items-center">
            {getTypeIcon()}
            <span className="ml-2">{getTypeTitle()}</span>
          </h2>
          <form onSubmit={handleSubmit}>
            <div className="mb-4">
              <label className="block text-sm font-medium mb-2">Name</label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                className="w-full p-2 border rounded-md"
                required
                placeholder={type === 'channel' ? '#channel-name' : 'Conversation name'}
              />
            </div>
            <div className="mb-4">
              <label className="block text-sm font-medium mb-2">Description</label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                className="w-full p-2 border rounded-md h-20"
                placeholder="Optional description"
              />
            </div>
            
            {type !== 'direct-message' && (
              <div className="mb-4">
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={formData.isPrivate}
                    onChange={(e) => setFormData({ ...formData, isPrivate: e.target.checked })}
                    className="mr-2"
                  />
                  Private
                </label>
              </div>
            )}

            <div className="mb-4">
              <label className="block text-sm font-medium mb-2">
                {type === 'direct-message' ? 'Select User' : 'Add Members'}
              </label>
              <Button
                type="button"
                variant="outline"
                onClick={() => setIsUserSelectorOpen(true)}
                className="w-full"
              >
                <Plus className="w-4 h-4 mr-2" />
                {type === 'direct-message' ? 'Select User' : 'Add Members'}
              </Button>
              {selectedUsers.length > 0 && (
                <div className="mt-2 flex flex-wrap gap-1">
                  {selectedUsers.map(user => (
                    <span
                      key={user.id}
                      className="inline-flex items-center gap-1 px-2 py-1 bg-blue-100 text-blue-800 text-xs rounded"
                    >
                      {user.name}
                      <button
                        type="button"
                        onClick={() => setSelectedUsers(prev => prev.filter(u => u.id !== user.id))}
                        className="hover:bg-blue-200 rounded"
                      >
                        ×
                      </button>
                    </span>
                  ))}
                </div>
              )}
            </div>

            <div className="flex gap-2 justify-end">
              <Button type="button" variant="outline" onClick={onClose}>
                Cancel
              </Button>
              <Button type="submit" disabled={isLoading || (type === 'direct-message' && selectedUsers.length !== 1)}>
                {isLoading ? 'Creating...' : 'Create'}
              </Button>
            </div>
          </form>
        </div>
      </div>

      <UserSelector
        isOpen={isUserSelectorOpen}
        onClose={() => setIsUserSelectorOpen(false)}
        onUsersSelected={setSelectedUsers}
        title={type === 'direct-message' ? 'Select User' : 'Select Members'}
        multiSelect={type !== 'direct-message'}
        excludeUserIds={user ? [user.id] : []}
        workspaceId={type !== 'direct-message' ? workspaceId : undefined}
      />
    </>
  );
};

// Workspace Info Dialog Component
interface WorkspaceInfoDialogProps {
  isOpen: boolean;
  onClose: () => void;
  workspace: any;
}

const WorkspaceInfoDialog: React.FC<WorkspaceInfoDialogProps> = ({ isOpen, onClose, workspace }) => {
  const [members, setMembers] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isAddMemberOpen, setIsAddMemberOpen] = useState(false);
  const { user } = useAppStore();

  useEffect(() => {
    if (isOpen && workspace) {
      loadMembers();
    }
  }, [isOpen, workspace]);

  const loadMembers = async () => {
    if (!workspace) return;
    
    setIsLoading(true);
    try {
      const membersData = await chatApiService.getWorkspaceMembers(workspace.id);
      setMembers(membersData);
    } catch (error) {
      console.error('Failed to load members:', error);
      toast.error('Failed to load members');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddMembers = async (selectedUsers: any[]) => {
    for (const selectedUser of selectedUsers) {
      try {
        await chatApiService.addWorkspaceMember(workspace.id, {
          userId: selectedUser.id,
          addedById: user!.id,
          role: 'Member'
        });
      } catch (error) {
        console.error(`Failed to add user ${selectedUser.name}:`, error);
        toast.error(`Failed to add ${selectedUser.name}`);
      }
    }
    
    // Refresh members list
    loadMembers();
    setIsAddMemberOpen(false);
    toast.success('Members added successfully');
  };

  const handleRemoveMember = async (memberId: string) => {
    try {
      await chatApiService.removeWorkspaceMember(workspace.id, memberId, user!.id);
      loadMembers();
      toast.success('Member removed successfully');
    } catch (error) {
      console.error('Failed to remove member:', error);
      toast.error('Failed to remove member');
    }
  };

  if (!isOpen) return null;

  return (
    <>
      <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
        <div className="bg-white rounded-lg p-6 w-full max-w-2xl max-h-[80vh] overflow-y-auto">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-xl font-semibold">{workspace?.name} - Members</h2>
            <Button variant="ghost" onClick={onClose}>×</Button>
          </div>
          
          <div className="mb-4">
            <p className="text-gray-600">{workspace?.description}</p>
          </div>

          <div className="flex justify-between items-center mb-4">
            <h3 className="text-lg font-medium">Members ({members.length})</h3>
            <Button
              size="sm"
              onClick={() => setIsAddMemberOpen(true)}
              variant="outline"
            >
              <UserPlus className="w-4 h-4 mr-2" />
              Add Members
            </Button>
          </div>

          {isLoading ? (
            <div className="text-center py-4">Loading members...</div>
          ) : (
            <div className="space-y-2">
              {members.map(member => (
                <div key={member.id} className="flex items-center justify-between p-3 border rounded-lg">
                  <div className="flex items-center space-x-3">
                    <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center text-white text-sm font-medium">
                      {member.name?.charAt(0)?.toUpperCase() || 'U'}
                    </div>
                    <div>
                      <p className="font-medium">{member.name}</p>
                      <p className="text-sm text-gray-500">{member.email}</p>
                    </div>
                  </div>
                  <div className="flex items-center space-x-2">
                    <span className="text-xs bg-gray-100 px-2 py-1 rounded">{member.role}</span>
                    {member.id !== user?.id && member.role !== 'Owner' && (
                      <Button
                        size="sm"
                        variant="ghost"
                        onClick={() => handleRemoveMember(member.id)}
                        className="text-red-600 hover:text-red-800"
                      >
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      <UserSelector
        isOpen={isAddMemberOpen}
        onClose={() => setIsAddMemberOpen(false)}
        onUsersSelected={handleAddMembers}
        excludeUserIds={members.map(m => m.id)}
        multiSelect={true}
        title="Add Members to Workspace"
      />
    </>
  );
};

interface ConversationManagerProps {
  onCreateTaskFromChannel?: (channel: Conversation) => void;
}

export const ConversationManager: React.FC<ConversationManagerProps> = ({ onCreateTaskFromChannel }) => {
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [createDialogType, setCreateDialogType] = useState<'channel' | 'group' | 'direct-message'>('channel');
  const [isLoading, setIsLoading] = useState(true);
  const [isWorkspaceInfoOpen, setIsWorkspaceInfoOpen] = useState(false);
  const [channelTasks, setChannelTasks] = useState<{[channelId: string]: any[]}>({});
  const [expandedChannel, setExpandedChannel] = useState<string | null>(null);
  const [selectedTaskGroup, setSelectedTaskGroup] = useState<any>(null);
  const [taskGroupTaskInfo, setTaskGroupTaskInfo] = useState<any>(null);
  const { user, currentWorkspace, setCurrentConversation, setCurrentWorkspace } = useAppStore();

  useEffect(() => {
    if (currentWorkspace) {
      loadConversations();
    }
  }, [currentWorkspace]);

  const loadConversations = async () => {
    if (!currentWorkspace || !user) return;
    
    setIsLoading(true);
    try {
      const conversationsData = await chatApiService.getWorkspaceConversations(currentWorkspace.id, user.id);
      setConversations(conversationsData);
    } catch (error) {
      console.error('Failed to load conversations:', error);
      toast.error('Failed to load conversations');
    } finally {
      setIsLoading(false);
    }
  };

  const handleConversationCreated = (conversation: Conversation) => {
    setConversations([...conversations, conversation]);
  };

  const handleConversationSelect = (conversation: Conversation) => {
    setCurrentConversation(conversation as any);
  };

  const handleBackToWorkspaces = () => {
    setCurrentWorkspace(null);
  };

  const handleCreateTaskFromChannel = (channel: Conversation) => {
    // Channel'dan task oluşturma dialog'unu aç
    console.log('Create task from channel:', channel);
    onCreateTaskFromChannel?.(channel);
  };

  const loadChannelTasks = async (channelId: string) => {
    try {
      const tasks = await taskApiService.getTasksByChannel(channelId);
      setChannelTasks(prev => ({
        ...prev,
        [channelId]: tasks
      }));
    } catch (error) {
      console.error('Failed to load channel tasks:', error);
    }
  };

  const loadTaskGroupTaskInfo = async (taskGroup: any) => {
    try {
      // TaskGroup name'inden task ID'sini çıkar
      // Format: "Task: [Task Title] (Channel: [Channel Name])" veya "Task: [Task Title]"
      const nameMatch = taskGroup.name.match(/Task: (.+?)(?:\s*\(Channel: .+\))?$/);
      if (nameMatch) {
        const taskTitle = nameMatch[1];
        // Task title'ına göre task'ı bul (bu basit bir yaklaşım, gerçek implementasyonda daha iyi bir yöntem kullanılabilir)
        const allTasks = await taskApiService.getAllTasks(user?.id || '');
        const task = allTasks.find((t: any) => t.title === taskTitle);
        setTaskGroupTaskInfo(task);
      }
    } catch (error) {
      console.error('Failed to load task info:', error);
      setTaskGroupTaskInfo(null);
    }
  };

  const checkTaskGroupMembers = async (taskGroupId: string) => {
    try {
      console.log(`🔍 Checking TaskGroup members for: ${taskGroupId}`);
      const members = await chatApiService.getConversationMembers(taskGroupId, user?.id || '');
      console.log(`👥 TaskGroup members:`, members);
      console.log(`👥 TaskGroup members count:`, members.length);
      console.log(`👥 TaskGroup members details:`, members.map(m => ({ userId: m.userId, name: m.name, email: m.email })));
    } catch (error) {
      console.error('Failed to load TaskGroup members:', error);
    }
  };

  const toggleChannelTasks = (channelId: string) => {
    if (expandedChannel === channelId) {
      setExpandedChannel(null);
    } else {
      setExpandedChannel(channelId);
      if (!channelTasks[channelId]) {
        loadChannelTasks(channelId);
      }
    }
  };

  const openCreateDialog = (type: 'channel' | 'group' | 'direct-message') => {
    setCreateDialogType(type);
    setIsCreateDialogOpen(true);
  };

  const getTypeIcon = (type: string | number) => {
    // Backend'den gelen enum değerini string'e çevir
    const typeStr = typeof type === 'number' ? 
      (type === 1 ? 'channel' : type === 2 ? 'group' : type === 3 ? 'directmessage' : type === 4 ? 'taskgroup' : 'channel') :
      type.toString().toLowerCase();
    
    switch (typeStr) {
      case 'channel': return <Hash className="w-4 h-4" />;
      case 'group': return <Users className="w-4 h-4" />;
      case 'directmessage': return <MessageSquare className="w-4 h-4" />;
      case 'taskgroup': return <CheckSquare className="w-4 h-4" />;
      default: return <MessageSquare className="w-4 h-4" />;
    }
  };

  if (!currentWorkspace) {
    return (
      <div className="p-4 text-center text-gray-500">
        Select a workspace to view conversations
      </div>
    );
  }

  // Note: ChatArea artık sağ panelde gösteriliyor, burada gösterme

  if (isLoading) {
    return (
      <div className="p-4">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-200 rounded mb-4"></div>
          <div className="space-y-2">
            {[1, 2, 3].map(i => (
              <div key={i} className="h-12 bg-gray-200 rounded"></div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="h-full flex flex-col">
      <div className="p-4 border-b border-border">
        <div className="flex justify-between items-center mb-4">
          <div className="flex items-center space-x-2">
            <Button
              size="sm"
              variant="ghost"
              onClick={handleBackToWorkspaces}
              className="p-1"
            >
              <ArrowLeft className="w-4 h-4" />
            </Button>
            <h2 className="text-lg font-semibold">{currentWorkspace.name}</h2>
          </div>
          <Button
            size="sm"
            onClick={() => setIsWorkspaceInfoOpen(true)}
            variant="ghost"
          >
            <Info className="w-4 h-4" />
          </Button>
        </div>
        
        <div className="space-y-2">
          <Button
            size="sm"
            onClick={() => openCreateDialog('channel')}
            variant="outline"
            className="w-full justify-start"
          >
            <Hash className="w-4 h-4 mr-2" />
            Create Channel
          </Button>
          <Button
            size="sm"
            onClick={() => openCreateDialog('group')}
            variant="outline"
            className="w-full justify-start"
          >
            <Users className="w-4 h-4 mr-2" />
            Create Group
          </Button>
          <Button
            size="sm"
            onClick={() => openCreateDialog('direct-message')}
            variant="outline"
            className="w-full justify-start"
          >
            <MessageSquare className="w-4 h-4 mr-2" />
            Create DM
          </Button>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-4">
        <div className="space-y-2">
        {conversations
          .sort((a, b) => {
            // Channel (1) > Group (2) > DirectMessage (3) > TaskGroup (4) sırası
            console.log('Sorting conversations:', { a: a.type, b: b.type });
            
            // Backend'den gelen type'lar string veya number olabilir
            const getTypeString = (type: string | number) => {
              if (typeof type === 'number') {
                switch (type) {
                  case 1: return 'Channel';
                  case 2: return 'Group';
                  case 3: return 'DirectMessage';
                  case 4: return 'TaskGroup';
                  default: return 'Unknown';
                }
              }
              return type.toString();
            };
            
            const typeOrder = { 'Channel': 1, 'Group': 2, 'DirectMessage': 3, 'TaskGroup': 4 };
            const aTypeStr = getTypeString(a.type);
            const bTypeStr = getTypeString(b.type);
            const aOrder = typeOrder[aTypeStr as keyof typeof typeOrder] || 5;
            const bOrder = typeOrder[bTypeStr as keyof typeof typeOrder] || 5;
            return aOrder - bOrder;
          })
          .map(conversation => (
          <div
            key={conversation.id}
            className="border rounded-lg p-3 hover:bg-gray-50 cursor-pointer"
            onClick={() => handleConversationSelect(conversation)}
          >
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                {getTypeIcon(conversation.type)}
                <div className="ml-3">
                  <h3 className="font-medium">{conversation.name}</h3>
                  <p className="text-sm text-gray-600">{conversation.description}</p>
                  {conversation.lastMessage && (
                    <p className="text-xs text-gray-500 mt-1 truncate">
                      {conversation.lastMessage}
                    </p>
                  )}
                </div>
              </div>
              <div className="text-right text-xs text-gray-500">
                <p>{conversation.memberCount} members</p>
                {conversation.isPrivate && (
                  <p className="text-orange-600">Private</p>
                )}
                {/* Channel'larda task butonları */}
                {(conversation.type === 'Channel' || conversation.type === 1) && (
                  <div className="flex space-x-1 mt-1">
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={(e) => {
                        e.stopPropagation();
                        handleCreateTaskFromChannel(conversation);
                      }}
                      className="text-blue-600 hover:text-blue-800"
                      title="Create Task"
                    >
                      <CheckSquare className="w-4 h-4" />
                    </Button>
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={(e) => {
                        e.stopPropagation();
                        toggleChannelTasks(conversation.id);
                      }}
                      className="text-green-600 hover:text-green-800"
                      title="View Tasks"
                    >
                      <Users className="w-4 h-4" />
                    </Button>
                  </div>
                )}
                {/* TaskGroup'larda bilgilendirme butonu */}
                {(conversation.type === 'taskgroup' || conversation.type === 4) && (
                  <div className="flex space-x-1 mt-1">
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={(e) => {
                        e.stopPropagation();
                        setSelectedTaskGroup(conversation);
                        loadTaskGroupTaskInfo(conversation);
                        checkTaskGroupMembers(conversation.id);
                      }}
                      className="text-blue-600 hover:text-blue-800"
                      title="Task Info"
                    >
                      <Info className="w-4 h-4" />
                    </Button>
                  </div>
                )}
              </div>
            </div>
            
            {/* Channel Task Listesi */}
            {expandedChannel === conversation.id && (conversation.type === 'Channel' || conversation.type === 1) && (
              <div className="mt-2 ml-8 border-l-2 border-gray-200 pl-4">
                <div className="text-xs text-gray-500 mb-2">Tasks:</div>
                {channelTasks[conversation.id] ? (
                  channelTasks[conversation.id].length > 0 ? (
                    <div className="space-y-1">
                      {channelTasks[conversation.id].slice(0, 3).map((task: any) => (
                        <div key={task.id} className="text-xs bg-gray-50 p-2 rounded">
                          <div className="flex items-center justify-between">
                            <span className="font-medium truncate">{task.title}</span>
                            <span className={`px-1 py-0.5 rounded text-xs ${
                              task.status === 'Completed' ? 'bg-green-100 text-green-800' :
                              task.status === 'InProgress' ? 'bg-blue-100 text-blue-800' :
                              task.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
                              'bg-gray-100 text-gray-800'
                            }`}>
                              {task.status}
                            </span>
                          </div>
                          <div className="text-xs text-gray-500 mt-1">
                            Due: {new Date(task.dueDate).toLocaleDateString()}
                          </div>
                        </div>
                      ))}
                      {channelTasks[conversation.id].length > 3 && (
                        <div className="text-xs text-gray-500 text-center">
                          +{channelTasks[conversation.id].length - 3} more tasks
                        </div>
                      )}
                    </div>
                  ) : (
                    <div className="text-xs text-gray-500 italic">No tasks yet</div>
                  )
                ) : (
                  <div className="text-xs text-gray-500">Loading tasks...</div>
                )}
              </div>
            )}
          </div>
        ))}
        </div>
      </div>
      <CreateConversationDialog
        isOpen={isCreateDialogOpen}
        onClose={() => setIsCreateDialogOpen(false)}
        onConversationCreated={handleConversationCreated}
        workspaceId={currentWorkspace.id}
        type={createDialogType}
      />
      
      {/* TaskGroup Info Dialog */}
      {selectedTaskGroup && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="bg-card rounded-lg p-6 w-full max-w-md">
            <div className="flex items-start justify-between mb-4">
              <div>
                <h2 className="text-lg font-semibold">{selectedTaskGroup.name}</h2>
                <p className="text-sm text-muted-foreground mt-1">{selectedTaskGroup.description}</p>
              </div>
              <Button variant="ghost" size="sm" onClick={() => setSelectedTaskGroup(null)}>
                ✕
              </Button>
            </div>
            
            <div className="space-y-3">
              <div className="flex items-center space-x-2">
                <span className="text-sm font-medium text-muted-foreground">Type:</span>
                <span className="text-sm">TaskGroup</span>
              </div>
              
              <div className="flex items-center space-x-2">
                <span className="text-sm font-medium text-muted-foreground">Members:</span>
                <span className="text-sm">{selectedTaskGroup.memberCount}</span>
              </div>
              
              {selectedTaskGroup.isPrivate && (
                <div className="flex items-center space-x-2">
                  <span className="text-sm text-orange-600 font-medium">🔒 Private</span>
                </div>
              )}
              
              <div className="flex items-center space-x-2">
                <span className="text-sm font-medium text-muted-foreground">Created:</span>
                <span className="text-sm">{new Date(selectedTaskGroup.createdAt).toLocaleDateString()}</span>
              </div>

              {/* Task Bilgileri */}
              {taskGroupTaskInfo && (
                <div className="border-t pt-3 mt-3">
                  <h4 className="text-sm font-medium text-muted-foreground mb-2">Task Details:</h4>
                  <div className="space-y-2">
                    <div className="flex items-center space-x-2">
                      <span className="text-sm font-medium text-muted-foreground">Title:</span>
                      <span className="text-sm">{taskGroupTaskInfo.title}</span>
                    </div>
                    
                    {taskGroupTaskInfo.description && (
                      <div className="flex items-start space-x-2">
                        <span className="text-sm font-medium text-muted-foreground">Description:</span>
                        <span className="text-sm">{taskGroupTaskInfo.description}</span>
                      </div>
                    )}
                    
                    <div className="flex items-center space-x-2">
                      <span className="text-sm font-medium text-muted-foreground">Status:</span>
                      <span className="text-sm">{taskGroupTaskInfo.status}</span>
                    </div>
                    
                    <div className="flex items-center space-x-2">
                      <span className="text-sm font-medium text-muted-foreground">Priority:</span>
                      <span className="text-sm">{taskGroupTaskInfo.priority}</span>
                    </div>
                    
                    {taskGroupTaskInfo.dueDate && (
                      <div className="flex items-center space-x-2">
                        <span className="text-sm font-medium text-muted-foreground">Due Date:</span>
                        <span className="text-sm">{new Date(taskGroupTaskInfo.dueDate).toLocaleDateString()}</span>
                      </div>
                    )}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      <WorkspaceInfoDialog
        isOpen={isWorkspaceInfoOpen}
        onClose={() => setIsWorkspaceInfoOpen(false)}
        workspace={currentWorkspace}
      />
    </div>
  );
};
