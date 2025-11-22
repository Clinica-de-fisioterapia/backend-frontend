import apiClient from '../apiClient';
import { Booking } from '../../types';
import { API_ENDPOINTS } from '../../constants/roles';

export const bookingApi = {
  getAll: async (): Promise<Booking[]> => {
    const response = await apiClient.get(API_ENDPOINTS.BOOKINGS);
    return response.data;
  },

  getById: async (id: string): Promise<Booking> => {
    const response = await apiClient.get(`${API_ENDPOINTS.BOOKINGS}/${id}`);
    return response.data;
  },

  create: async (booking: Omit<Booking, 'id' | 'created_at'>): Promise<Booking> => {
    const response = await apiClient.post(API_ENDPOINTS.BOOKINGS, booking);
    return response.data;
  },

  update: async (id: string, booking: Partial<Booking>): Promise<Booking> => {
    const response = await apiClient.put(`${API_ENDPOINTS.BOOKINGS}/${id}`, booking);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`${API_ENDPOINTS.BOOKINGS}/${id}`);
  },

  getByProfessional: async (professionalId: string): Promise<Booking[]> => {
    const response = await apiClient.get(`${API_ENDPOINTS.BOOKINGS}/professional/${professionalId}`);
    return response.data;
  },

  getByCustomer: async (customerId: string): Promise<Booking[]> => {
    const response = await apiClient.get(`${API_ENDPOINTS.BOOKINGS}/customer/${customerId}`);
    return response.data;
  },
};