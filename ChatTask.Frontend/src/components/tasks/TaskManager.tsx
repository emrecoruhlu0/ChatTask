import React, { useState, useEffect } from 'react';
import { Button } from '../ui/button';
import { useAppStore } from '../../store/useAppStore';
import { taskApiService } from '../../services/tasks';
import { 
  Plus, 
  Clock, 
  AlertCircle, 
  CheckSquare, 
  Loader2,
  Calendar,
  Users,
  Filter
} from 'lucide-react';
import { format } from 'date-fns';
import { Task, CreateTaskRequest, TaskStatus, TaskPriority } from '../../types';
import toast from 'react-hot-toast';

export const TaskManager: React.FC = () => {
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

  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const [activeTab, setActiveTab] = useState<'all' | 'my'>('my');

  // Form state for creating tasks
  const [newTask, setNewTask] = useState<Partial<CreateTaskRequest>>({
    title: '',
    description: '',
    status: 'New',
    priority: 'Medium',
    dueDate: new Date().toISOString().split('T')[0],
    assigneeIds: []
  });

  useEffect(() => {
    loadTasks();
  }, [user]);

  const loadTasks = async () => {
    if (!user) return;
    
    setLoadingTasks(true);
    try {
      const [allTasks, myTasks] = await Promise.all([
        taskApiService.getAllTasks(),
        taskApiService.getTasksByUserId(user.id)
      ]);
      
      setTasks(allTasks);
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
      const taskData: CreateTaskRequest = {
        title: newTask.title!,
        description: newTask.description || '',
        status: newTask.status as TaskStatus,
        priority: newTask.priority as TaskPriority,
        dueDate: new Date(newTask.dueDate + 'T00:00:00Z').toISOString(),
        assigneeIds: [user.id] // Assign to current user by default
      };

      const createdTask = await taskApiService.createTask(taskData);
      addTask(createdTask);
      setIsCreateDialogOpen(false);
      setNewTask({
        title: '',
        description: '',
        status: 'New',
        priority: 'Medium',
        dueDate: new Date().toISOString().split('T')[0],
        assigneeIds: []
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

  const getPriorityColor = (priority: TaskPriority) => {
    switch (priority) {
      case 'Urgent': return 'destructive';
      case 'High': return 'default';
      case 'Medium': return 'secondary';
      case 'Low': return 'outline';
      default: return 'secondary';
    }
  };

  const getStatusIcon = (status: TaskStatus) => {
    switch (status) {
      case 'New': return Clock;
      case 'InProgress': return AlertCircle;
      case 'Done': return CheckSquare;
      default: return Clock;
    }
  };

  const getStatusColor = (status: TaskStatus) => {
    switch (status) {
      case 'New': return 'text-muted-foreground';
      case 'InProgress': return 'text-warning';
      case 'Done': return 'text-success';
      default: return 'text-muted-foreground';
    }
  };

  const TaskCard: React.FC<{ task: Task }> = ({ task }: { task: Task }) => {
    const StatusIcon = getStatusIcon(task.status);
    const isOverdue = new Date(task.dueDate) < new Date() && task.status !== 'Done';

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
          <Button variant={getPriorityColor(task.priority)} size="sm">
            {task.priority}
          </Button>
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
            All Tasks ({tasks.length})
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

            {/* All Tasks */}
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
                    <option value="Urgent">Urgent</option>
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
                  <option value="New">New</option>
                  <option value="InProgress">In Progress</option>
                  <option value="Done">Done</option>
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
    </div>
  );
};
