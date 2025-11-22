// src/services/authService.ts

import apiClient from './apiClient';
import { AuthResponse, User } from '../types';
import { API_ENDPOINTS } from '../constants/roles';

export interface LoginRequest {
  Email: string;
  Password: string;
}

export interface SignupRequest {
  CompanyName: string;
  Subdomain: string;
  AdminFullName: string;
  AdminEmail: string;
  AdminPassword: string;
}

export const authService = {
  // ********* NOVAS FUNÇÕES ADICIONADAS *********
  getAccessToken: (): string | null => {
    return localStorage.getItem('accessToken');
  },

  getTenant: (): string | null => {
    return localStorage.getItem('tenant');
  },
  // **********************************************

  login: async (
    email: string,
    password: string,
    tenant: string = 'default'
  ): Promise<AuthResponse> => {
    try {
      const response = await apiClient.post(API_ENDPOINTS.AUTH_LOGIN, {
        Email: email,
        Password: password,
      }, {
        headers: {
          'X-Tenant': tenant,
        },
    
      });
      return response.data;
    } catch (error: any) {
      throw new Error(
        error?.response?.data?.message || 'Erro ao fazer login'
      );
    }
  },

  signup: async (data: SignupRequest): Promise<void> => {
    try {
      await apiClient.post(API_ENDPOINTS.AUTH_SIGNUP, {
        CompanyName: data.CompanyName,
        Subdomain: data.Subdomain,
        AdminFullName: data.AdminFullName,
        AdminEmail: data.AdminEmail,
        AdminPassword: data.AdminPassword,
      });
    } catch (error: any) {
      throw new Error(
        error?.response?.data?.message || 'Erro ao criar conta'
      );
    }
  },

  logout: () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('tenant'); // Mantido aqui
  },

  refreshToken: async (): Promise<AuthResponse> => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      const response = await apiClient.post(API_ENDPOINTS.AUTH_REFRESH, {
        refreshToken,
      });
      return response.data;
    } catch (error: any) {
      throw new Error('Erro ao renovar token');
    }
  },

  getCurrentUser: (): User | null => {
    const user = localStorage.getItem('user');
    if (user) {
      try {
        return JSON.parse(user);
      } catch {
        return null;
      }
    }
    return null;
  },
};