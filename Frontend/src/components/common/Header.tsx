import React from 'react';
import { LogOut, Menu } from 'lucide-react';
import { Logo } from './Logo';
import { User } from '../../types';
import { ROLE_LABELS } from '../../constants/roles';

interface HeaderProps {
  user: User | null;
  onLogout: () => void;
  onMenuToggle?: () => void;
  showMenuButton?: boolean;
}

export const Header: React.FC<HeaderProps> = ({
  user,
  onLogout,
  onMenuToggle,
  showMenuButton = true
}) => {
  return (
    <header style={{
      background: 'white',
      borderBottom: '1px solid #e5e7eb',
      padding: '16px 24px',
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      boxShadow: '0 1px 3px rgba(0,0,0,0.05)',
      zIndex: 40
    }}>
      <div style={{
  
        display: 'flex',
        alignItems: 'center',
        gap: '20px'
      }}>
        <Logo size="medium" />
        {showMenuButton && onMenuToggle && (
          <button
            onClick={onMenuToggle}
            style={{
              background: 'none',
              border: 'none',
              cursor: 'pointer',
              color: '#6b7280',
              padding: '8px',
              borderRadius: '6px',
              display: 'flex',
        
              alignItems: 'center',
              justifyContent: 'center',
              transition: 'all 0.2s'
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.background = '#f3f4f6';
              e.currentTarget.style.color = '#111';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.background = 'none';
              e.currentTarget.style.color = '#6b7280';
            }}
          >
            <Menu size={20} />
          </button>
        )}
      </div>

      <div style={{
        display: 'flex',
        alignItems: 'center',
        gap: '16px'
      }}>
        <div 
          style={{
            textAlign: 'right',
            borderRight: '1px solid #e5e7eb',
            paddingRight: '16px'
          }}>
          <div style={{
            fontSize: '14px',
            fontWeight: '600',
            color: '#111'
       
          }}>
            {user?.full_name}
          </div>
          <div style={{
            fontSize: '12px',
            color: '#6b7280',
            marginTop: '2px'
          }}>
            {user?.role ?
              ROLE_LABELS[user.role] : 'Usuário'}
          </div>
        </div>

        <button
          onClick={onLogout}
          style={{
            background: '#ef4444',
            color: 'white',
            border: 'none',
            borderRadius: '6px',
            padding: '8px 14px',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            fontSize: '14px',
            fontWeight: '500',
        
            transition: 'all 0.2s'
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.background = '#dc2626';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.background = '#ef4444';
          }}
        >
          <LogOut size={16} />
          Sair
        </button>
      </div>
    </header>
  );
};