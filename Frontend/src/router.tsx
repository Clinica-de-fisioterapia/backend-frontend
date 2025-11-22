import { createBrowserRouter, Navigate } from 'react-router-dom';
import { ProtectedRoute } from './components/common/ProtectedRoute';
import Login from './features/auth/pages/Login';
import Register from './features/auth/pages/Register';
import HubRecepcionista from './features/receptionist/pages/HubRecepcionista';
import GerenciarClientes from './features/receptionist/pages/GerenciarClientes';
import AdminDashboard from './features/admin/pages/AdminDashboard';
import MeusAgendamentos from './features/professional/pages/MeusAgendamentos';
import Unauthorized from './pages/Unauthorized';
import NotFound from './pages/NotFound';

export const router = createBrowserRouter([
  { path: '/login', element: <Login /> },
  { path: '/register', element: <Register /> },
  // Root redirect
  { path: '/', element: <Navigate to="/login" replace /> },
  // Receptionist Routes
  {
    path: '/receptionist/hub',
    element: (
      <ProtectedRoute allowedRoles={['receptionist', 'admin']}>
        <HubRecepcionista />
      </ProtectedRoute>
    )
  },
  {
    path: '/receptionist/customers',
    element: (
      <ProtectedRoute allowedRoles={['receptionist', 'admin']}>
        <GerenciarClientes />
      </ProtectedRoute>
    )
  },
  // Admin Routes
  {
    path: '/admin/dashboard',
    element: (
      <ProtectedRoute allowedRoles={['admin']}>
        <AdminDashboard />
      </ProtectedRoute>
    )
  },
  // Professional Routes
  {
    path: '/professional/bookings',
    element: (
      <ProtectedRoute allowedRoles={['professional']}>
        <MeusAgendamentos />
      </ProtectedRoute>
    )
  },
  // Error Routes
  { path: '/unauthorized', element: <Unauthorized /> },
  { path: '*', element: <NotFound /> }
]);

export default router;