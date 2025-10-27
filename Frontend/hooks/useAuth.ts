// Frontend/hooks/useAuth.ts
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';

export interface User {
  userId: string;
  email: string;
  tenant: string;
}

export const useAuth = () => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    checkAuth();
  }, []);

  const checkAuth = () => {
    const userId = authService.getUserId();
    const email = authService.getEmail();
    const tenant = authService.getTenant();

    if (userId && email && tenant) {
      setUser({ userId, email, tenant });
    } else {
      setUser(null);
    }
    
    setLoading(false);
  };

  const login = async (email: string, password: string, tenant: string) => {
    try {
      const response = await authService.login(email, password, tenant);
      setUser({
        userId: response.userId,
        email: response.email,
        tenant
      });
      return response;
    } catch (error) {
      throw error;
    }
  };

  const logout = () => {
    authService.logout();
    setUser(null);
    navigate('/login');
  };

  return {
    user,
    loading,
    isAuthenticated: !!user,
    login,
    logout,
    checkAuth
  };
};