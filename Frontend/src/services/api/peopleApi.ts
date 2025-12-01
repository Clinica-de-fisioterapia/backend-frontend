import apiClient from '../apiClient';
import { Person } from '../../types';
import { API_ENDPOINTS } from '../../constants/roles';

export const peopleApi = {
  /**
   * Obtém todas as pessoas.
   */
  getAll: async (): Promise<Person[]> => {
    const response = await apiClient.get(API_ENDPOINTS.PEOPLE);
    return response.data;
  },

  /**
   * Obtém uma pessoa pelo ID.
   */
  getById: async (id: string): Promise<Person> => {
    const response = await apiClient.get(`${API_ENDPOINTS.PEOPLE}/${id}`);
    return response.data;
  },

  /**
   * Cria uma nova pessoa.
   */
  create: async (person: {
    fullName: string;
    cpf?: string;
    phone?: string;
    email?: string;
  }): Promise<{ id: string }> => {
    const response = await apiClient.post(API_ENDPOINTS.PEOPLE, person);
    return response.data;
  },

  /**
   * Atualiza parcialmente uma pessoa pelo ID.
   */
  update: async (id: string, person: {
    fullName?: string;
    cpf?: string;
    phone?: string;
    email?: string;
  }): Promise<Person> => {
    const response = await apiClient.put(`${API_ENDPOINTS.PEOPLE}/${id}`, person);
    return response.data;
  },

  /**
   * Deleta uma pessoa pelo ID.
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`${API_ENDPOINTS.PEOPLE}/${id}`);
  },
};