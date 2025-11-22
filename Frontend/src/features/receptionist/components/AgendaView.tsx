import React from 'react';
import { Professional } from '../../../types';
import { Clock, User } from 'lucide-react';

interface AgendaViewProps {
  selectedDate: Date;
  professionals: Professional[]; // Lista de profissionais
  onBookClick: (professionalId: string, time: string) => void;
}

// Componente de Visualização de Agenda Simplificado
export const AgendaView: React.FC<AgendaViewProps> = ({ 
  selectedDate, 
  professionals, 
  onBookClick 
}) => {
  const mockSchedule = [
    { time: '09:00', professionalId: professionals[0]?.id, available: true },
    { time: '10:00', professionalId: professionals[1]?.id, available: false, customer: 'Alice Souza' },
    { time: '11:00', professionalId: professionals[0]?.id, available: true },
    { time: '14:00', professionalId: professionals[2]?.id, available: false, customer: 'Roberto Silva' },
  ].filter(item => item.professionalId);

  return (
    <div style={{ display: 'grid', gap: '12px' }}>
      <p style={{ margin: 0, color: '#6b7280', fontSize: '14px' }}>
        Agenda para: {selectedDate.toLocaleDateString('pt-BR', { weekday: 'long', day: 'numeric', month: 'long' })}
      </p>
      {mockSchedule.length === 0 ? (
        <div style={{ 
          padding: '20px', 
          textAlign: 'center', 
          color: '#6b7280', 
          background: '#fff', 
          borderRadius: '8px',
          border: '1px solid #e5e7eb'
        }}>
          Nenhum horário disponível ou agendado.
        </div>
      ) : (
        mockSchedule.map((item, index) => {
          const prof = professionals.find(p => p.id === item.professionalId);
          return (
            <div 
              key={index} 
              style={{ 
                background: item.available ? '#ecfdf5' : '#fff',
                border: `1px solid ${item.available ? '#a7f3d0' : '#e5e7eb'}`,
                padding: '12px',
                borderRadius: '8px',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                transition: 'all 0.2s'
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <Clock size={18} style={{ color: item.available ? '#047857' : '#6b7280' }} />
                <div>
                  <div style={{ fontWeight: '600', color: '#111' }}>{item.time}</div>
                  <div style={{ fontSize: '12px', color: '#6b7280' }}>
                    {prof?.full_name}
                  </div>
                </div>
              </div>
              {item.available ? (
                <button
                  onClick={() => onBookClick(item.professionalId!, item.time)}
                  style={{
                    background: '#22c55e',
                    color: 'white',
                    padding: '6px 10px',
                    borderRadius: '6px',
                    fontSize: '12px',
                    fontWeight: '500',
                  }}
                >
                  Agendar
                </button>
              ) : (
                <div style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '12px', color: '#6b7280' }}>
                  <User size={14} /> {item.customer || 'Ocupado'}
                </div>
              )}
            </div>
          );
        })
      )}
    </div>
  );
};