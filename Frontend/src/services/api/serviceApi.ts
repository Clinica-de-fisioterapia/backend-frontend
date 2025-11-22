import apiClient from '../apiClient';
import { Service } from '../../types';
import { API_ENDPOINTS } from '../../constants/roles';

export const serviceApi = {
  getAll: async (): Promise<Service[]> => {
    const response = await apiClient.get(API_ENDPOINTS.SERVICES);
    return response.data;
  },

  getById: async (id: string): Promise<Service> => {
    const response = await apiClient.get(`${API_ENDPOINTS.SERVICES}/${id}`);
    return response.data;
  },

  create: async (service: Omit<Service, 'id' | 'created_at'>): Promise<Service> => {
    const response = await apiClient.post(API_ENDPOINTS.SERVICES, service);
    return response.data;
  },

  update: async (id: string, service: Partial<Service>): Promise<Service> => {
    const response = await apiClient.put(`${API_ENDPOINTS.SERVICES}/${id}`, service);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`${API_ENDPOINTS.SERVICES}/${id}`);
  },
};