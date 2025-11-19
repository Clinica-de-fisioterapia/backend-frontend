import axios from "axios";

const rawEnv = (import.meta as any).env?.VITE_API_URL ?? "";
let BASE_URL = rawEnv.trim() || "/api";
if (rawEnv) {
  BASE_URL = BASE_URL.replace(/\/+$/, "");
}

// simple axios instance
export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { "Content-Type": "application/json" },
});

// Interceptor para adicionar o token de autenticação em todas as requisições
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token"); // ou de onde você estiver armazenando o token
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor para lidar com erros de resposta
apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    // Aqui você pode adicionar lógica para lidar com erros globais
    return Promise.reject(error);
  }
);

// Removed duplicate re-export to avoid redeclaration conflicts
export default apiClient;