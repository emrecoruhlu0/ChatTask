import React, { useState, useEffect, useRef } from 'react';
import { Button } from '../ui/button';
import { useAppStore } from '../../store/useAppStore';
import { chatApiService } from '../../services/chat';
import { signalRService } from '../../services/signalr';
import { ConversationInfoDialog } from '../conversation/ConversationInfoDialog';
import { 
  Send, 
  Hash, 
  User, 
  Loader2, 
  MessageSquare,
  Check,
  ArrowLeft,
  Info
} from 'lucide-react';
import { format, isToday, isYesterday } from 'date-fns';
import toast from 'react-hot-toast';

export const ChatArea: React.FC = () => {
  const {
    user,
    currentConversation,
    messages,
    isLoadingMessages,
    setMessages,
    addMessage,
    setLoadingMessages,
    updateMessage,
    setCurrentConversation
  } = useAppStore();

  const [newMessage, setNewMessage] = useState('');
  const [isSending, setIsSending] = useState(false);
  const [isConversationInfoOpen, setIsConversationInfoOpen] = useState(false);
  const scrollAreaRef = useRef<HTMLDivElement>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Load messages and join SignalR group when conversation changes
  useEffect(() => {
    if (currentConversation) {
      loadMessages();
      // Join SignalR group for real-time messages
      signalRService.joinConversation(currentConversation.id);
    }
    
    // Cleanup: leave previous conversation group
    return () => {
      if (currentConversation) {
        signalRService.leaveConversation(currentConversation.id);
      }
    };
  }, [currentConversation]);

  // Auto-scroll to bottom when messages change
  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const loadMessages = async () => {
    if (!currentConversation) return;
    
    setLoadingMessages(true);
    try {
      const messages = await chatApiService.getMessages(currentConversation.id);
      setMessages(messages);
    } catch (error) {
      console.error('Failed to load messages:', error);
      toast.error('Failed to load messages');
    } finally {
      setLoadingMessages(false);
    }
  };

  const sendMessage = async () => {
    if (!currentConversation || !user || !newMessage.trim()) return;

    const messageContent = newMessage.trim();
    setNewMessage('');
    setIsSending(true);

    try {
      const message = await chatApiService.sendMessage(currentConversation.id, {
        senderId: user.id,
        content: messageContent
      });
      
      addMessage(message);
      toast.success('Message sent');
    } catch (error) {
      console.error('Failed to send message:', error);
      toast.error('Failed to send message');
      setNewMessage(messageContent); // Restore message on error
    } finally {
      setIsSending(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      sendMessage();
    }
  };

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const formatMessageTime = (dateString: string) => {
    const date = new Date(dateString);
    if (isToday(date)) {
      return format(date, 'HH:mm');
    } else if (isYesterday(date)) {
      return `Yesterday ${format(date, 'HH:mm')}`;
    } else {
      return format(date, 'MMM dd, HH:mm');
    }
  };

  const getInitials = (name: string) => {
    return name.split(' ').map(n => n[0]).join('').toUpperCase();
  };

  const markAsRead = async (messageId: string) => {
    if (!user) return;
    
    try {
      await chatApiService.markMessageAsRead(messageId, user.id);
      updateMessage(messageId, { isRead: true });
    } catch (error) {
      console.error('Failed to mark message as read:', error);
    }
  };

  if (!currentConversation) {
    return (
      <div className="flex-1 flex items-center justify-center bg-background">
        <div className="text-center">
          <MessageSquare className="h-16 w-16 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-semibold text-foreground mb-2">
            Select a conversation
          </h3>
          <p className="text-muted-foreground">
            Choose a channel or direct message to start chatting
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 flex flex-col bg-background">
      {/* Header */}
      <div className="border-b border-border p-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setCurrentConversation(null)}
            >
              <ArrowLeft className="w-4 h-4" />
            </Button>
            {currentConversation.type === 'Channel' ? (
              <Hash className="h-5 w-5 text-muted-foreground" />
            ) : (
              <User className="h-5 w-5 text-muted-foreground" />
            )}
            <div>
              <h2 className="font-semibold text-foreground">{currentConversation.name}</h2>
            </div>
            {currentConversation.memberCount && (
              <div className="text-sm text-muted-foreground bg-muted px-2 py-1 rounded-full">
                {currentConversation.memberCount}
              </div>
            )}
          </div>
          <div className="flex items-center space-x-2">
            <Button 
              variant="ghost" 
              size="sm"
              onClick={() => setIsConversationInfoOpen(true)}
            >
              <Info className="h-4 w-4" />
            </Button>
          </div>
        </div>
        {currentConversation.description && (
          <p className="text-sm text-muted-foreground mt-2">{currentConversation.description}</p>
        )}
      </div>

      {/* Messages */}
      <div ref={scrollAreaRef} className="flex-1 overflow-y-auto p-4 space-y-4">
        {isLoadingMessages ? (
          <div className="flex items-center justify-center py-8">
            <Loader2 className="h-6 w-6 animate-spin" />
          </div>
        ) : messages.length === 0 ? (
          <div className="text-center py-8">
            <MessageSquare className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <h3 className="font-medium text-foreground mb-2">No messages yet</h3>
            <p className="text-sm text-muted-foreground">
              Be the first to start the conversation!
            </p>
          </div>
        ) : (
          <div className="space-y-4">
            {messages.map((message, index) => {
              const isCurrentUser = message.senderId === user?.id;
              const showAvatar = index === 0 || messages[index - 1].senderId !== message.senderId;
              
              return (
                <div
                  key={message.id}
                  className={`flex space-x-3 ${!message.isRead ? 'opacity-100' : 'opacity-75'} hover:opacity-100 transition-opacity`}
                  onClick={() => !message.isRead && markAsRead(message.id)}
                >
                  {/* Avatar */}
                  <div className="flex-shrink-0">
                    {showAvatar ? (
                      <div className="w-8 h-8 bg-gradient-to-r from-primary to-chat-primary rounded-full flex items-center justify-center text-white text-sm font-medium">
                        {getInitials(message.senderName)}
                      </div>
                    ) : (
                      <div className="w-8 h-8" />
                    )}
                  </div>

                  {/* Message Content */}
                  <div className="flex-1 min-w-0">
                    {showAvatar && (
                      <div className="flex items-center space-x-2 mb-1">
                        <span className="font-medium text-sm text-foreground">
                          {isCurrentUser ? 'You' : message.senderName}
                        </span>
                        <span className="text-xs text-muted-foreground">
                          {formatMessageTime(message.createdAt)}
                        </span>
                      </div>
                    )}
                    <div className="bg-muted/50 rounded-lg p-3 max-w-2xl">
                      <p className="text-sm text-foreground whitespace-pre-wrap">{message.content}</p>
                    </div>

                    {/* Message status */}
                    {isCurrentUser && (
                      <div className="flex items-center space-x-1 mt-1">
                        {message.isRead && (
                          <div className="flex items-center space-x-1 text-xs text-muted-foreground">
                            <Check className="h-3 w-3" />
                            <span>Read</span>
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              );
            })}
            <div ref={messagesEndRef} />
          </div>
        )}
      </div>

      {/* Message Input */}
      <div className="border-t border-border p-4">
        <div className="flex space-x-2">
          <textarea
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            onKeyPress={handleKeyPress}
            disabled={isSending}
            placeholder="Type a message..."
            className="flex-1 resize-none border border-input rounded-md px-3 py-2 bg-background focus:outline-none focus:ring-2 focus:ring-ring"
            rows={1}
          />
          <Button onClick={sendMessage} disabled={isSending || !newMessage.trim()}>
            {isSending ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <Send className="h-4 w-4" />
            )}
          </Button>
        </div>
      </div>
      
      <ConversationInfoDialog
        isOpen={isConversationInfoOpen}
        onClose={() => setIsConversationInfoOpen(false)}
        conversation={currentConversation}
      />
    </div>
  );
};
