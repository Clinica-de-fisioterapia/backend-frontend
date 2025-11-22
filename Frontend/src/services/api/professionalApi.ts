import apiClient from '../apiClient';
import { Professional } from '../../types';
import { API_ENDPOINTS } from '../../constants/roles';

export const professionalApi = {
  getAll: async (): Promise<Professional[]> => {
    const response = await apiClient.get(API_ENDPOINTS.PROFESSIONALS);
    return response.data;
  },

  getById: async (id: string): Promise<Professional> => {
    const response = await apiClient.get(`${API_ENDPOINTS.PROFESSIONALS}/${id}`);
    return response.data;
  },

  create: async (professional: Omit<Professional, 'id' | 'created_at'>): Promise<Professional> => {
    const response = await apiClient.post(API_ENDPOINTS.PROFESSIONALS, professional);
    return response.data;
  },

  update: async (id: string, professional: Partial<Professional>): Promise<Professional> => {
    const response = await apiClient.put(`${API_ENDPOINTS.PROFESSIONALS}/${id}`, professional);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`${API_ENDPOINTS.PROFESSIONALS}/${id}`);
  },
};