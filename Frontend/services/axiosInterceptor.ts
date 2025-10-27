// Frontend/services/axiosInterceptor.ts
import { AxiosError, AxiosRequestHeaders } from 'axios';
import { apiClient } from './apiClient';
import { authService } from './authService';

let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: any) => void;
}> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach(prom => {
    if (error) prom.reject(error);
    else prom.resolve(token!);
  });
  failedQueue = [];
};

// request interceptor on apiClient
apiClient.interceptors.request.use(
  (config) => {
    const token = authService.getAccessToken();
    const tenant = authService.getTenant();

    if (!config.headers) config.headers = {} as AxiosRequestHeaders;

    if (token) {
      // padrão Bearer
      (config.headers as any).Authorization = `Bearer ${token}`;
    }

    if (tenant) {
      // backend usa "X-Tenant" em alguns endpoints; adicionamos ambos por segurança
      (config.headers as any)['X-Tenant'] = tenant;
      (config.headers as any)['X-Tenant-Id'] = tenant;
    }

    return config;
  },
  (error) => Promise.reject(error)
);

// response interceptor on apiClient
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError & { config?: any }) => {
    // Se não houve resposta, provavelmente é erro de rede / proxy / backend offline
    if (!error?.response) {
      console.error('[apiClient] Network/proxy error:', error?.message || error);
      return Promise.reject(new Error('Falha na comunicação com o servidor. Verifique se o backend (http://localhost:5238) está ativo.'));
    }

    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest?._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return apiClient(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const newToken = await authService.refreshAccessToken();
        processQueue(null, newToken);
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        authService.logout();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default apiClient;