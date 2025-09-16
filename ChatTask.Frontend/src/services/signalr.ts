import * as signalR from '@microsoft/signalr';
import { config } from './api';
import { Workspace } from '../types';
import toast from 'react-hot-toast';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private maxReconnectAttempts = 5;

  async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    try {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(config.signalRUrl)
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
              return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
            }
            return null;
          }
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Connection event handlers
      this.connection.onreconnecting(() => {
        console.log('🔄 SignalR: Reconnecting...');
        toast.loading('Reconnecting to chat...', { id: 'signalr-reconnect' });
      });

      this.connection.onreconnected(() => {
        console.log('✅ SignalR: Reconnected');
        toast.success('Reconnected to chat', { id: 'signalr-reconnect' });
      });

      this.connection.onclose((error) => {
        console.log('❌ SignalR: Connection closed', error);
        toast.error('Chat connection lost', { id: 'signalr-reconnect' });
      });

      await this.connection.start();
      console.log('🚀 SignalR: Connected successfully');
      toast.success('Connected to chat');
      
    } catch (error) {
      console.error('❌ SignalR: Connection failed', error);
      toast.error('Failed to connect to chat');
      throw error;
    }
  }

  onUserWorkspaces(callback: (workspaces: Workspace[]) => void): void {
    if (!this.connection) {
      console.warn('⚠️ SignalR: Connection not established');
      return;
    }

    this.connection.on('UserWorkspaces', (workspaces: Workspace[]) => {
      console.log('📨 SignalR: UserWorkspaces received', workspaces);
      callback(workspaces);
    });
  }

  onNewMessage(callback: (message: any) => void): void {
    if (!this.connection) return;
    
    this.connection.on('NewMessage', (message) => {
      console.log('📨 SignalR: NewMessage received', message);
      callback(message);
    });
  }

  onMessageRead(callback: (data: any) => void): void {
    if (!this.connection) return;
    
    this.connection.on('MessageRead', (data) => {
      console.log('📨 SignalR: MessageRead received', data);
      callback(data);
    });
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      console.log('🔌 SignalR: Disconnected');
    }
  }

  get connectionState(): signalR.HubConnectionState | null {
    return this.connection?.state || null;
  }

  get isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

export const signalRService = new SignalRService();
