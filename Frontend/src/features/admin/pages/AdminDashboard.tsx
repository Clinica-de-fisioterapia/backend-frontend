import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';
import { authService } from '../../../services/authService';
import { Header } from '../../../components/common/Header';
import { Sidebar } from '../../../components/common/Sidebar';
import {
  Calendar,
  Users,
  DollarSign,
  Clock,
} from 'lucide-react';

export default function AdminDashboard() {
  const navigate = useNavigate();
  const { user, clearAuth } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const handleLogout = () => {
    authService.logout();
    clearAuth();
    navigate('/login');
  };

  const handleNavigate = (view: string) => {
    // Mapeamento correto dos IDs do Sidebar para rotas
    const routes: Record<string, string> = {
      'dashboard': '/admin/dashboard',
      'bookings': '/admin/bookings',
      'professionals': '/admin/professionals',
      'customers': '/admin/customers',
      'units': '/admin/units',
      'settings': '/admin/settings',
    };
    
    const route = routes[view];
    if (route) {
      navigate(route);
    } else {
      console.warn(`Rota não encontrada para view: ${view}`);
    }
  };

  const cards = [
    {
      title: 'Agendamentos Hoje',
      value: 45,
      icon: <Calendar size={32} />,
      color: '#10b981',
    },
    {
      title: 'Total de Clientes',
      value: 1200,
      icon: <Users size={32} />,
      color: '#3b82f6',
    },
    {
      title: 'Faturamento Mensal',
      value: 'R$ 85.000',
      icon: <DollarSign size={32} />,
      color: '#f59e0b',
    },
    {
      title: 'Média de Atendimento',
      value: '45 min',
      icon: <Clock size={32} />,
      color: '#8b5cf6',
    },
  ];

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
          role="admin" 
          onNavigate={handleNavigate} 
        />
        <main style={{ flex: 1, overflowY: 'auto', padding: '24px' }}>
          <h2 style={{ margin: '0 0 24px 0', fontSize: '28px', fontWeight: '700', color: '#111' }}>
            📊 Dashboard Administrativo 
          </h2>
          
          <div 
            style={{ 
              display: 'grid', 
              gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', 
              gap: '20px', 
              marginBottom: '32px' 
            }}
          >
            {cards.map((card) => (
              <div 
                key={card.title} 
                style={{ 
                  background: 'white', 
                  borderRadius: '12px', 
                  padding: '24px', 
                  border: '1px solid #e5e7eb', 
                  boxShadow: '0 1px 3px rgba(0,0,0,0.05)',
                  transition: 'all 0.2s',
                  cursor: 'default'
                }}
                onMouseEnter={(e) => { 
                  e.currentTarget.style.boxShadow = '0 10px 25px rgba(0,0,0,0.1)'; 
                  e.currentTarget.style.transform = 'translateY(-4px)';
                }}
                onMouseLeave={(e) => { 
                  e.currentTarget.style.boxShadow = '0 1px 3px rgba(0,0,0,0.05)'; 
                  e.currentTarget.style.transform = 'translateY(0)';
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
                  <div style={{ 
                    fontSize: '14px', 
                    fontWeight: '500', 
                    color: '#6b7280' 
                  }}>
                    {card.title}
                  </div>
                  <div style={{ color: card.color }}>
                    {card.icon}
                  </div>
                </div>
                <div style={{ 
                  fontSize: '32px', 
                  fontWeight: '700', 
                  color: '#111' 
                }}>
                  {card.value}
                </div>
              </div>
            ))}
          </div>
          
          <h3 style={{ margin: '0 0 16px 0', fontSize: '20px', fontWeight: '600', color: '#111' }}>
            Visão Geral Rápida
          </h3>
          <div style={{ background: 'white', padding: '24px', borderRadius: '12px', border: '1px solid #e5e7eb' }}>
            <p style={{ color: '#6b7280', margin: 0 }}>
              Detalhes sobre a ocupação de unidades e o desempenho dos profissionais estarão aqui.
            </p>
          </div>

        </main>
      </div>
    </div>
  );
}