import { createStore } from 'zustand';

interface UserState {
    user: User | null;
    setUser: (user: User | null) => void;
}

interface User {
    id: string;
    fullName: string;
    email: string;
    role: string;
}

export const useUserStore = createStore<UserState>((set) => ({
    user: null,
    setUser: (user) => set({ user }),
}));