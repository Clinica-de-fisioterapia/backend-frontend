import React, { useEffect } from 'react';
import { RouterProvider } from 'react-router-dom';
import { router } from './router';
import { useAuthStore } from './store/authStore';

const App: React.FC = () => {
  const hydrate = useAuthStore((state) => state.hydrate);

  useEffect(() => {
    // Recuperar dados de autenticação do localStorage ao carregar
    hydrate();
  }, [hydrate]);

  return <RouterProvider router={router} />;
};

export default App;