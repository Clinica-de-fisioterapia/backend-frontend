import apiClient from '../api/apiClient';
import { authService } from './authService';

// Attach interceptors to the shared apiClient to avoid duplicate instances.

// Ensure base apiClient exists
if (!apiClient) {
  throw new Error('apiClient not found. Check src/api/apiClient.ts');
}

apiClient.interceptors.request.use(
  (config) => {
    if (!config.headers) config.headers = {};
    const token = (authService as any).getAccessToken?.();
    const tenant = (authService as any).getTenant?.();
    if (token) (config.headers as any).Authorization = `Bearer ${token}`;
    if (tenant) (config.headers as any)['X-Tenant'] = tenant;
    return config;
  },
  (error) => Promise.reject(error)
);

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error?.response?.status === 401) {
      // optional: central logout or redirect handled in UI
      console.warn('401 - unauthorized');
    }
    return Promise.reject(error);
  }
);

export default apiClient;