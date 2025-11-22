import React from 'react';
import {
  Home,
  Calendar,
  Users,
  Settings,
  BarChart3,
  ChevronRight
} from 'lucide-react';
import { UserRole } from '../../types';

interface MenuItem {
  label: string;
  icon: React.ReactNode;
  id: string;
}

interface SidebarProps {
  isOpen: boolean;
  role: UserRole;
  onNavigate: (view: string) => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ isOpen, role, onNavigate }) => {
  const menuItems: Record<UserRole, MenuItem[]> = {
    admin: [
      { label: 'Dashboard', icon: <Home size={18} />, id: 'dashboard' },
      { label: 'Agendamentos', icon: <Calendar size={18} />, id: 'bookings' },
      { label: 'Profissionais', icon: <Users size={18} />, id: 'professionals' },
      { label: 'Clientes', icon: <Users size={18} />, id: 'customers' },
      { label: 'Unidades', icon: <BarChart3 size={18} />, id: 'units' },
  
      { label: 'Configurações', icon: <Settings size={18} />, id: 'settings' }
    ],
    receptionist: [
      { label: 'Agendamentos', icon: <Calendar size={18} />, id: 'bookings' },
      { label: 'Clientes', icon: <Users size={18} />, id: 'customers' }
    ],
    professional: [
      { label: 'Meus Agendamentos', icon: <Calendar size={18} />, id: 'my-bookings' }
    ]
  };
  const items = menuItems[role];

  return (
    <aside
      style={{
        background: 'white',
        borderRight: '1px solid #e5e7eb',
        width: isOpen ? '280px' : '0',
        overflow: 'hidden',
        transition: 'width 0.3s ease, opacity 0.3s ease',
        display: 'flex',
        flexDirection: 'column',
        height: '100%',
  
        boxShadow: isOpen ? '2px 0 8px rgba(0,0,0,0.05)' : 'none',
        zIndex: 30
      }}
    >
      <nav style={{
        flex: 1,
        padding: '20px',
        display: 'flex',
        flexDirection: 'column',
        gap: '8px'
      }}>
        {items.map((item) => (
          <button
            key={item.id}
            onClick={() => onNavigate(item.id)}
            style={{
              background: 'none',
              border: 'none',
              padding: '12px 16px',
         
              borderRadius: '8px',
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              gap: '12px',
              color: '#4b5563',
              fontSize: '14px',
              fontWeight: '500',
              transition: 'all 0.2s',
              textAlign: 'left',
              width: '100%'
            }}
            onMouseEnter={(e) => {
         
              e.currentTarget.style.background = '#f3f4f6';
              e.currentTarget.style.color = '#2563eb';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.background = 'none';
              e.currentTarget.style.color = '#4b5563';
            }}
          >
            {item.icon}
            <span style={{ flex: 1 }}>{item.label}</span>
            <ChevronRight size={16} style={{ opacity: 0.5 }} />
          </button>
        ))}
      </nav>

      <div style={{
        padding: '20px',
     
        borderTop: '1px solid #e5e7eb'
      }}>
        <div style={{
          fontSize: '12px',
          color: '#9ca3af',
          marginBottom: '12px'
        }}>
          ChronoSys v1.0
        </div>
      </div>
    </aside>
  );
};