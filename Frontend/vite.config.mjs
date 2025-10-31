import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // redireciona /api para o backend em desenvolvimento (porta correta: 5238)
      '/api': {
        target: 'http://localhost:5238',
        changeOrigin: true,
        secure: false,
      },
    },
  },
  base: 'https://Clinica-de-fisioterapia.github.io/backend-frontend',
});
