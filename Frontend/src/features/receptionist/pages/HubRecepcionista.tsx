import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';
import { authService } from '../../../services/authService';
import { Header } from '../../../components/common/Header';
import { Sidebar } from '../../../components/common/Sidebar';
import { Professional } from '../../../types';
import { OptimizedCalendar } from '../components/OptimizedCalendar';
import { AgendaView } from '../components/AgendaView';

export default function HubRecepcionista() {
  const navigate = useNavigate();
  const { user, clearAuth } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [selectedDate, setSelectedDate] = useState(new Date());
  const [professionals, setProfessionals] = useState<Professional[]>([]);

  useEffect(() => {
    const mockProfessionals: Professional[] = [
      { id: 'p1', full_name: 'Dr. João Silva', email: 'joao@clinic.com', role: 'professional', is_active: true, created_at: new Date().toISOString(), person_id: 'per1', specialty: 'Dentista', registry_code: 'CRM-123' },
      { id: 'p2', full_name: 'Dra. Maria Oliveira', email: 'maria@clinic.com', role: 'professional', is_active: true, created_at: new Date().toISOString(), person_id: 'per2', specialty: 'Fisioterapeuta', registry_code: 'CREFITO-456' },
      { id: 'p3', full_name: 'Enf. Carlos Souza', email: 'carlos@clinic.com', role: 'professional', is_active: true, created_at: new Date().toISOString(), person_id: 'per3', specialty: 'Enfermeiro', registry_code: 'COREN-789' },
    ];
    setProfessionals(mockProfessionals);
  }, []);

  const handleLogout = () => {
    authService.logout();
    clearAuth();
    navigate('/login');
  };

  const handleNavigate = (view: string) => {
    const routes: Record<string, string> = {
      'bookings': '/receptionist/hub',
      'customers': '/receptionist/customers'
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
      <div style={{ display: 'flex', flex: 1, overflow: 'hidden' }}>
        <Sidebar 
          isOpen={sidebarOpen} 
          role="receptionist" 
          onNavigate={handleNavigate} 
        />
        <main style={{ 
          flex: 1, 
          overflowY: 'auto', 
          padding: '24px', 
          display: 'grid', 
          gridTemplateColumns: '2fr 1fr', 
          gap: '24px' 
        }}>
          <div>
            <h2 style={{ margin: '0 0 16px 0', fontSize: '20px', fontWeight: '600', color: '#111' }}>
              📅 Calendário de Agendamentos 
            </h2>
            <OptimizedCalendar 
              horizonDays={60} 
              onDaySelect={setSelectedDate} 
              selectedDate={selectedDate} 
            />
          </div>
          <div>
            <h2 style={{ margin: '0 0 16px 0', fontSize: '20px', fontWeight: '600', color: '#111' }}>
              📋 Agenda do Dia 
            </h2>
            <AgendaView 
              selectedDate={selectedDate} 
              professionals={professionals} 
              onBookClick={(profId, time) => { 
                const profName = professionals.find((p) => p.id === profId)?.full_name;
                alert(`Iniciando agendamento com ${profName} às ${time}`);
              }} 
            />
          </div>
        </main>
      </div>
    </div>
  );
}