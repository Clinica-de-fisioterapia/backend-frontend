import { create } from 'zustand';
import { Customer, Professional } from '../types';

interface UserStoreState {
  customers: Customer[];
  professionals: Professional[];
  setCustomers: (customers: Customer[]) => void;
  setProfessionals: (professionals: Professional[]) => void;
  addCustomer: (customer: Customer) => void;
  addProfessional: (professional: Professional) => void;
}

export const useUserStore = create<UserStoreState>((set) => ({
  customers: [],
  professionals: [],
  setCustomers: (customers) => set({ customers }),
  setProfessionals: (professionals) => set({ professionals }),
  addCustomer: (customer) =>
    set((state) => ({ customers: [...state.customers, customer] })),
  addProfessional: (professional) =>
    set((state) => ({ professionals: [...state.professionals, professional] })),
}));