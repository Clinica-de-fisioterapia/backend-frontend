import React, { createContext, useContext, useEffect, useState } from 'react';
import { authService } from '../services/authService';

type User = {
  email?: string;
  role?: string;
  [k: string]: any;
};

type AuthContextValue = {
  user: User | null;
  loading: boolean;
  login: (email: string, password: string, tenant?: string) => Promise<void>;
  logout: () => void;
};

export const AuthContext = createContext<AuthContextValue | null>(null);

export const AuthProvider: React.FC<{ children?: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // minimal initialization: infer user from stored email/token
    const init = async () => {
      try {
        const token = (authService as any).getAccessToken?.();
        const email = (authService as any).getEmail?.() ?? localStorage.getItem('chronosys_email');
        if (token) {
          setUser({ email });
        } else {
          setUser(null);
        }
      } catch (err) {
        setUser(null);
      } finally {
        setLoading(false);
      }
    };
    init();
  }, []);

  const login = async (email: string, password: string, tenant = 'default') => {
    await authService.login(email, password, tenant);
    setUser({ email });
  };

  const logout = () => {
    (authService as any).logout?.();
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

// helper hook export for convenience
export const useAuthProvider = () => useContext(AuthContext);
export default AuthProvider;