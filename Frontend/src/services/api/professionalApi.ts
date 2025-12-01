import apiClient from '../apiClient';
import { Professional } from '../../types';
import { API_ENDPOINTS } from '../../constants/roles';

export const professionalApi = {
  /**
   * Obtém todos os profissionais.
   */
  getAll: async (): Promise<Professional[]> => {
    const response = await apiClient.get(API_ENDPOINTS.PROFESSIONALS);
    return response.data;
  },

  /**
   * Obtém um profissional pelo ID.
   */
  getById: async (id: string): Promise<Professional> => {
    const response = await apiClient.get(`${API_ENDPOINTS.PROFESSIONALS}/${id}`);
    return response.data;
  },

  /**
   * Cria um novo profissional a partir de um PersonId existente.
   */
  create: async (professional: {
    personId: string;
    specialty?: string;
  }): Promise<{ id: string }> => {
    const response = await apiClient.post(API_ENDPOINTS.PROFESSIONALS, professional);
    return response.data;
  },

  /**
   * Atualiza parcialmente um profissional pelo ID.
   */
  update: async (id: string, professional: Partial<Professional>): Promise<Professional> => {
    const response = await apiClient.put(`${API_ENDPOINTS.PROFESSIONALS}/${id}`, professional);
    return response.data;
  },

  /**
   * Deleta um profissional pelo ID.
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`${API_ENDPOINTS.PROFESSIONALS}/${id}`);
  },
};