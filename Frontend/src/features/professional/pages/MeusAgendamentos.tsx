import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';
import { authService } from '../../../services/authService';
import { Header } from '../../../components/common/Header';
import { Sidebar } from '../../../components/common/Sidebar';
import { DollarSign, MapPin, Clock } from 'lucide-react';
import { Booking } from '../../../types';

const mockAgendamentos: (Booking & { 
  patient: string; 
  time: string; 
  date: string; 
  service: string; 
  unit: string; 
})[] = [
  {
    id: '1',
    professional_id: 'p-user',
    customer_id: 'c1',
    service_id: 's1',
    unit_id: 'u1',
    start_time: '2024-01-15T09:00:00Z',
    end_time: '2024-01-15T09:45:00Z',
    status: 'confirmed',
    created_at: '2024-01-10T10:00:00Z',
    patient: 'João da Silva',
    time: '09:00',
    date: '2024-01-15',
    service: 'Consulta Inicial',
    unit: 'Unidade Principal',
  },
  {
    id: '2',
    professional_id: 'p-user',
    customer_id: 'c2',
    service_id: 's2',
    unit_id: 'u1',
    start_time: '2024-01-15T10:00:00Z',
    end_time: '2024-01-15T10:45:00Z',
    status: 'confirmed',
    created_at: '2024-01-10T10:00:00Z',
    patient: 'Maria Santos',
    time: '10:00',
    date: '2024-01-15',
    service: 'Avaliação',
    unit: 'Unidade Centro',
  },
  {
    id: '3',
    professional_id: 'p-user',
    customer_id: 'c3',
    service_id: 's3',
    unit_id: 'u2',
    start_time: '2024-01-15T14:00:00Z',
    end_time: '2024-01-15T14:30:00Z',
    status: 'pending',
    created_at: '2024-01-10T10:00:00Z',
    patient: 'Carlos Pedro',
    time: '14:00',
    date: '2024-01-15',
    service: 'Retorno',
    unit: 'Unidade Vila Madalena',
  },
];

export default function MeusAgendamentos() {
  const navigate = useNavigate();
  const { user, clearAuth } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [agendamentos] = useState(mockAgendamentos);

  const handleLogout = () => {
    authService.logout();
    clearAuth();
    navigate('/login');
  };

  // A navegação aqui é mínima para profissionais, apenas para o próprio hub (meus-agendamentos)
  const handleNavigate = (view: string) => {
    if (view === 'my-bookings') {
      navigate('/professional/bookings');
    }
  };

  const getStatusStyle = (status: Booking['status']) => {
    switch (status) {
      case 'confirmed':
        return { background: '#dcfce7', color: '#16a34a', label: 'Confirmado' }; // green
      case 'pending':
        return { background: '#fef3c7', color: '#d97706', label: 'Pendente' }; // yellow
      case 'cancelled':
        return { background: '#fee2e2', color: '#ef4444', label: 'Cancelado' }; // red
      case 'completed':
        return { background: '#e0f2f1', color: '#0d9488', label: 'Concluído' }; // teal
      default:
        return { background: '#f3f4f6', color: '#6b7280', label: 'Desconhecido' };
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100vh', background: '#f9fafb' }} >
      <Header 
        user={user} 
        onLogout={handleLogout} 
        onMenuToggle={() => setSidebarOpen(!sidebarOpen)} 
        showMenuButton={true}
      />
      <div style={{ display: 'flex', flex: 1, overflow: 'hidden' }}>
        <Sidebar 
          isOpen={sidebarOpen} 
          role="professional" 
          onNavigate={handleNavigate} 
        />
        <main style={{ flex: 1, overflowY: 'auto', padding: '24px' }}>
          <h2 style={{ margin: '0 0 24px 0', fontSize: '28px', fontWeight: '700', color: '#111' }} > 
            📅 Meus Agendamentos 
          </h2>
          <div style={{ display: 'grid', gap: '16px' }}>
            {agendamentos.map((ag) => {
              const statusStyle = getStatusStyle(ag.status);
              return (
                <div 
                  key={ag.id} 
                  style={{ 
                    background: 'white', 
                    borderRadius: '12px', 
                    padding: '16px', 
                    border: '1px solid #e5e7eb', 
                    display: 'grid', 
                    gridTemplateColumns: '1fr auto', 
                    gap: '20px' 
                  }} 
                >
                  <div>
                    <h3 style={{ 
                      margin: '0 0 12px 0', 
                      fontSize: '18px', 
                      fontWeight: '600', 
                      color: '#111' 
                    }}> 
                      {ag.patient} 
                    </h3>
                    <div style={{ display: 'grid', gap: '8px' }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '14px', color: '#4b5563' }}>
                        <Clock size={16} /> 
                        <span>{ag.time} em {new Date(ag.date).toLocaleDateString('pt-BR')}</span>
                      </div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '14px', color: '#4b5563' }}>
                        <DollarSign size={16} /> 
                        <span>{ag.service}</span>
                      </div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '14px', color: '#4b5563' }}>
                        <MapPin size={16} /> 
                        <span>{ag.unit}</span>
                      </div>
                    </div>
                  </div>
                  <div style={{ textAlign: 'right', display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'flex-end', gap: '8px' }}>
                    <div style={{
                      background: statusStyle.background,
                      color: statusStyle.color,
                      padding: '4px 10px',
                      borderRadius: '9999px',
                      fontSize: '12px',
                      fontWeight: '600'
                    }}>
                      {statusStyle.label}
                    </div>
                    <button
                      style={{
                        background: '#2563eb',
                        color: 'white',
                        padding: '8px 14px',
                        borderRadius: '6px',
                        fontSize: '14px',
                        fontWeight: '500',
                        transition: 'background 0.2s'
                      }}
                      onClick={() => alert(`Iniciar atendimento para ${ag.patient}`)}
                    >
                      Detalhes / Iniciar
                    </button>
                  </div>
                </div>
              );
            })}
            {agendamentos.length === 0 && (
              <div style={{ padding: '20px', textAlign: 'center', color: '#6b7280', background: '#fff', borderRadius: '8px', border: '1px solid #e5e7eb' }}>
                Nenhum agendamento encontrado.
              </div>
            )}
          </div>
        </main>
      </div>
    </div>
  );
}