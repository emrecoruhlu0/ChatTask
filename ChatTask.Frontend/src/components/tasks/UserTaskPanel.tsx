import React, { useState, useEffect } from 'react';
import { Button } from '../ui/button';
import { CheckSquare, Calendar, User, AlertCircle, Clock, CheckCircle } from 'lucide-react';
import { useAppStore } from '../../store/useAppStore';
import { taskApiService } from '../../services/tasks';
import { Task, TaskStatus, TaskPriority } from '../../types';
import toast from 'react-hot-toast';

interface UserTaskPanelProps {
  isOpen: boolean;
  onClose: () => void;
}

export const UserTaskPanel: React.FC<UserTaskPanelProps> = ({ isOpen, onClose }) => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [filter, setFilter] = useState<'all' | 'pending' | 'inprogress' | 'completed'>('all');
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const { user } = useAppStore();

  useEffect(() => {
    if (isOpen && user) {
      loadUserTasks();
    }
  }, [isOpen, user]);

  const loadUserTasks = async () => {
    if (!user) return;

    setIsLoading(true);
    try {
      const userTasks = await taskApiService.getTasksByUser(user.id);
      setTasks(userTasks);
    } catch (error) {
      console.error('Failed to load user tasks:', error);
      toast.error('Failed to load your tasks');
    } finally {
      setIsLoading(false);
    }
  };

  const getStatusIcon = (status: TaskStatus | number) => {
    const statusStr = getStatusString(status);
    switch (statusStr) {
      case 'Pending': return <Clock className="w-4 h-4 text-yellow-600" />;
      case 'InProgress': return <AlertCircle className="w-4 h-4 text-blue-600" />;
      case 'Completed': return <CheckCircle className="w-4 h-4 text-green-600" />;
      default: return <Clock className="w-4 h-4 text-gray-600" />;
    }
  };

  const getPriorityColor = (priority: TaskPriority) => {
    switch (priority) {
      case 'Critical': return 'bg-red-100 text-red-800 border-red-200';
      case 'High': return 'bg-orange-100 text-orange-800 border-orange-200';
      case 'Medium': return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'Low': return 'bg-green-100 text-green-800 border-green-200';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  const getStatusString = (status: TaskStatus | number): string => {
    if (typeof status === 'number') {
      switch (status) {
        case 1: return 'Pending';
        case 2: return 'InProgress';
        case 3: return 'Completed';
        default: return 'Pending';
      }
    }
    return status.toString();
  };

  const getStatusColor = (status: TaskStatus | number) => {
    const statusStr = getStatusString(status);
    switch (statusStr) {
      case 'Pending': return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'InProgress': return 'bg-blue-100 text-blue-800 border-blue-200';
      case 'Completed': return 'bg-green-100 text-green-800 border-green-200';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  const filteredTasks = tasks.filter(task => {
    if (filter === 'all') return true;
    return getStatusString(task.status).toLowerCase() === filter;
  });

  const getFilterCount = (status: string) => {
    if (status === 'all') return tasks.length;
    return tasks.filter(task => getStatusString(task.status).toLowerCase() === status).length;
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
      <div className="bg-card rounded-lg w-full max-w-4xl max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="p-6 border-b border-border">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-3">
              <CheckSquare className="w-6 h-6 text-primary" />
              <div>
                <h2 className="text-xl font-semibold">My Tasks</h2>
                <p className="text-sm text-muted-foreground">
                  All tasks assigned to you
                </p>
              </div>
            </div>
            <Button variant="ghost" size="sm" onClick={onClose}>
              ✕
            </Button>
          </div>
        </div>

        {/* Filter Tabs */}
        <div className="p-4 border-b border-border">
          <div className="flex space-x-1 bg-muted rounded-lg p-1">
            {[
              { key: 'all', label: 'All', count: getFilterCount('all') },
              { key: 'pending', label: 'Pending', count: getFilterCount('pending') },
              { key: 'inprogress', label: 'In Progress', count: getFilterCount('inprogress') },
              { key: 'completed', label: 'Completed', count: getFilterCount('completed') }
            ].map(({ key, label, count }) => (
              <button
                key={key}
                onClick={() => setFilter(key as any)}
                className={`flex-1 px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                  filter === key
                    ? 'bg-background text-foreground shadow-sm'
                    : 'text-muted-foreground hover:text-foreground'
                }`}
              >
                {label} ({count})
              </button>
            ))}
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {isLoading ? (
            <div className="flex items-center justify-center py-12">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            </div>
          ) : filteredTasks.length === 0 ? (
            <div className="text-center py-12">
              <CheckSquare className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <h3 className="font-medium text-foreground mb-2">
                {filter === 'all' ? 'No tasks assigned to you' : `No ${filter} tasks`}
              </h3>
              <p className="text-muted-foreground">
                {filter === 'all' 
                  ? 'You don\'t have any tasks assigned yet'
                  : `You don't have any ${filter} tasks`
                }
              </p>
            </div>
          ) : (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {filteredTasks.map((task) => (
                <div
                  key={task.id}
                  className="border rounded-lg p-4 hover:shadow-md transition-shadow bg-background cursor-pointer"
                  onClick={() => setSelectedTask(task)}
                >
                  <div className="flex items-start justify-between mb-3">
                    <div className="flex items-center space-x-2">
                      {getStatusIcon(task.status)}
                      <h3 className="font-medium text-foreground line-clamp-2">
                        {task.title}
                      </h3>
                    </div>
                  </div>

                  {task.description && (
                    <p className="text-sm text-muted-foreground mb-3 line-clamp-3">
                      {task.description}
                    </p>
                  )}

                  <div className="space-y-2">
                    {/* Priority */}
                    <div className="flex items-center space-x-2">
                      <span className="text-xs font-medium text-muted-foreground">Priority:</span>
                      <span className={`px-2 py-1 rounded-full text-xs font-medium border ${getPriorityColor(task.priority)}`}>
                        {task.priority}
                      </span>
                    </div>

                    {/* Status */}
                    <div className="flex items-center space-x-2">
                      <span className="text-xs font-medium text-muted-foreground">Status:</span>
                      <span className={`px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(task.status)}`}>
                        {getStatusString(task.status)}
                      </span>
                    </div>

                    {/* Due Date */}
                    <div className="flex items-center space-x-2">
                      <Calendar className="w-3 h-3 text-muted-foreground" />
                      <span className="text-xs text-muted-foreground">
                        Due: {new Date(task.dueDate).toLocaleDateString()}
                      </span>
                    </div>

                    {/* Assignments */}
                    {task.assignments && task.assignments.length > 0 && (
                      <div className="flex items-center space-x-2">
                        <User className="w-3 h-3 text-muted-foreground" />
                        <span className="text-xs text-muted-foreground">
                          {task.assignments.length} assignee{task.assignments.length > 1 ? 's' : ''}
                        </span>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="p-4 border-t border-border">
          <div className="flex justify-between items-center text-sm text-muted-foreground">
            <span>Total: {filteredTasks.length} task{filteredTasks.length !== 1 ? 's' : ''}</span>
            <Button variant="outline" size="sm" onClick={loadUserTasks}>
              Refresh
            </Button>
          </div>
        </div>
      </div>

      {/* Task Detail Dialog */}
      {selectedTask && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="bg-card rounded-lg p-6 w-full max-w-md">
            <div className="flex items-start justify-between mb-4">
              <div>
                <h2 className="text-lg font-semibold">{selectedTask.title}</h2>
                <div className="flex items-center space-x-2 mt-2">
                  {getStatusIcon(selectedTask.status)}
                  <span className={`px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(selectedTask.status)}`}>
                    {getStatusString(selectedTask.status)}
                  </span>
                </div>
              </div>
              <Button variant="ghost" size="sm" onClick={() => setSelectedTask(null)}>
                ✕
              </Button>
            </div>
            
            {selectedTask.description && (
              <p className="text-muted-foreground mb-4">{selectedTask.description}</p>
            )}
            
            <div className="space-y-3">
              <div className="flex items-center space-x-2">
                <span className="text-sm font-medium text-muted-foreground">Priority:</span>
                <span className={`px-2 py-1 rounded-full text-xs font-medium border ${getPriorityColor(selectedTask.priority)}`}>
                  {selectedTask.priority}
                </span>
              </div>

              <div className="flex items-center space-x-2">
                <Calendar className="w-4 h-4 text-muted-foreground" />
                <span className="text-sm text-muted-foreground">
                  Due: {new Date(selectedTask.dueDate).toLocaleDateString()}
                </span>
              </div>

              <div className="flex items-center space-x-2">
                <span className="text-sm font-medium text-muted-foreground">Created:</span>
                <span className="text-sm text-muted-foreground">
                  {new Date(selectedTask.createdAt).toLocaleDateString()}
                </span>
              </div>

              {selectedTask.assignments && selectedTask.assignments.length > 0 && (
                <div className="flex items-center space-x-2">
                  <User className="w-4 h-4 text-muted-foreground" />
                  <span className="text-sm text-muted-foreground">
                    {selectedTask.assignments.length} assignee{selectedTask.assignments.length > 1 ? 's' : ''}
                  </span>
                </div>
              )}

              {selectedTask.isPrivate && (
                <div className="flex items-center space-x-2">
                  <span className="text-sm text-orange-600 font-medium">🔒 Private Task</span>
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
