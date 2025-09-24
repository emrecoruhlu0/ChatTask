import React, { useState, useEffect } from 'react';
import { Button } from '../ui/button';
import { UserSelector } from '../common/UserSelector';
import { useAppStore } from '../../store/useAppStore';
import { taskApiService } from '../../services/tasks';
import { 
  Plus, 
  Clock, 
  AlertCircle, 
  CheckSquare, 
  Loader2,
  Trash2,
  UserPlus,
  Calendar,
  Users,
  Filter,
  XCircle,
  Pause
} from 'lucide-react';
import { format } from 'date-fns';
import { Task, CreateTaskRequest, TaskStatus, TaskPriority } from '../../types';
import toast from 'react-hot-toast';

interface TaskManagerProps {
  channelContext?: {
    id: string;
    name: string;
    members: any[];
  };
  availableChannels?: any[];
  onTaskCreated?: (task: Task) => void;
  openCreateDialog?: boolean;
  onCreateDialogClose?: () => void;
}

export const TaskManager: React.FC<TaskManagerProps> = ({ 
  channelContext, 
  availableChannels = [],
  openCreateDialog = false, 
  onCreateDialogClose 
}) => {
  const {
    user,
    tasks,
    userTasks,
    isLoadingTasks,
    setTasks,
    setUserTasks,
    addTask,
    updateTask,
    setLoadingTasks
  } = useAppStore();

  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(openCreateDialog);
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const [activeTab, setActiveTab] = useState<'all' | 'my'>('my');
  const [selectedChannelId, setSelectedChannelId] = useState<string>('');
  const [isUserSelectorOpen, setIsUserSelectorOpen] = useState(false);
  const [taskToAssign, setTaskToAssign] = useState<Task | null>(null);

  // Form state for creating tasks
  const [newTask, setNewTask] = useState<Partial<CreateTaskRequest>>({
    title: '',
    description: '',
    status: 'Pending',
    priority: 'Medium',
    dueDate: new Date().toISOString().split('T')[0],
    isPrivate: false,
    createdById: user?.id || '',
    AssigneeIds: []
  });

  useEffect(() => {
    loadTasks();
  }, [user]);

  useEffect(() => {
    setIsCreateDialogOpen(openCreateDialog);
  }, [openCreateDialog]);

  useEffect(() => {
    // Channel context'ten gelen channel'ı default olarak seç
    console.log('TaskManager: useEffect triggered, channelContext:', channelContext);
    if (channelContext?.id) {
      console.log('TaskManager: Channel context received:', channelContext);
      console.log('TaskManager: Channel members:', channelContext.members);
      setSelectedChannelId(channelContext.id);
    } else {
      // Channel context yoksa hiçbir channel seçili olmasın
      console.log('TaskManager: No channel context');
      setSelectedChannelId('');
    }
  }, [channelContext]);

  const loadTasks = async () => {
    if (!user) return;
    
    setLoadingTasks(true);
    try {
      // Sadece kullanıcının oluşturduğu task'ları göster (Assigned Tasks)
      const createdTasks = await taskApiService.getAllTasks(user.id);
      setTasks(createdTasks);
      
      // My Tasks için ayrı API çağrısı
      const myTasks = await taskApiService.getTasksByUserId(user.id);
      setUserTasks(myTasks);
    } catch (error) {
      console.error('Failed to load tasks:', error);
      toast.error('Failed to load tasks');
    } finally {
      setLoadingTasks(false);
    }
  };

  const handleCreateTask = async () => {
    if (!user || !newTask.title) {
      toast.error('Please fill in required fields');
      return;
    }

    try {
      // AssigneeIds kontrolü ve temizleme
      let assigneeIds: string[] = [];
      if (newTask.AssigneeIds && Array.isArray(newTask.AssigneeIds) && newTask.AssigneeIds.length > 0) {
        assigneeIds = newTask.AssigneeIds.filter(id => id && id.trim() !== '');
        console.log('TaskManager: Filtered AssigneeIds:', assigneeIds);
      }
      
      // Eğer hiç assignee yoksa, current user'ı ekle
      if (assigneeIds.length === 0) {
        assigneeIds = [user.id];
        console.log('TaskManager: No assignees, using current user:', user.id);
      }

      const taskData: CreateTaskRequest = {
        title: newTask.title!,
        description: newTask.description || '',
        status: newTask.status as TaskStatus,
        priority: newTask.priority as TaskPriority,
        dueDate: new Date(newTask.dueDate + 'T00:00:00Z').toISOString(),
        isPrivate: newTask.isPrivate || false,
        createdById: user.id,
        channelId: selectedChannelId || undefined,
        AssigneeIds: assigneeIds
      };

      console.log('🚀 Selected Channel ID:', selectedChannelId);
      console.log('🚀 Original AssigneeIds:', newTask.AssigneeIds);
      console.log('🚀 Processed AssigneeIds:', assigneeIds);
      console.log('🚀 Sending task data:', taskData);
      const createdTask = await taskApiService.createTask(taskData);
      addTask(createdTask);
      setIsCreateDialogOpen(false);
      onCreateDialogClose?.(); // Parent'a bildir
      setNewTask({
        title: '',
        description: '',
        status: 'Pending',
        priority: 'Medium',
        dueDate: new Date().toISOString().split('T')[0],
        AssigneeIds: []
      });
      toast.success('Task created successfully');
    } catch (error) {
      console.error('Failed to create task:', error);
      toast.error('Failed to create task');
    }
  };

  const handleStatusUpdate = async (taskId: string, newStatus: TaskStatus) => {
    try {
      const updatedTask = await taskApiService.updateTaskStatus(taskId, newStatus);
      updateTask(taskId, updatedTask);
      toast.success('Task status updated');
    } catch (error) {
      console.error('Failed to update task status:', error);
      toast.error('Failed to update task status');
    }
  };

  const handleDeleteTask = async (taskId: string) => {
    if (!confirm('Are you sure you want to delete this task?')) {
      return;
    }

    try {
      await taskApiService.deleteTask(taskId);
      // Remove from local state
      const updatedTasks = tasks.filter(task => task.id !== taskId);
      const updatedUserTasks = userTasks.filter(task => task.id !== taskId);
      // Update store
      useAppStore.setState({ tasks: updatedTasks, userTasks: updatedUserTasks });
      toast.success('Task deleted successfully');
    } catch (error) {
      console.error('Failed to delete task:', error);
      toast.error('Failed to delete task');
    }
  };

  const handleAssignTask = (task: Task) => {
    setTaskToAssign(task);
    setIsUserSelectorOpen(true);
  };

  const handleUsersSelected = async (users: any[]) => {
    console.log('TaskManager: handleUsersSelected called with:', users);
    if (!taskToAssign) return;

    // Eğer yeni task oluşturuluyorsa
    if (taskToAssign.id === 'new') {
      console.log('TaskManager: Setting selected users for new task:', users);
      setNewTask(prev => {
        const newAssigneeIds = users.map(u => u.id);
        console.log('TaskManager: New AssigneeIds:', newAssigneeIds);
        return {
          ...prev,
          AssigneeIds: newAssigneeIds
        };
      });
      setIsUserSelectorOpen(false);
      setTaskToAssign(null);
      return;
    }

    // Mevcut task'a member ekleme
    try {
      await taskApiService.assignTaskToUsers(taskToAssign.id, users.map(u => u.id));
      toast.success(`Task assigned to ${users.length} user${users.length > 1 ? 's' : ''}`);
      // Reload tasks to get updated assignments
      loadTasks();
    } catch (error) {
      console.error('Failed to assign task:', error);
      toast.error('Failed to assign task');
    } finally {
      setTaskToAssign(null);
      setIsUserSelectorOpen(false);
    }
  };

  const getPriorityColor = (priority: TaskPriority) => {
    switch (priority) {
      case 'Critical': return 'destructive';
      case 'High': return 'default';
      case 'Medium': return 'secondary';
      case 'Low': return 'outline';
      default: return 'secondary';
    }
  };

  const getStatusIcon = (status: TaskStatus) => {
    switch (status) {
      case 'Pending': return Clock;
      case 'InProgress': return AlertCircle;
      case 'Completed': return CheckSquare;
      case 'Cancelled': return XCircle;
      case 'OnHold': return Pause;
      default: return Clock;
    }
  };

  const getStatusColor = (status: TaskStatus) => {
    switch (status) {
      case 'Pending': return 'text-muted-foreground';
      case 'InProgress': return 'text-warning';
      case 'Completed': return 'text-success';
      case 'Cancelled': return 'text-destructive';
      case 'OnHold': return 'text-orange-500';
      default: return 'text-muted-foreground';
    }
  };

  const TaskCard: React.FC<{ task: Task }> = ({ task }: { task: Task }) => {
    const StatusIcon = getStatusIcon(task.status);
    const isOverdue = new Date(task.dueDate) < new Date() && task.status !== 'Completed';

    return (
      <div 
        className="bg-card border border-border rounded-lg p-4 hover:shadow-md transition-shadow cursor-pointer"
        onClick={() => setSelectedTask(task)}
      >
        <div className="flex items-start justify-between mb-3">
          <div className="flex-1 min-w-0">
            <h3 className="font-medium text-foreground truncate">{task.title}</h3>
            {task.description && (
              <p className="text-sm text-muted-foreground mt-1 line-clamp-2">{task.description}</p>
            )}
          </div>
          <div className="flex items-center gap-2">
            <Button variant={getPriorityColor(task.priority)} size="sm">
              {task.priority}
            </Button>
            <div className="flex gap-1">
              <Button
                variant="ghost"
                size="sm"
                onClick={(e) => {
                  e.stopPropagation();
                  handleAssignTask(task);
                }}
                title="Assign to users"
              >
                <UserPlus className="w-4 h-4" />
              </Button>
              <Button
                variant="ghost"
                size="sm"
                onClick={(e) => {
                  e.stopPropagation();
                  handleDeleteTask(task.id);
                }}
                title="Delete task"
              >
                <Trash2 className="w-4 h-4" />
              </Button>
            </div>
          </div>
        </div>
        
        <div className="flex items-center justify-between text-sm">
          <div className="flex items-center space-x-4">
            <div className="flex items-center space-x-1">
              <StatusIcon className="h-4 w-4" />
              <span className={getStatusColor(task.status)}>{task.status}</span>
            </div>
            <div className="flex items-center space-x-1">
              <Calendar className="h-4 w-4" />
              <span className={isOverdue ? 'text-destructive' : 'text-muted-foreground'}>
                {format(new Date(task.dueDate), 'MMM dd')}
              </span>
            </div>
          </div>
          {task.assignmentCount > 0 && (
            <div className="flex items-center space-x-1 text-muted-foreground">
              <Users className="h-4 w-4" />
              <span>{task.assignmentCount} assigned</span>
            </div>
          )}
        </div>
      </div>
    );
  };

  return (
    <div className="flex-1 flex flex-col bg-background">
      {/* Header */}
      <div className="border-b border-border p-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-foreground">Tasks</h1>
            <p className="text-muted-foreground mt-1">Manage your projects and assignments</p>
          </div>
          <Button onClick={() => setIsCreateDialogOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            New Task
          </Button>
        </div>
      </div>

      {/* Task Lists */}
      <div className="flex-1 p-6">
        <div className="flex items-center space-x-2 mb-6">
          <Button
            variant={activeTab === 'my' ? 'default' : 'outline'}
            onClick={() => setActiveTab('my')}
          >
            <Users className="mr-2 h-4 w-4" />
            My Tasks ({userTasks.length})
          </Button>
          <Button
            variant={activeTab === 'all' ? 'default' : 'outline'}
            onClick={() => setActiveTab('all')}
          >
            <Filter className="mr-2 h-4 w-4" />
            Assigned Tasks ({tasks.length})
          </Button>
        </div>

        {isLoadingTasks ? (
          <div className="flex items-center justify-center py-8">
            <Loader2 className="h-6 w-6 animate-spin" />
          </div>
        ) : (
          <>
            {/* My Tasks */}
            {activeTab === 'my' && (
              <div className="space-y-4">
                {userTasks.length === 0 ? (
                  <div className="text-center py-12">
                    <CheckSquare className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                    <h3 className="font-medium text-foreground mb-2">No tasks assigned</h3>
                    <p className="text-muted-foreground">Create a new task to get started</p>
                  </div>
                ) : (
                  <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {userTasks.map((task) => (
                      <TaskCard key={task.id} task={task} />
                    ))}
                  </div>
                )}
              </div>
            )}

            {/* Assigned Tasks */}
            {activeTab === 'all' && (
              <div className="space-y-4">
                {tasks.length === 0 ? (
                  <div className="text-center py-12">
                    <CheckSquare className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                    <h3 className="font-medium text-foreground mb-2">No tasks available</h3>
                    <p className="text-muted-foreground">Be the first to create a task</p>
                  </div>
                ) : (
                  <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {tasks.map((task) => (
                      <TaskCard key={task.id} task={task} />
                    ))}
                  </div>
                )}
              </div>
            )}
          </>
        )}
      </div>

      {/* Create Task Dialog */}
      {isCreateDialogOpen && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="bg-card rounded-lg p-6 w-full max-w-md">
            <h2 className="text-lg font-semibold mb-4">Create New Task</h2>
            <p className="text-sm text-muted-foreground mb-6">
              Add a new task to your project. Fill in the details below.
            </p>
            
            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium">Title *</label>
                <input
                  type="text"
                  className="w-full mt-1 px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  value={newTask.title}
                  onChange={(e) => setNewTask({ ...newTask, title: e.target.value })}
                />
              </div>
              <div>
                <label className="text-sm font-medium">Channel</label>
                <select
                  className="w-full mt-1 px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  value={selectedChannelId}
                  onChange={(e) => setSelectedChannelId(e.target.value)}
                >
                  <option value="">No Channel (General Task)</option>
                  {availableChannels.map((channel) => (
                    <option key={channel.id} value={channel.id}>
                      {channel.name}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="text-sm font-medium">Description</label>
                <textarea
                  className="w-full mt-1 px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  rows={3}
                  value={newTask.description}
                  onChange={(e) => setNewTask({ ...newTask, description: e.target.value })}
                />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium">Priority</label>
                  <select
                    className="w-full mt-1 px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                    value={newTask.priority}
                    onChange={(e) => setNewTask({ ...newTask, priority: e.target.value as TaskPriority })}
                  >
                    <option value="Low">Low</option>
                    <option value="Medium">Medium</option>
                    <option value="High">High</option>
                    <option value="Critical">Critical</option>
                  </select>
                </div>
                <div>
                  <label className="text-sm font-medium">Due Date</label>
                  <input
                    type="date"
                    className="w-full mt-1 px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                    value={newTask.dueDate}
                    onChange={(e) => setNewTask({ ...newTask, dueDate: e.target.value })}
                  />
                </div>
              </div>
              
              {/* Private Task Option */}
              <div className="flex items-center space-x-2 mt-4">
                <input
                  type="checkbox"
                  id="isPrivate"
                  className="rounded border-input"
                  checked={newTask.isPrivate || false}
                  onChange={(e) => setNewTask({ ...newTask, isPrivate: e.target.checked })}
                />
                <label htmlFor="isPrivate" className="text-sm font-medium">
                  Private Task (Only visible to creator and assigned users)
                </label>
              </div>

              {/* Member Assignment */}
              <div>
                <label className="text-sm font-medium">Assign Members</label>
                <div className="mt-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      console.log('TaskManager: Opening user selector with channelContext:', channelContext);
                      console.log('TaskManager: Channel members:', channelContext?.members);
                      setTaskToAssign({ id: 'new', assignments: [] } as any);
                      setIsUserSelectorOpen(true);
                      console.log('TaskManager: isUserSelectorOpen set to true');
                    }}
                    className="w-full justify-start"
                  >
                    <UserPlus className="w-4 h-4 mr-2" />
                    {newTask.AssigneeIds && newTask.AssigneeIds.length > 0 
                      ? `${newTask.AssigneeIds.length} member(s) selected`
                      : 'Select members'
                    }
                  </Button>
                  
                  {/* Display selected members */}
                  {newTask.AssigneeIds && newTask.AssigneeIds.length > 0 && (
                    <div className="mt-2 flex flex-wrap gap-1">
                      {newTask.AssigneeIds.map((userId) => {
                        // Try to find user name from channel members
                        const member = channelContext?.members?.find(m => (m.UserId || m.id) === userId);
                        const userName = member ? (member.Name || member.name) : `User ${userId.slice(0, 8)}...`;
                        
                        return (
                          <span
                            key={userId}
                            className="bg-blue-100 text-blue-800 px-2 py-1 rounded-full text-xs flex items-center"
                          >
                            {userName}
                            <button
                              type="button"
                              onClick={() => {
                                setNewTask(prev => ({
                                  ...prev,
                                  AssigneeIds: prev.AssigneeIds?.filter(id => id !== userId) || []
                                }));
                              }}
                              className="ml-1 text-blue-600 hover:text-blue-800"
                            >
                              ×
                            </button>
                          </span>
                        );
                      })}
                    </div>
                  )}
                </div>
              </div>
            </div>
            
            <div className="flex justify-end space-x-2 mt-6">
              <Button variant="outline" onClick={() => setIsCreateDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleCreateTask}>
                Create Task
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Task Detail Dialog */}
      {selectedTask && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="bg-card rounded-lg p-6 w-full max-w-md">
            <div className="flex items-start justify-between mb-4">
              <div>
                <h2 className="text-lg font-semibold">{selectedTask.title}</h2>
                <Button variant={getPriorityColor(selectedTask.priority)} size="sm" className="mt-2">
                  {selectedTask.priority}
                </Button>
              </div>
            </div>
            
            {selectedTask.description && (
              <p className="text-muted-foreground mb-4">{selectedTask.description}</p>
            )}
            
            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium">Status</label>
                <select
                  className="w-full mt-1 px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  value={selectedTask.status}
                  onChange={(e) => handleStatusUpdate(selectedTask.id, e.target.value as TaskStatus)}
                >
                  <option value="Pending">Pending</option>
                  <option value="InProgress">In Progress</option>
                  <option value="Completed">Completed</option>
                  <option value="Cancelled">Cancelled</option>
                  <option value="OnHold">On Hold</option>
                </select>
              </div>
              <div>
                <label className="text-sm font-medium">Due Date</label>
                <p className="text-sm text-muted-foreground mt-1">
                  {format(new Date(selectedTask.dueDate), 'PPP')}
                </p>
              </div>
              <div>
                <label className="text-sm font-medium">Assignments ({selectedTask.assignmentCount})</label>
                <div className="mt-2 space-y-2">
                  {selectedTask.assignments.map((assignment) => (
                    <div key={assignment.userId} className="flex items-center space-x-2 text-sm">
                      <Users className="h-4 w-4" />
                      <span>User {assignment.userId.slice(0, 8)}</span>
                      <span className="text-muted-foreground">
                        (assigned {format(new Date(assignment.assignedAt), 'MMM dd')})
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
            
            <div className="flex justify-end mt-6">
              <Button variant="outline" onClick={() => setSelectedTask(null)}>
                Close
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* User Selector for Task Assignment */}
      <UserSelector
        isOpen={isUserSelectorOpen}
        onClose={() => setIsUserSelectorOpen(false)}
        onUsersSelected={handleUsersSelected}
        title={channelContext ? `Assign Task to Channel Members` : "Assign Task to Users"}
        multiSelect={true}
        channelMembers={channelContext?.members}
        excludeUserIds={taskToAssign?.assignments?.map(a => a.userId) || []}
      />
    </div>
  );
};
