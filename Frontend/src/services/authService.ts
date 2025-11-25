// src/services/authService.ts

import apiClient from './apiClient';
import { AuthResponse, User } from '../types';
import { API_ENDPOINTS } from '../constants/roles';

export interface LoginRequest {
  Email: string;
  Password: string;
}

export interface SignupRequest {
  companyName: string;
  subdomain: string;
  adminFullName: string;
  adminEmail: string;
  adminPassword: string;
}

export const authService = {
  // ---- GETTERS ----
  getAccessToken: (): string | null => {
    return localStorage.getItem('accessToken');
  },

  getTenant: (): string | null => {
    return localStorage.getItem('tenant');
  },

  // ---- LOGIN ----
  login: async (
    email: string,
    password: string,
    tenant: string = 'default'
  ): Promise<AuthResponse> => {
    try {
      // 🔥 IMPORTANTE: salvar tenant antes, para o interceptor funcionar
      localStorage.setItem('tenant', tenant);

      const response = await apiClient.post(
        API_ENDPOINTS.AUTH_LOGIN,
        {
          // 🔥 IMPORTANTE: .NET usa PascalCase
          Email: email,
          Password: password
        },
        {
          headers: {
            'X-Tenant': tenant
          }
        }
      );

      const data = response.data;

      // 🔥 Salvar tokens e usuário
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      localStorage.setItem('user', JSON.stringify(data.user));

      return data;
    } catch (error: any) {
      throw new Error(error?.response?.data?.message || 'Erro ao fazer login');
    }
  },

  // ---- SIGNUP ----
  signup: async (data: SignupRequest): Promise<void> => {
    try {
      await apiClient.post(API_ENDPOINTS.AUTH_SIGNUP, {
        companyName: data.companyName,
        subdomain: data.subdomain,
        adminFullName: data.adminFullName,
        adminEmail: data.adminEmail,
        adminPassword: data.adminPassword
      });
    } catch (error: any) {
      throw new Error(error?.response?.data?.message || 'Erro ao criar conta');
    }
  },

  // ---- LOGOUT ----
  logout: () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('tenant');
  },

  // ---- REFRESH TOKEN ----
  refreshToken: async (): Promise<AuthResponse> => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');

      const response = await apiClient.post(API_ENDPOINTS.AUTH_REFRESH, {
        refreshToken
      });

      const data = response.data;

      // atualizar tokens
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);

      return data;
    } catch (error: any) {
      throw new Error('Erro ao renovar token');
    }
  },

  // ---- GET USER ----
  getCurrentUser: (): User | null => {
    const user = localStorage.getItem('user');
    if (!user) return null;

    try {
      return JSON.parse(user);
    } catch {
      return null;
    }
  }
};
