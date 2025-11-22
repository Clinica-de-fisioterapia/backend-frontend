import React from 'react';

interface OptimizedCalendarProps {
  horizonDays?: number;
  onDaySelect: (date: Date) => void;
  selectedDate?: Date;
}

// Componente de Calendário Simplificado (Apenas Estrutura)
export const OptimizedCalendar: React.FC<OptimizedCalendarProps> = ({
  horizonDays = 30,
  onDaySelect,
  selectedDate,
}) => {
  const today = new Date();
  const selectedDay = selectedDate || today;

  const mockDays = Array.from({ length: 5 }).map((_, i) => {
    const date = new Date(today);
    date.setDate(today.getDate() + i);
    return date;
  });

  return (
    <div style={{
      background: 'white',
      borderRadius: '12px',
      border: '1px solid #e5e7eb',
      padding: '16px',
      overflowX: 'auto',
      whiteSpace: 'nowrap'
    }}>
      <div style={{ display: 'flex', gap: '10px' }}>
        {mockDays.map((date, index) => {
          const isSelected = date.toDateString() === selectedDay.toDateString();
          return (
            <button
              key={index}
              onClick={() => onDaySelect(date)}
              style={{
                background: isSelected ? '#2563eb' : '#f3f4f6',
                color: isSelected ? 'white' : '#111',
                padding: '10px 15px',
                borderRadius: '8px',
                border: 'none',
                cursor: 'pointer',
                transition: 'all 0.2s',
                flexShrink: 0
              }}
              onMouseEnter={(e) => { if (!isSelected) e.currentTarget.style.background = '#e5e7eb'; }}
              onMouseLeave={(e) => { if (!isSelected) e.currentTarget.style.background = '#f3f4f6'; }}
            >
              <div style={{ fontWeight: '600', fontSize: '16px' }}>
                {date.toLocaleDateString('pt-BR', { day: '2-digit' })}
              </div>
              <div style={{ fontSize: '12px', opacity: 0.8 }}>
                {date.toLocaleDateString('pt-BR', { weekday: 'short' })}
              </div>
            </button>
          );
        })}
        <div style={{ 
          display: 'flex', 
          alignItems: 'center', 
          paddingLeft: '10px', 
          color: '#6b7280', 
          fontSize: '14px' 
        }}>
          + {horizonDays - 5} dias
        </div>
      </div>
    </div>
  );
};