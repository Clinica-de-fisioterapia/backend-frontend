import apiClient from '../apiClient';
import { Customer, Person } from '../../types';
import { API_ENDPOINTS } from '../../constants/roles';

export const customerApi = {
  getAll: async (): Promise<Customer[]> => {
    const response = await apiClient.get(API_ENDPOINTS.CUSTOMERS);
    return response.data;
  },

  getById: async (id: string): Promise<Customer> => {
    const response = await apiClient.get(`${API_ENDPOINTS.CUSTOMERS}/${id}`);
    return response.data;
  },

  create: async (customer: Omit<Customer, 'id' | 'created_at'>): Promise<Customer> => {
    const response = await apiClient.post(API_ENDPOINTS.CUSTOMERS, customer);
    return response.data;
  },

  update: async (id: string, customer: Partial<Customer>): Promise<Customer> => {
    const response = await apiClient.put(`${API_ENDPOINTS.CUSTOMERS}/${id}`, customer);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`${API_ENDPOINTS.CUSTOMERS}/${id}`);
  },

  createFromPerson: async (person: Person): Promise<Customer> => {
    const personResponse = await apiClient.post(API_ENDPOINTS.PEOPLE, person);
    const customerResponse = await apiClient.post(API_ENDPOINTS.CUSTOMERS, {
      person_id: personResponse.data.id,
    });
    return customerResponse.data;
  },
};