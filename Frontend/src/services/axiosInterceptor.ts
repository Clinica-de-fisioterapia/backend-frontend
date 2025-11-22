import apiClient from '../api/apiClient';
import { authService } from './authService';

if (!apiClient) {
  throw new Error('apiClient not found. Check src/api/apiClient.ts');
}

apiClient.interceptors.request.use(
  (config) => {
    const mutableConfig = config as any;

    if (!mutableConfig.headers) {
      mutableConfig.headers = {};
    }

    const token = authService.getAccessToken(); 
    const tenant = authService.getTenant();
    
    if (token) {
      mutableConfig.headers.Authorization = `Bearer ${token}`;
    }
    if (tenant) {
      mutableConfig.headers['X-Tenant'] = tenant;
    }

    return config;
  },
  (error) => Promise.reject(error)
);

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error?.response?.status === 401) {
      console.warn('401 - unauthorized');
    }
    return Promise.reject(error);
  }
);

export default apiClient;