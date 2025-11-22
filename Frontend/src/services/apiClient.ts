import axios from 'axios';
import { useAuthStore } from '../store/authStore';

const rawEnv = (import.meta as any).env?.VITE_API_URL ?? "";
export const BASE_URL = rawEnv.trim() || "http://localhost:5238/api";


export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor: Adicionar token em requisições
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  // No contexto da aplicação, o tenant pode estar no localStorage ou ser 'default'
  const tenant = localStorage.getItem('tenant') || 'default';
  
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  config.headers['X-Tenant'] = tenant;
  
  return config;
});

// Interceptor: Tratar erros de resposta
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // Redireciona para o login em caso de 401 (Não Autorizado)
    if (error?.response?.status === 401) {
      const store = useAuthStore.getState();
      store.clearAuth();
      // Navegação pura, assumindo que esta é a camada mais baixa de erro
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default apiClient;