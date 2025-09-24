import React, { useState, useEffect } from 'react';
import { Button } from '../ui/button';
import { UserSelector } from '../common/UserSelector';
import { useAppStore } from '../../store/useAppStore';
import { chatApiService } from '../../services/chat';
// import { authService } from '../../services/auth';
import { Plus, Settings, Users, Trash2, ArrowLeft } from 'lucide-react';
import toast from 'react-hot-toast';

interface Workspace {
  id: string;
  name: string;
  description: string;
  createdById: string;
  isActive: boolean;
  createdAt: string;
  memberCount: number;
  conversationCount: number;
}

interface CreateWorkspaceDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onWorkspaceCreated: (workspace: Workspace) => void;
}

const CreateWorkspaceDialog: React.FC<CreateWorkspaceDialogProps> = ({ isOpen, onClose, onWorkspaceCreated }) => {
  const [formData, setFormData] = useState({
    name: '',
    description: ''
  });
  const [isLoading, setIsLoading] = useState(false);
  const [selectedUsers, setSelectedUsers] = useState<any[]>([]);
  const [isUserSelectorOpen, setIsUserSelectorOpen] = useState(false);
  const { user } = useAppStore();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;

    setIsLoading(true);
    try {
      const workspace = await chatApiService.createWorkspace({
        name: formData.name,
        description: formData.description,
        createdById: user.id
      });
      
      // Add selected users as members
      if (selectedUsers.length > 0) {
        for (const selectedUser of selectedUsers) {
          try {
            await chatApiService.addWorkspaceMember(workspace.id, {
              userId: selectedUser.id,
              addedById: user.id,
              role: 'Member'
            });
          } catch (error) {
            console.error(`Failed to add user ${selectedUser.name} to workspace:`, error);
          }
        }
      }
      
      onWorkspaceCreated(workspace);
      setFormData({ name: '', description: '' });
      setSelectedUsers([]);
      onClose();
      toast.success('Workspace created successfully');
    } catch (error) {
      console.error('Failed to create workspace:', error);
      toast.error('Failed to create workspace');
    } finally {
      setIsLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md">
        <h2 className="text-xl font-semibold mb-4">Create Workspace</h2>
        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label className="block text-sm font-medium mb-2">Name</label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              className="w-full p-2 border rounded-md"
              required
            />
          </div>
          <div className="mb-4">
            <label className="block text-sm font-medium mb-2">Description</label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              className="w-full p-2 border rounded-md h-20"
            />
          </div>
          
          {/* User Selection */}
          <div className="mb-4">
            <label className="block text-sm font-medium mb-2">Add Members</label>
            <Button
              type="button"
              variant="outline"
              onClick={() => setIsUserSelectorOpen(true)}
              className="w-full"
            >
              <Users className="w-4 h-4 mr-2" />
              Select Users ({selectedUsers.length} selected)
            </Button>
            {selectedUsers.length > 0 && (
              <div className="mt-2 flex flex-wrap gap-1">
                {selectedUsers.map(user => (
                  <span
                    key={user.id}
                    className="bg-blue-100 text-blue-800 px-2 py-1 rounded-full text-xs flex items-center"
                  >
                    {user.name}
                    <button
                      type="button"
                      onClick={() => setSelectedUsers(selectedUsers.filter(u => u.id !== user.id))}
                      className="ml-1 text-blue-600 hover:text-blue-800"
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
            <Button type="submit" disabled={isLoading}>
              {isLoading ? 'Creating...' : 'Create'}
            </Button>
          </div>
        </form>
      </div>
      
      {/* User Selector Modal */}
      <UserSelector
        isOpen={isUserSelectorOpen}
        onClose={() => setIsUserSelectorOpen(false)}
        onUsersSelected={(users) => {
          setSelectedUsers([...selectedUsers, ...users]);
          setIsUserSelectorOpen(false);
        }}
        excludeUserIds={selectedUsers.map(u => u.id)}
        multiSelect={true}
        title="Add Members to Workspace"
      />
    </div>
  );
};

interface WorkspaceSettingsDialogProps {
  workspace: Workspace | null;
  isOpen: boolean;
  onClose: () => void;
  onWorkspaceUpdated: (workspace: Workspace) => void;
  onWorkspaceDeleted: (workspaceId: string) => void;
}

const WorkspaceSettingsDialog: React.FC<WorkspaceSettingsDialogProps> = ({
  workspace,
  isOpen,
  onClose,
  onWorkspaceUpdated,
  onWorkspaceDeleted
}) => {
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    isActive: true
  });
  const [isLoading, setIsLoading] = useState(false);
  const { user } = useAppStore();

  useEffect(() => {
    if (workspace) {
      setFormData({
        name: workspace.name,
        description: workspace.description,
        isActive: workspace.isActive
      });
    }
  }, [workspace]);

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!workspace || !user) return;

    setIsLoading(true);
    try {
      const updatedWorkspace = await chatApiService.updateWorkspace(workspace.id, {
        name: formData.name,
        description: formData.description,
        isActive: formData.isActive,
        updatedById: user.id
      });
      
      onWorkspaceUpdated(updatedWorkspace);
      onClose();
      toast.success('Workspace updated successfully');
    } catch (error) {
      console.error('Failed to update workspace:', error);
      toast.error('Failed to update workspace');
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!workspace || !user) return;
    
    if (!confirm('Are you sure you want to delete this workspace? This action cannot be undone.')) {
      return;
    }

    setIsLoading(true);
    try {
      await chatApiService.deleteWorkspace(workspace.id, user.id);
      onWorkspaceDeleted(workspace.id);
      onClose();
      toast.success('Workspace deleted successfully');
    } catch (error) {
      console.error('Failed to delete workspace:', error);
      toast.error('Failed to delete workspace');
    } finally {
      setIsLoading(false);
    }
  };

  if (!isOpen || !workspace) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md">
        <h2 className="text-xl font-semibold mb-4">Workspace Settings</h2>
        <form onSubmit={handleUpdate}>
          <div className="mb-4">
            <label className="block text-sm font-medium mb-2">Name</label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              className="w-full p-2 border rounded-md"
              required
            />
          </div>
          <div className="mb-4">
            <label className="block text-sm font-medium mb-2">Description</label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              className="w-full p-2 border rounded-md h-20"
            />
          </div>
          <div className="mb-4">
            <label className="flex items-center">
              <input
                type="checkbox"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                className="mr-2"
              />
              Active
            </label>
          </div>
          <div className="flex gap-2 justify-between">
            <Button type="button" variant="destructive" onClick={handleDelete} disabled={isLoading}>
              <Trash2 className="w-4 h-4 mr-2" />
              Delete
            </Button>
            <div className="flex gap-2">
              <Button type="button" variant="outline" onClick={onClose}>
                Cancel
              </Button>
              <Button type="submit" disabled={isLoading}>
                {isLoading ? 'Updating...' : 'Update'}
              </Button>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
};

export const WorkspaceManager: React.FC = () => {
  const [workspaces, setWorkspaces] = useState<Workspace[]>([]);
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [isSettingsDialogOpen, setIsSettingsDialogOpen] = useState(false);
  const [selectedWorkspace, setSelectedWorkspace] = useState<Workspace | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const { user, setCurrentWorkspace, currentWorkspace } = useAppStore();

  useEffect(() => {
    loadWorkspaces();
  }, []);

  // Kullanıcı değiştiğinde workspace'leri yeniden yükle
  useEffect(() => {
    if (user?.id) {
      loadWorkspaces();
    }
  }, [user?.id]);

  const loadWorkspaces = async () => {
    if (!user?.id) {
      console.warn('No user ID available for loading workspaces');
      setIsLoading(false);
      return;
    }

    try {
      const workspacesData = await chatApiService.getAllWorkspaces(user.id);
      setWorkspaces(workspacesData);
    } catch (error) {
      console.error('Failed to load workspaces:', error);
      toast.error('Failed to load workspaces');
    } finally {
      setIsLoading(false);
    }
  };

  const handleWorkspaceCreated = (_newWorkspace: Workspace) => {
    // Workspace listesini yeniden yükle (member sayıları güncel olsun)
    setTimeout(() => {
      loadWorkspaces();
    }, 1000);
  };

  const handleWorkspaceUpdated = (updatedWorkspace: Workspace) => {
    setWorkspaces(workspaces.map(w => w.id === updatedWorkspace.id ? updatedWorkspace : w));
  };

  const handleWorkspaceDeleted = (workspaceId: string) => {
    setWorkspaces(workspaces.filter(w => w.id !== workspaceId));
  };

  const handleWorkspaceSelect = (workspace: Workspace) => {
    setCurrentWorkspace(workspace as any);
  };

  const handleBackToWorkspaces = () => {
    setCurrentWorkspace(null);
  };

  const handleSettingsClick = (workspace: Workspace) => {
    setSelectedWorkspace(workspace);
    setIsSettingsDialogOpen(true);
  };

  if (isLoading) {
    return (
      <div className="p-4">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-200 rounded mb-4"></div>
          <div className="space-y-2">
            {[1, 2, 3].map(i => (
              <div key={i} className="h-16 bg-gray-200 rounded"></div>
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
            {currentWorkspace && (
              <Button
                size="sm"
                variant="ghost"
                onClick={handleBackToWorkspaces}
                className="p-1"
              >
                <ArrowLeft className="w-4 h-4" />
              </Button>
            )}
            <h2 className="text-lg font-semibold">Workspaces</h2>
          </div>
          <Button onClick={() => setIsCreateDialogOpen(true)}>
            <Plus className="w-4 h-4 mr-2" />
            Create Workspace
          </Button>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-4">
        <div className="space-y-2">
        {workspaces.map(workspace => (
          <div
            key={workspace.id}
            className="border rounded-lg p-4 hover:bg-gray-50 cursor-pointer"
            onClick={() => handleWorkspaceSelect(workspace)}
          >
            <div className="flex justify-between items-start">
              <div className="flex-1">
                <h3 className="font-medium">{workspace.name}</h3>
                <p className="text-sm text-gray-600 mt-1">{workspace.description}</p>
                <div className="flex items-center gap-4 mt-2 text-xs text-gray-500">
                  <span className="flex items-center">
                    <Users className="w-3 h-3 mr-1" />
                    {workspace.memberCount} members
                  </span>
                  <span>{workspace.conversationCount} conversations</span>
                </div>
              </div>
              <Button
                variant="ghost"
                size="sm"
                onClick={(e) => {
                  e.stopPropagation();
                  handleSettingsClick(workspace);
                }}
              >
                <Settings className="w-4 h-4" />
              </Button>
            </div>
          </div>
        ))}
        </div>
      </div>
      <CreateWorkspaceDialog
        isOpen={isCreateDialogOpen}
        onClose={() => setIsCreateDialogOpen(false)}
        onWorkspaceCreated={handleWorkspaceCreated}
      />

      <WorkspaceSettingsDialog
        workspace={selectedWorkspace}
        isOpen={isSettingsDialogOpen}
        onClose={() => setIsSettingsDialogOpen(false)}
        onWorkspaceUpdated={handleWorkspaceUpdated}
        onWorkspaceDeleted={handleWorkspaceDeleted}
      />
    </div>
  );
};
