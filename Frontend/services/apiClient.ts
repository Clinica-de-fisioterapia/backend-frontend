import axios from "axios";

const rawEnv = (import.meta as any).env?.VITE_API_URL ?? "";
// Se VITE_API_URL apontou direto para um endpoint (ex: .../auth/signup) removemos sufixos /auth* indesejados
let BASE_URL = rawEnv.trim() || "/api";
if (rawEnv) {
  // remove qualquer sufixo /auth ou /auth/... para evitar concatenação duplicada
  BASE_URL = BASE_URL.replace(/\/auth(\/.*)?\/?$/i, "").replace(/\/+$/, "");
  if (!BASE_URL) BASE_URL = "/api";
}

console.info("[apiClient] baseURL:", BASE_URL);

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

export default apiClient;