import apiClient from '../apiClient';
import { Unit } from '../../types';
import { API_ENDPOINTS } from '../../constants/roles';

export const unitApi = {
  getAll: async (): Promise<Unit[]> => {
    const response = await apiClient.get(API_ENDPOINTS.UNITS);
    return response.data;
  },

  getById: async (id: string): Promise<Unit> => {
    const response = await apiClient.get(`${API_ENDPOINTS.UNITS}/${id}`);
    return response.data;
  },

  create: async (unit: Omit<Unit, 'id' | 'created_at'>): Promise<Unit> => {
    const response = await apiClient.post(API_ENDPOINTS.UNITS, unit);
    return response.data;
  },

  update: async (id: string, unit: Partial<Unit>): Promise<Unit> => {
    const response = await apiClient.put(`${API_ENDPOINTS.UNITS}/${id}`, unit);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`${API_ENDPOINTS.UNITS}/${id}`);
  },
};