import React, { useState, useEffect } from 'react';
import { Button } from '../ui/button';
import { chatApiService } from '../../services/chat';
import { User, Check, X } from 'lucide-react';
import toast from 'react-hot-toast';

interface UserData {
  id: string;
  name: string;
  email: string;
}

interface UserSelectorProps {
  isOpen: boolean;
  onClose: () => void;
  onUsersSelected: (users: UserData[]) => void;
  title?: string;
  multiSelect?: boolean;
  excludeUserIds?: string[];
  workspaceId?: string; // If provided, only show workspace members
  channelMembers?: any[]; // If provided, use these members instead of fetching
}

export const UserSelector: React.FC<UserSelectorProps> = ({
  isOpen,
  onClose,
  onUsersSelected,
  title = "Select Users",
  multiSelect = true,
  excludeUserIds = [],
  workspaceId,
  channelMembers
}) => {
  const [users, setUsers] = useState<UserData[]>([]);
  const [selectedUsers, setSelectedUsers] = useState<UserData[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    if (isOpen) {
      loadUsers();
    }
  }, [isOpen, channelMembers]);

  const loadUsers = async () => {
    setIsLoading(true);
    try {
      let usersData;
      
      // If channelMembers is provided, use them directly
      if (channelMembers && channelMembers.length > 0) {
        console.log('UserSelector: Using channelMembers:', channelMembers);
        usersData = channelMembers.map(member => {
          console.log('UserSelector: Mapping member:', member);
          const mappedUser = {
            id: member.UserId || member.id,
            name: member.Name || member.name,
            email: member.Email || member.email
          };
          console.log('UserSelector: Mapped user:', mappedUser);
          return mappedUser;
        });
        console.log('UserSelector: Final usersData:', usersData);
      }
      // If workspaceId is provided, get workspace members
      else if (workspaceId) {
        usersData = await chatApiService.getWorkspaceMembers(workspaceId);
      }
      // Otherwise get all users
      else {
        usersData = await chatApiService.getAllUsers();
      }
      
      // Only filter if excludeUserIds is provided and not empty
      const filteredUsers = excludeUserIds && excludeUserIds.length > 0 
        ? usersData.filter(user => !excludeUserIds.includes(user.id))
        : usersData;
      setUsers(filteredUsers);
    } catch (error) {
      console.error('Failed to load users:', error);
      toast.error('Failed to load users');
    } finally {
      setIsLoading(false);
    }
  };

  const handleUserToggle = (user: UserData) => {
    console.log('UserSelector: Toggling user:', user);
    console.log('UserSelector: Current selectedUsers:', selectedUsers);
    
    if (multiSelect) {
      setSelectedUsers(prev => {
        const isSelected = prev.some(u => u.id === user.id);
        console.log('UserSelector: User isSelected:', isSelected);
        
        if (isSelected) {
          // Remove user from selection
          const newSelection = prev.filter(u => u.id !== user.id);
          console.log('UserSelector: Removed user, new selection:', newSelection);
          return newSelection;
        } else {
          // Add user to selection
          const newSelection = [...prev, user];
          console.log('UserSelector: Added user, new selection:', newSelection);
          return newSelection;
        }
      });
    } else {
      console.log('UserSelector: Single select mode, setting user:', user);
      setSelectedUsers([user]);
    }
  };

  const handleConfirm = () => {
    console.log('UserSelector: Confirming selection:', selectedUsers);
    console.log('UserSelector: Selected user IDs:', selectedUsers.map(u => u.id));
    onUsersSelected(selectedUsers);
    setSelectedUsers([]);
    setSearchTerm('');
    onClose();
  };

  const handleCancel = () => {
    setSelectedUsers([]);
    setSearchTerm('');
    onClose();
  };

  const filteredUsers = users.filter(user =>
    user.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    user.email.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (!isOpen) {
    console.log('UserSelector: Not open, returning null');
    return null;
  }
  
  console.log('UserSelector: Rendering with props:', {
    isOpen,
    multiSelect,
    channelMembers: channelMembers?.length,
    users: users.length,
    selectedUsers: selectedUsers.length
  });

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md max-h-[80vh] flex flex-col">
        <h2 className="text-xl font-semibold mb-4">{title}</h2>
        
        <div className="mb-4">
          <input
            type="text"
            placeholder="Search users..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full p-2 border rounded-md"
          />
        </div>

        <div className="flex-1 overflow-y-auto mb-4">
          {isLoading ? (
            <div className="animate-pulse space-y-2">
              {[1, 2, 3].map(i => (
                <div key={i} className="h-12 bg-gray-200 rounded"></div>
              ))}
            </div>
          ) : filteredUsers.length === 0 ? (
            <div className="text-center py-8 text-gray-500">
              {channelMembers && channelMembers.length === 0 ? (
                <div>
                  <User className="w-12 h-12 mx-auto mb-2 text-gray-400" />
                  <p>No members found in this channel</p>
                </div>
              ) : (
                <div>
                  <User className="w-12 h-12 mx-auto mb-2 text-gray-400" />
                  <p>No users found</p>
                </div>
              )}
            </div>
          ) : (
            <div className="space-y-2">
              {filteredUsers.map(user => {
                const isSelected = selectedUsers.some(u => u.id === user.id);
                console.log(`UserSelector: User ${user.name} (${user.id}) isSelected:`, isSelected);
                return (
                  <button
                    key={user.id}
                    type="button"
                    className={`w-full p-3 border rounded-lg cursor-pointer transition-colors text-left ${
                      isSelected ? 'bg-blue-50 border-blue-200' : 'hover:bg-gray-50'
                    }`}
                    onClick={() => {
                      console.log('UserSelector: User button clicked:', user);
                      handleUserToggle(user);
                    }}
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex items-center">
                        <div className="w-8 h-8 bg-gray-200 rounded-full flex items-center justify-center mr-3">
                          <User className="w-4 h-4 text-gray-600" />
                        </div>
                        <div>
                          <p className="font-medium">{user.name}</p>
                          <p className="text-sm text-gray-600">{user.email}</p>
                        </div>
                      </div>
                      {isSelected && (
                        <Check className="w-5 h-5 text-blue-600" />
                      )}
                    </div>
                  </button>
                );
              })}
            </div>
          )}
        </div>

        {selectedUsers.length > 0 && (
          <div className="mb-4 p-3 bg-blue-50 rounded-lg">
            <p className="text-sm font-medium text-blue-800">
              {selectedUsers.length} user{selectedUsers.length > 1 ? 's' : ''} selected
            </p>
            <div className="flex flex-wrap gap-1 mt-2">
              {selectedUsers.map(user => (
                <span
                  key={user.id}
                  className="inline-flex items-center gap-1 px-2 py-1 bg-blue-100 text-blue-800 text-xs rounded"
                >
                  {user.name}
                  <button
                    onClick={() => handleUserToggle(user)}
                    className="hover:bg-blue-200 rounded"
                  >
                    <X className="w-3 h-3" />
                  </button>
                </span>
              ))}
            </div>
          </div>
        )}

        <div className="flex gap-2 justify-end">
          <Button variant="outline" onClick={handleCancel}>
            Cancel
          </Button>
          <Button 
            onClick={handleConfirm}
            disabled={selectedUsers.length === 0}
          >
            Select {selectedUsers.length > 0 ? `(${selectedUsers.length})` : ''}
          </Button>
        </div>
      </div>
    </div>
  );
};
