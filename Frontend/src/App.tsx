import React from 'react';
import { RouterProvider } from 'react-router-dom';
import { AuthProvider } from './providers/AuthProvider';
import { QueryProvider } from './providers/QueryProvider';
import { ThemeProvider } from './providers/ThemeProvider';
import router from './router';

const App: React.FC = () => {
  return (
    <AuthProvider>
      <QueryProvider>
        <ThemeProvider>
          <RouterProvider router={router} />
        </ThemeProvider>
      </QueryProvider>
    </AuthProvider>
  );
};

export default App;