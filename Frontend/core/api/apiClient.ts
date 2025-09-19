// Pseudo-código em apiClient.ts
import axios, { AxiosRequestConfig, AxiosInstance } from 'axios';

const api: AxiosInstance = axios.create({
    // Adicione aqui as configurações base do seu cliente, como baseURL, headers, etc.
});

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        if (error.response && error.response.status === 401) {
            const newAccessToken = await refreshAuthToken(); // Chama o endpoint de refresh
            error.config.headers['Authorization'] = `Bearer ${newAccessToken}`;
            return api.request(error.config); // Reenvia a requisição original
        }
        return Promise.reject(error);
    }
);

async function refreshAuthToken(): Promise<string> {
    throw new Error("Function not implemented.");
}

export default api;

