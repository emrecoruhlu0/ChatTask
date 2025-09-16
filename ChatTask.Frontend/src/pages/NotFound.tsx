import React from 'react';
import { Button } from '../components/ui/button';
import { MessageSquare, Home } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

const NotFound: React.FC = () => {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-background flex items-center justify-center p-4">
      <div className="text-center">
        <div className="mb-8">
          <MessageSquare className="h-24 w-24 text-muted-foreground mx-auto mb-4" />
          <h1 className="text-6xl font-bold text-foreground mb-2">404</h1>
          <h2 className="text-2xl font-semibold text-foreground mb-4">Page Not Found</h2>
          <p className="text-muted-foreground max-w-md mx-auto">
            The page you're looking for doesn't exist or has been moved.
          </p>
        </div>
        
        <Button onClick={() => navigate('/')} className="inline-flex items-center">
          <Home className="mr-2 h-4 w-4" />
          Go Home
        </Button>
      </div>
    </div>
  );
};

export default NotFound;
