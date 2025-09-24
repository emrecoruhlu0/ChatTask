import React, { useState, useEffect } from 'react';
import { Button } from '../ui/button';
import { UserSelector } from '../common/UserSelector';
import { useAppStore } from '../../store/useAppStore';
import { chatApiService } from '../../services/chat';
import { Trash2, UserPlus } from 'lucide-react';
import toast from 'react-hot-toast';

interface ConversationInfoDialogProps {
  isOpen: boolean;
  onClose: () => void;
  conversation: any;
}

export const ConversationInfoDialog: React.FC<ConversationInfoDialogProps> = ({ 
  isOpen, 
  onClose, 
  conversation 
}) => {
  const [members, setMembers] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isAddMemberOpen, setIsAddMemberOpen] = useState(false);
  const { user, currentWorkspace } = useAppStore();

  useEffect(() => {
    if (isOpen && conversation) {
      loadMembers();
    }
  }, [isOpen, conversation]);

  const loadMembers = async () => {
    if (!conversation || !currentWorkspace) return;
    
    setIsLoading(true);
    try {
      // Channel/Group için workspace member'larından conversation member'larını filtrele
      const workspaceMembers = await chatApiService.getWorkspaceMembers(currentWorkspace.id);
      
      // TODO: Conversation member'larını getiren API endpoint'i eklenecek
      // Şimdilik workspace member'larını göster
      setMembers(workspaceMembers);
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
        // TODO: Conversation member ekleme API'si eklenecek
        console.log(`Adding user ${selectedUser.name} to conversation ${conversation.id}`);
        toast.success(`${selectedUser.name} added to conversation`);
      } catch (error) {
        console.error(`Failed to add user ${selectedUser.name}:`, error);
        toast.error(`Failed to add ${selectedUser.name}`);
      }
    }
    
    loadMembers();
    setIsAddMemberOpen(false);
  };

  const handleRemoveMember = async (memberId: string) => {
    try {
      // TODO: Conversation member çıkarma API'si eklenecek
      console.log(`Removing user ${memberId} from conversation ${conversation.id}`);
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
            <h2 className="text-xl font-semibold">{conversation?.name} - Members</h2>
            <Button variant="ghost" onClick={onClose}>×</Button>
          </div>
          
          <div className="mb-4">
            <p className="text-gray-600">{conversation?.description}</p>
            <p className="text-sm text-gray-500">Type: {conversation?.type}</p>
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
                    <div className="w-8 h-8 bg-green-500 rounded-full flex items-center justify-center text-white text-sm font-medium">
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
        title="Add Members to Conversation"
        workspaceId={currentWorkspace?.id}
      />
    </>
  );
};
