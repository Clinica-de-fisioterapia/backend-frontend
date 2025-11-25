import { createBrowserRouter, Navigate } from 'react-router-dom';
import { ProtectedRoute } from './components/common/ProtectedRoute';
import Login from './features/auth/pages/Login';
import Register from './features/auth/pages/Register';
import HubRecepcionista from './features/receptionist/pages/HubRecepcionista';
import GerenciarClientes from './features/receptionist/pages/GerenciarClientes';
import AdminDashboard from './features/admin/pages/AdminDashboard';
import CalendarioAgendamentos from './features/admin/pages/CalendarioAgendamentos'; // NOVO
import { CalendarioAvancado } from './features/calendar/CalendarioAvancado'; // NOVO
import GerenciarProfissionais from './features/admin/pages/GerenciarProfissionais';
import GerenciarUnidades from './features/admin/pages/GerenciarUnidades';
import GerenciarConfiguracoes from './features/admin/pages/GerenciarConfiguracoes';
import MeusAgendamentos from './features/professional/pages/MeusAgendamentos';
import Unauthorized from './pages/Unauthorized';
import NotFound from './pages/NotFound';

export const router = createBrowserRouter([
  { path: '/login', element: <Login /> },
  { path: '/register', element: <Register /> },
  
  // Root redirect
  { path: '/', element: <Navigate to="/login" replace /> },
  
  // ========== ADMIN ROUTES ==========
  {
    path: '/admin/dashboard',
    element: (
      <ProtectedRoute allowedRoles={['admin']}>
        <AdminDashboard />
      </ProtectedRoute>
    )
  },
  {
    path: '/admin/calendar', // ROTA DO CALENDÁRIO
    element: (
      <ProtectedRoute allowedRoles={['admin']}>
        <CalendarioAgendamentos />
      </ProtectedRoute>
    )
  },
  {
    path: '/admin/calendar-advanced', // ROTA DO CALENDÁRIO AVANÇADO
    element: (
      <ProtectedRoute allowedRoles={['admin']}>
        <CalendarioAvancado />
      </ProtectedRoute>
    )
  },
  {
    path: '/admin/professionals',
    element: (
      <ProtectedRoute allowedRoles={['admin']}>
        <GerenciarProfissionais />
      </ProtectedRoute>
    )
  },
  {
    path: '/admin/customers',
    element: (
      <ProtectedRoute allowedRoles={['admin']}>
        <GerenciarClientes />
      </ProtectedRoute>
    )
  },
  {
    path: '/admin/units',
    element: (
      <ProtectedRoute allowedRoles={['admin']}>
        <GerenciarUnidades />
      </ProtectedRoute>
    )
  },
  {
    path: '/admin/settings',
    element: (
      <ProtectedRoute allowedRoles={['admin']}>
        <GerenciarConfiguracoes />
      </ProtectedRoute>
    )
  },
  
  // ========== RECEPTIONIST ROUTES ==========
  {
    path: '/receptionist/hub',
    element: (
      <ProtectedRoute allowedRoles={['receptionist', 'admin']}>
        <HubRecepcionista />
      </ProtectedRoute>
    )
  },
  {
    path: '/receptionist/bookings',
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
  
  // ========== PROFESSIONAL ROUTES ==========
  {
    path: '/professional/bookings',
    element: (
      <ProtectedRoute allowedRoles={['professional']}>
        <MeusAgendamentos />
      </ProtectedRoute>
    )
  },
  {
    path: '/professional/my-bookings',
    element: (
      <ProtectedRoute allowedRoles={['professional']}>
        <MeusAgendamentos />
      </ProtectedRoute>
    )
  },
  
  // ========== ERROR ROUTES ==========
  { path: '/unauthorized', element: <Unauthorized /> },
  { path: '*', element: <NotFound /> }
]);

export default router;