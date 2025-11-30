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
   * Cria um novo profissional.
   * A função 'create' espera um objeto Professional sem 'id' e 'created_at'.
   * Observação: A criação da 'Person' associada deve ser tratada no backend ou em uma chamada anterior/composta,
   * para seguir o padrão CRUD simples das outras APIs.
   */
  create: async (professional: Omit<Professional, 'id' | 'created_at'>): Promise<Professional> => {
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