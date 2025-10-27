// Frontend/services/authService.ts
import axios from 'axios';
import { apiClient } from './apiClient';

// agora usamos apiClient.baseURL (configurado via Vite ou proxy)
const API_BASE_URL = (import.meta as any).env?.VITE_API_URL ?? '/api';

export interface LoginRequest {
  email: string;
  password: string;
  tenant: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  userId: string;
  email: string;
  expiresIn: number;
}

export interface RefreshTokenRequest {
  refreshToken: string;
  tenant: string;
}

class AuthService {
  private readonly storageKeys = {
    accessToken: 'chronosystem_access_token',
    refreshToken: 'chronosystem_refresh_token',
    userId: 'chronosystem_user_id',
    email: 'chronosystem_email',
    tenant: 'chronosystem_tenant'
  };

  async login(email: string, password: string, tenant: string): Promise<LoginResponse> {
    try {
      const response = await apiClient.post<LoginResponse>(
        `/auth/login`,
        { email, password, tenant },
        { headers: { 'X-Tenant-Id': tenant } }
      );

      const { accessToken, refreshToken, userId, email: userEmail, expiresIn } = response.data;

      // Armazena os tokens e dados do usuário
      this.setTokens(accessToken, refreshToken, userId, userEmail, tenant);

      return response.data;
    } catch (error: any) {
      const msg = error?.response?.data?.message
        || `Erro ao fazer login (status: ${error?.response?.status ?? 'N/A'})`;
      throw new Error(msg);
    }
  }

  async refreshAccessToken(): Promise<string> {
    const refreshToken = this.getRefreshToken();
    const tenant = this.getTenant();

    if (!refreshToken || !tenant) {
      throw new Error('Token de refresh ou tenant não encontrado');
    }

    try {
      const response = await apiClient.post<{ accessToken: string; expiresIn: number }>(
        `/auth/refresh`,
        { refreshToken, tenant },
        { headers: { 'X-Tenant-Id': tenant } }
      );

      const { accessToken } = response.data;
      localStorage.setItem(this.storageKeys.accessToken, accessToken);

      return accessToken;
    } catch (error) {
      // Se falhar o refresh, faz logout
      this.logout();
      throw new Error('Sessão expirada. Faça login novamente.');
    }
  }

  logout(): void {
    Object.values(this.storageKeys).forEach(key => {
      localStorage.removeItem(key);
    });
  }

  private setTokens(
    accessToken: string,
    refreshToken: string,
    userId: string,
    email: string,
    tenant: string
  ): void {
    localStorage.setItem(this.storageKeys.accessToken, accessToken);
    localStorage.setItem(this.storageKeys.refreshToken, refreshToken);
    localStorage.setItem(this.storageKeys.userId, userId);
    localStorage.setItem(this.storageKeys.email, email);
    localStorage.setItem(this.storageKeys.tenant, tenant);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.storageKeys.accessToken);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.storageKeys.refreshToken);
  }

  getUserId(): string | null {
    return localStorage.getItem(this.storageKeys.userId);
  }

  getEmail(): string | null {
    return localStorage.getItem(this.storageKeys.email);
  }

  getTenant(): string | null {
    return localStorage.getItem(this.storageKeys.tenant);
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }
  async register(data: {
    companyName: string;
    subdomain: string;
    adminFullName: string;
    adminEmail: string;
    adminPassword: string;
    tenant?: string;
  }): Promise<void> {
    try {
      const payload = {
        CompanyName: data.companyName,
        Subdomain: data.subdomain,
        AdminFullName: data.adminFullName,
        AdminEmail: data.adminEmail,
        AdminPassword: data.adminPassword
      };

      // usa apiClient que aponta para /api (ou VITE_API_URL se definido)
      await apiClient.post(`/auth/signup`, payload, {
        headers: { 'X-Tenant': data.tenant ?? 'default' }
      });
    } catch (error: any) {
      // Caso haja resposta do servidor (500, 400, etc.)
      if (error?.response) {
        const resp = error.response.data;
        const respMsg = resp?.error || resp?.message || (typeof resp === 'string' ? resp : null);
        const status = error.response.status;
        if (respMsg) throw new Error(String(respMsg));
        if (status === 409) throw new Error('Este email já está cadastrado');
        throw new Error(`Erro ao criar conta (status: ${status ?? 'N/A'})`);
      }

      // Caso não haja resposta => erro de rede / proxy / backend offline
      if (error?.request || error?.message) {
        // Mensagem clara para o desenvolvedor/usuário
        throw new Error('Falha de conexão com o backend. Verifique se o servidor está rodando em http://localhost:5238');
      }

      // fallback
      throw new Error('Erro desconhecido ao criar conta');
    }
  }
}

export const authService = new AuthService();