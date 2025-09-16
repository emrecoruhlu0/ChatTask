import React, { useState } from 'react';
import { Button } from '../ui/button';
import { authService } from '../../services/auth';
import { useAppStore } from '../../store/useAppStore';
import { Loader2, MessageSquare } from 'lucide-react';
import toast from 'react-hot-toast';

export const Login: React.FC = () => {
  const [loginData, setLoginData] = useState({ userName: '', password: '' });
  const [registerData, setRegisterData] = useState({ name: '', email: '', password: '' });
  const [isLoading, setIsLoading] = useState(false);
  const { setUser, setLoading } = useAppStore();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!loginData.userName || !loginData.password) {
      toast.error('Please fill in all fields');
      return;
    }

    setIsLoading(true);
    setLoading(true);
    
    try {
      const user = await authService.login(loginData);
      setUser(user);
      toast.success(`Welcome back, ${user.name}!`);
    } catch (error: any) {
      console.error('Login failed:', error);
      toast.error(error.response?.data?.message || 'Login failed. Please check your credentials.');
    } finally {
      setIsLoading(false);
      setLoading(false);
    }
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!registerData.name || !registerData.email || !registerData.password) {
      toast.error('Please fill in all fields');
      return;
    }

    setIsLoading(true);
    setLoading(true);
    
    try {
      const user = await authService.register(registerData);
      setUser(user);
      toast.success(`Welcome, ${user.name}! Your account has been created.`);
    } catch (error: any) {
      console.error('Registration failed:', error);
      toast.error(error.response?.data?.message || 'Registration failed. Please try again.');
    } finally {
      setIsLoading(false);
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-chat-secondary flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="flex items-center justify-center mb-4">
            <div className="p-3 bg-gradient-to-r from-primary to-chat-primary rounded-2xl shadow-lg">
              <MessageSquare className="h-8 w-8 text-white" />
            </div>
          </div>
          <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-chat-primary bg-clip-text text-transparent">
            ChatTask
          </h1>
          <p className="text-muted-foreground mt-2">
            Connect, communicate, and collaborate
          </p>
        </div>

        {/* Auth Form */}
        <div className="bg-card rounded-2xl shadow-xl border p-8">
          <div className="text-center mb-6">
            <h2 className="text-2xl font-semibold">Get Started</h2>
            <p className="text-muted-foreground text-sm mt-1">
              Sign in to your account or create a new one
            </p>
          </div>
          
          <div className="space-y-6">
            {/* Login Form */}
            <form onSubmit={handleLogin} className="space-y-4">
              <div className="space-y-2">
                <label className="text-sm font-medium">Username</label>
                <input
                  type="text"
                  className="w-full px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  value={loginData.userName}
                  onChange={(e) => setLoginData({ ...loginData, userName: e.target.value })}
                  disabled={isLoading}
                  required
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Password</label>
                <input
                  type="password"
                  className="w-full px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  value={loginData.password}
                  onChange={(e) => setLoginData({ ...loginData, password: e.target.value })}
                  disabled={isLoading}
                  required
                />
              </div>
              <Button type="submit" className="w-full" disabled={isLoading}>
                {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Sign In
              </Button>
            </form>

            {/* Register Form */}
            <form onSubmit={handleRegister} className="space-y-4">
              <div className="space-y-2">
                <label className="text-sm font-medium">Full Name</label>
                <input
                  type="text"
                  className="w-full px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  value={registerData.name}
                  onChange={(e) => setRegisterData({ ...registerData, name: e.target.value })}
                  disabled={isLoading}
                  required
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Email Address</label>
                <input
                  type="email"
                  className="w-full px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  value={registerData.email}
                  onChange={(e) => setRegisterData({ ...registerData, email: e.target.value })}
                  disabled={isLoading}
                  required
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Password</label>
                <input
                  type="password"
                  className="w-full px-3 py-2 border border-input rounded-md bg-background focus:outline-none focus:ring-2 focus:ring-ring"
                  value={registerData.password}
                  onChange={(e) => setRegisterData({ ...registerData, password: e.target.value })}
                  disabled={isLoading}
                  required
                />
              </div>
              <Button type="submit" variant="outline" className="w-full" disabled={isLoading}>
                {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Create Account
              </Button>
            </form>
          </div>
        </div>

        {/* Demo Info */}
        <div className="mt-6 p-4 bg-muted/50 rounded-lg border">
          <div className="text-center">
            <h3 className="font-medium text-sm mb-2">Demo Credentials</h3>
            <p className="text-xs text-muted-foreground">
              Username: testuser<br />
              Password: testpass
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
