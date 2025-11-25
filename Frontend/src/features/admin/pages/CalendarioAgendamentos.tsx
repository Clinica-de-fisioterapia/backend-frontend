import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';
import { authService } from '../../../services/authService';
import { Header } from '../../../components/common/Header';
import { Sidebar } from '../../../components/common/Sidebar';
import { CalendarioAvancado } from '../../calendar/CalendarioAvancado';

export default function CalendarioAgendamentos() {
  const navigate = useNavigate();
  const { user, clearAuth } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const handleLogout = () => {
    authService.logout();
    clearAuth();
    navigate('/login');
  };

  const handleNavigate = (view: string) => {
    const routes: Record<string, string> = {
      'dashboard': '/admin/dashboard',
      'bookings': '/admin/calendar',
      'professionals': '/admin/professionals',
      'customers': '/admin/customers',
      'units': '/admin/units',
      'settings': '/admin/settings',
    };
    
    const route = routes[view];
    if (route) {
      navigate(route);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100vh', background: '#f9fafb' }}>
      <Header 
        user={user} 
        onLogout={handleLogout} 
        onMenuToggle={() => setSidebarOpen(!sidebarOpen)} 
      />
      <div style={{ display: 'flex', flex: 1 }}> {/* CORRIGIDO: Removido overflow: 'hidden' */}
        <Sidebar 
          isOpen={sidebarOpen} 
          role="admin" 
          onNavigate={handleNavigate} 
        />
        <main style={{ flex: 1, overflowY: 'auto', padding: '24px' }}> {/* CORRIGIDO: Padding adicionado */}
          <CalendarioAvancado />
        </main>
      </div>
    </div>
  );
}