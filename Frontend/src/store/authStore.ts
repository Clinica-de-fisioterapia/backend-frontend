import { create } from 'zustand';
import { User, AuthResponse } from '../types';

interface AuthStore {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  loading: boolean;
  setAuth: (data: AuthResponse) => void;
  clearAuth: () => void;
  setUser: (user: User) => void;
  setLoading: (loading: boolean) => void;
  hydrate: () => void;
}

export const useAuthStore = create<AuthStore>((set) => ({
  user: null,
  accessToken: null,
  refreshToken: null,
  isAuthenticated: false,
  loading: true,

  setAuth: (data: AuthResponse) => {
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('user', JSON.stringify(data.user));
    
    set({
      user: data.user,
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      isAuthenticated: true,
      loading: false
    });
  },

  clearAuth: () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    
    set({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      loading: false
    });
  },

  setUser: (user: User) => set({ user }),

  setLoading: (loading: boolean) => set({ loading }),

  hydrate: () => {
    const token = localStorage.getItem('accessToken');
    const user = localStorage.getItem('user');
    
    if (token && user) {
      try {
        set({
          accessToken: token,
          user: JSON.parse(user),
          isAuthenticated: true,
          loading: false
        });
      } catch (error) {
        localStorage.removeItem('user');
        set({ loading: false });
      }
    } else {
      set({ loading: false });
    }
  }
}));