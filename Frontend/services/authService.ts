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
  AccessToken: string;
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
      // envia o payload com as propriedades que o backend espera (Email / Password)
      const payload = { Email: email, Password: password };

      const response = await apiClient.post<LoginResponse>(
        `/auth/login`,
        payload,
        // backend espera header "X-Tenant"
        { headers: { 'X-Tenant': tenant } }
      );

      const { AccessToken, RefreshToken, User, ExpiresAtUtc } = (response.data as any);

      // adaptação caso o backend retorne nomes diferentes
      const accessToken = response.data.accessToken ?? response.data.accessToken ?? (response.data as any).accessToken;
      const refreshToken = response.data.refreshToken ?? response.data.refreshToken ?? (response.data as any).refreshToken;
      const userId = response.data.userId ?? (response.data as any).user?.id ?? (response.data as any).User?.Id ?? (response.data as any).User?.Id;
      const userEmail = response.data.email ?? (response.data as any).user?.email ?? (response.data as any).User?.Email;
      const expiresIn = response.data.expiresIn ?? 0;

      // Armazena os tokens e dados do usuário (valide que accessToken/refreshToken existem)
      if (!accessToken || !refreshToken) {
        // tenta extrair mensagem do corpo se houver
        const body = response.data as any;
        console.warn('[authService] login: tokens não encontrados na resposta', body);
        throw new Error('Resposta de autenticação inválida do servidor.');
      }

      this.setTokens(accessToken, refreshToken, userId ?? '', userEmail ?? '', tenant);

      return {
        AccessToken: accessToken,
        accessToken,
        refreshToken,
        userId: userId ?? '',
        email: userEmail ?? '',
        expiresIn
      };
    } catch (error: any) {
      // preferir mensagens claras do servidor (error / message / title / detail)
      const resp = error?.response?.data;
      const serverMsg =
        resp?.error ||
        resp?.message ||
        resp?.title ||
        resp?.detail ||
        (typeof resp === 'string' ? resp : null);

      if (serverMsg) {
        throw new Error(String(serverMsg));
      }

      if (error?.response) {
        // resposta recebida, código 4xx/5xx, mas sem body legível
        throw new Error(`Erro ao fazer login (status: ${error.response.status})`);
      }

      // sem resposta -> erro de rede/proxy
      if (error?.request || error?.message) {
        throw new Error('Falha de conexão com o backend. Verifique se o servidor está rodando em http://localhost:5238');
      }

      throw new Error('Erro desconhecido ao fazer login');
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
        { RefreshToken: refreshToken },
        { headers: { 'X-Tenant': tenant } }
      );

      const accessToken = response.data.accessToken ?? (response.data as any).AccessToken;
      if (!accessToken) throw new Error('Resposta inválida do servidor ao renovar token.');

      localStorage.setItem(this.storageKeys.accessToken, accessToken);

      return accessToken;
    } catch (error) {
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
    if (userId) localStorage.setItem(this.storageKeys.userId, userId);
    if (email) localStorage.setItem(this.storageKeys.email, email);
    if (tenant) localStorage.setItem(this.storageKeys.tenant, tenant);
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

      // envia header X-Tenant (o controller usa X-Tenant)
      await apiClient.post(`/auth/signup`, payload, {
        headers: { 'X-Tenant': data.tenant ?? 'default' }
      });
    } catch (error: any) {
      if (error?.response) {
        const resp = error.response.data;
        const respMsg = resp?.error || resp?.message || resp?.title || resp?.detail || (typeof resp === 'string' ? resp : null);
        const status = error.response.status;
        if (respMsg) throw new Error(String(respMsg));
        if (status === 409) throw new Error('Este email já está cadastrado');
        throw new Error(`Erro ao criar conta (status: ${status ?? 'N/A'})`);
      }

      if (error?.request || error?.message) {
        throw new Error('Falha de conexão com o backend. Verifique se o servidor está rodando em http://localhost:5238');
      }

      throw new Error('Erro desconhecido ao criar conta');
    }
  }
}

export const authService = new AuthService();