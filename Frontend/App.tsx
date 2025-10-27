// Frontend/App.tsx
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login/Login';
import Register from './pages/Register/Register';
import Menu from './features/menu/Menu'; // Sua página de menu
import ProtectedRoute from './components/ProtectedRoute';
import './services/axiosInterceptor'; // Importa o interceptor para configurar
import ErrorBoundary from './components/ErrorBoundary'; // <-- IMPORT ADICIONADO

function App() {
  return (
    // ErrorBoundary envolve toda a aplicação para capturar erros e enviar logs
    <ErrorBoundary>
      <Router>
        <Routes>
          {/* Rotas públicas */}
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          
          {/* Rotas protegidas */}
          <Route
            path="/menu"
            element={
              <ProtectedRoute>
                <Menu />
              </ProtectedRoute>
            }
          />
          
          {/* Outras rotas protegidas */}
          {/* 
          <Route
            path="/agendamentos"
            element={
              <ProtectedRoute>
                <Agendamentos />
              </ProtectedRoute>
            }
          />
          */}
          
          {/* Rota padrão redireciona para login */}
          <Route path="/" element={<Navigate to="/login" replace />} />
          
          {/* Rota 404 */}
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </Router>
    </ErrorBoundary>
  );
}

export default App;