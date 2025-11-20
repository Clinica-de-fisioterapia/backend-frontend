import axios from "axios";

export const BASE_URL = "/api"; // relativo: Vite irá proxyar para o backend em dev

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { "Content-Type": "application/json" },
});

export default apiClient;