import React, { useState, useEffect } from 'react';
import { Calendar, Clock, User, ChevronLeft, ChevronRight } from 'lucide-react';

// Mock de dados
const PROFESSIONALS = [
  { id: '1', name: 'Dr. João Silva', role: 'Cardiologista', color: '#3b82f6' },
  { id: '2', name: 'Dra. Ana Costa', role: 'Psicóloga', color: '#8b5cf6' },
  { id: '3', name: 'Enf. Pedro Reis', role: 'Enfermeiro', color: '#10b981' }
];

interface Appointment {
  id: string;
  time: string;
  duration: number;
  title: string;
  patient: string;
  type: string;
}

const monthNames = ["Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"];
const dayNames = ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"];

export const CalendarioAvancado: React.FC = () => {
  const [selectedDate, setSelectedDate] = useState(new Date());
  const [selectedProfessional, setSelectedProfessional] = useState(PROFESSIONALS[0].id);
  const [currentMonth, setCurrentMonth] = useState(new Date());

  // Mock de disponibilidade
  const getDayAvailability = (date: Date): 'high' | 'low' | 'full' | 'none' => {
    const day = date.getDate();
    if (day === 15) return 'none';
    if (day % 7 === 1) return 'full';
    if (day % 7 === 2) return 'low';
    return 'high';
  };

  // Mock de agendamentos
  const getMockAppointments = (dateString: string, professionalId: string): Appointment[] => {
    const date = new Date(dateString);
    const dayOfWeek = date.getDay();
    if (dayOfWeek === 0 || dayOfWeek === 6) return [];

    const appointments: Appointment[] = [];
    if (professionalId === '1') {
      appointments.push({ id: '1', time: '09:00', duration: 60, title: 'Consulta Inicial', patient: 'João Silva', type: 'Presencial' });
      appointments.push({ id: '2', time: '11:00', duration: 30, title: 'Retorno', patient: 'Maria Santos', type: 'Online' });
      appointments.push({ id: '3', time: '14:30', duration: 90, title: 'Procedimento', patient: 'Carlos Oliveira', type: 'Cirurgia' });
    } else if (professionalId === '2') {
      appointments.push({ id: '4', time: '08:00', duration: 50, title: 'Terapia', patient: 'Ana Lima', type: 'Presencial' });
      appointments.push({ id: '5', time: '10:00', duration: 50, title: 'Acompanhamento', patient: 'Pedro Costa', type: 'Online' });
      appointments.push({ id: '6', time: '13:00', duration: 50, title: 'Primeira Sessão', patient: 'Julia Martins', type: 'Presencial' });
    } else {
      appointments.push({ id: '7', time: '08:30', duration: 120, title: 'Curativos', patient: 'Roberto Silva', type: 'Ambulatório' });
      appointments.push({ id: '8', time: '14:00', duration: 60, title: 'Medicação', patient: 'Fernanda Souza', type: 'Ambulatório' });
    }
    return appointments;
  };

  const renderCalendar = () => {
    const year = currentMonth.getFullYear();
    const month = currentMonth.getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingDayOfWeek = firstDay.getDay();
    
    const days = [];
    
    // Dias vazios antes do início do mês
    for (let i = 0; i < startingDayOfWeek; i++) {
      days.push(<div key={`empty-${i}`} style={{ height: '80px' }} />);
    }
    
    // Dias do mês
    for (let day = 1; day <= daysInMonth; day++) {
      const date = new Date(year, month, day);
      const isToday = date.toDateString() === new Date().toDateString();
      const isSelected = date.toDateString() === selectedDate.toDateString();
      const availability = getDayAvailability(date);
      const isPast = date < new Date(new Date().setHours(0, 0, 0, 0));
      
      let bgColor = '#ffffff';
      let textColor = '#111827';
      let borderColor = '#e5e7eb';
      
      if (isPast) {
        bgColor = '#f3f4f6';
        textColor = '#9ca3af';
      } else {
        switch (availability) {
          case 'high':
            bgColor = '#dcfce7';
            borderColor = '#bbf7d0';
            textColor = '#166534';
            break;
          case 'low':
            bgColor = '#fef3c7';
            borderColor = '#fde047';
            textColor = '#854d0e';
            break;
          case 'full':
            bgColor = '#fee2e2';
            borderColor = '#fecaca';
            textColor = '#991b1b';
            break;
          case 'none':
            bgColor = '#f3f4f6';
            textColor = '#9ca3af';
            break;
        }
      }
      
      if (isSelected) {
        bgColor = '#2563eb';
        textColor = '#ffffff';
        borderColor = '#1d4ed8';
      }
      
      days.push(
        <div
          key={day}
          onClick={() => !isPast && setSelectedDate(date)}
          style={{
            height: '80px',
            background: bgColor,
            border: `2px solid ${borderColor}`,
            borderRadius: '8px',
            padding: '8px',
            cursor: isPast ? 'not-allowed' : 'pointer',
            transition: 'all 0.2s',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            position: 'relative',
            opacity: isPast ? 0.5 : 1
          }}
          onMouseEnter={(e) => {
            if (!isPast && !isSelected) {
              e.currentTarget.style.transform = 'scale(1.05)';
              e.currentTarget.style.boxShadow = '0 4px 6px rgba(0,0,0,0.1)';
            }
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.transform = 'scale(1)';
            e.currentTarget.style.boxShadow = 'none';
          }}
        >
          {isToday && !isSelected && (
            <div style={{
              position: 'absolute',
              top: '4px',
              right: '4px',
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              background: '#2563eb'
            }} />
          )}
          <div style={{
            fontSize: '20px',
            fontWeight: '700',
            color: textColor
          }}>
            {day}
          </div>
          {!isPast && availability !== 'none' && (
            <div style={{
              fontSize: '10px',
              marginTop: '4px',
              color: textColor,
              opacity: 0.8
            }}>
              {availability === 'high' ? 'Disponível' : availability === 'low' ? 'Pouco' : 'Cheio'}
            </div>
          )}
        </div>
      );
    }
    
    return days;
  };

  const renderTimeSlots = () => {
    const dateString = selectedDate.toISOString().split('T')[0];
    const appointments = getMockAppointments(dateString, selectedProfessional);
    const START_HOUR = 8;
    const END_HOUR = 19;
    const SLOT_HEIGHT = 48;
    
    const prof = PROFESSIONALS.find(p => p.id === selectedProfessional);
    
    return (
      <div style={{ display: 'flex', height: '100%', minHeight: '600px' }}>
        {/* Coluna de horas */}
        <div style={{ width: '60px', flexShrink: 0, borderRight: '1px solid #e5e7eb' }}>
          {Array.from({ length: END_HOUR - START_HOUR }, (_, i) => (
            <div
              key={i}
              style={{
                height: `${SLOT_HEIGHT * 2}px`,
                display: 'flex',
                alignItems: 'flex-start',
                justifyContent: 'flex-end',
                paddingRight: '8px',
                paddingTop: '4px',
                fontSize: '12px',
                color: '#6b7280'
              }}
            >
              {`${(START_HOUR + i).toString().padStart(2, '0')}:00`}
            </div>
          ))}
        </div>
        
        {/* Grade de horários */}
        <div style={{ flex: 1, position: 'relative' }}>
          {/* Linhas de grade */}
          {Array.from({ length: END_HOUR - START_HOUR }, (_, i) => (
            <div
              key={i}
              style={{
                position: 'absolute',
                top: `${i * SLOT_HEIGHT * 2}px`,
                left: 0,
                right: 0,
                height: `${SLOT_HEIGHT * 2}px`,
                borderTop: '1px solid #e5e7eb',
                borderBottom: i === END_HOUR - START_HOUR - 1 ? '1px solid #e5e7eb' : 'none'
              }}
            >
              <div style={{
                position: 'absolute',
                top: `${SLOT_HEIGHT}px`,
                left: 0,
                right: 0,
                borderTop: '1px dashed #f3f4f6'
              }} />
            </div>
          ))}
          
          {/* Agendamentos */}
          {appointments.map((apt) => {
            const [hour, minute] = apt.time.split(':').map(Number);
            const startMinutes = (hour - START_HOUR) * 60 + minute;
            const topPx = (startMinutes / 30) * SLOT_HEIGHT;
            const heightPx = (apt.duration / 30) * SLOT_HEIGHT;
            
            return (
              <div
                key={apt.id}
                style={{
                  position: 'absolute',
                  top: `${topPx}px`,
                  left: '8px',
                  right: '8px',
                  height: `${heightPx}px`,
                  background: prof?.color,
                  opacity: 0.9,
                  borderRadius: '6px',
                  padding: '8px',
                  color: 'white',
                  fontSize: '12px',
                  overflow: 'hidden',
                  boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
                  cursor: 'pointer',
                  transition: 'all 0.2s',
                  zIndex: 10
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.opacity = '1';
                  e.currentTarget.style.transform = 'scale(1.02)';
                  e.currentTarget.style.zIndex = '20';
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.opacity = '0.9';
                  e.currentTarget.style.transform = 'scale(1)';
                  e.currentTarget.style.zIndex = '10';
                }}
              >
                <div style={{ fontWeight: '600' }}>{apt.time} - {apt.title}</div>
                <div style={{ fontSize: '11px', marginTop: '4px', opacity: 0.9 }}>
                  <User size={10} style={{ display: 'inline', marginRight: '4px' }} />
                  {apt.patient}
                </div>
                <div style={{ fontSize: '10px', marginTop: '2px', opacity: 0.8 }}>
                  {apt.type}
                </div>
              </div>
            );
          })}
        </div>
      </div>
    );
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%', background: '#f9fafb' }}>
      {/* Header */}
      <div style={{ padding: '24px', borderBottom: '1px solid #e5e7eb', background: 'white' }}>
        <h1 style={{ margin: '0 0 8px 0', fontSize: '28px', fontWeight: '700', color: '#111' }}>
          <Calendar size={28} style={{ display: 'inline', marginRight: '12px', verticalAlign: 'middle' }} />
          Calendário de Agendamentos
        </h1>
        <p style={{ margin: 0, color: '#6b7280' }}>Gerencie sua agenda e visualize disponibilidade</p>
      </div>
      
      {/* Conteúdo Principal */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '24px', padding: '24px', flex: 1, overflow: 'hidden' }}>
        
        {/* LADO ESQUERDO: Calendário Mensal */}
        <div style={{ background: 'white', borderRadius: '12px', border: '1px solid #e5e7eb', padding: '24px', display: 'flex', flexDirection: 'column' }}>
          {/* Controles do Calendário */}
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '24px' }}>
            <button
              onClick={() => setCurrentMonth(new Date(currentMonth.getFullYear(), currentMonth.getMonth() - 1))}
              style={{
                background: '#f3f4f6',
                border: 'none',
                borderRadius: '6px',
                padding: '8px',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center'
              }}
            >
              <ChevronLeft size={20} />
            </button>
            
            <h2 style={{ margin: 0, fontSize: '20px', fontWeight: '600' }}>
              {monthNames[currentMonth.getMonth()]} {currentMonth.getFullYear()}
            </h2>
            
            <button
              onClick={() => setCurrentMonth(new Date(currentMonth.getFullYear(), currentMonth.getMonth() + 1))}
              style={{
                background: '#f3f4f6',
                border: 'none',
                borderRadius: '6px',
                padding: '8px',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center'
              }}
            >
              <ChevronRight size={20} />
            </button>
          </div>
          
          {/* Cabeçalho dos dias */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: '4px', marginBottom: '8px' }}>
            {dayNames.map(day => (
              <div key={day} style={{ textAlign: 'center', fontSize: '12px', fontWeight: '600', color: '#6b7280', padding: '8px 0' }}>
                {day}
              </div>
            ))}
          </div>
          
          {/* Grade do Calendário */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: '4px', flex: 1 }}>
            {renderCalendar()}
          </div>
          
          {/* Legenda */}
          <div style={{ marginTop: '16px', paddingTop: '16px', borderTop: '1px solid #e5e7eb', display: 'flex', gap: '16px', flexWrap: 'wrap', fontSize: '12px' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <div style={{ width: '12px', height: '12px', borderRadius: '3px', background: '#dcfce7', border: '1px solid #bbf7d0' }} />
              <span>Disponível</span>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <div style={{ width: '12px', height: '12px', borderRadius: '3px', background: '#fef3c7', border: '1px solid #fde047' }} />
              <span>Pouco</span>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <div style={{ width: '12px', height: '12px', borderRadius: '3px', background: '#fee2e2', border: '1px solid #fecaca' }} />
              <span>Cheio</span>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <div style={{ width: '12px', height: '12px', borderRadius: '3px', background: '#f3f4f6', border: '1px solid #e5e7eb' }} />
              <span>Indisponível</span>
            </div>
          </div>
        </div>
        
        {/* LADO DIREITO: Agenda do Dia */}
        <div style={{ background: 'white', borderRadius: '12px', border: '1px solid #e5e7eb', display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
          {/* Cabeçalho da Agenda */}
          <div style={{ padding: '24px', borderBottom: '1px solid #e5e7eb' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '16px' }}>
              <Clock size={24} style={{ color: '#2563eb' }} />
              <div>
                <h3 style={{ margin: 0, fontSize: '18px', fontWeight: '600' }}>
                  {selectedDate.toLocaleDateString('pt-BR', { weekday: 'long', day: 'numeric', month: 'long' })}
                </h3>
                <p style={{ margin: '4px 0 0 0', fontSize: '14px', color: '#6b7280' }}>
                  Agenda detalhada do dia
                </p>
              </div>
            </div>
            
            {/* Seletor de Profissionais */}
            <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
              {PROFESSIONALS.map(prof => (
                <button
                  key={prof.id}
                  onClick={() => setSelectedProfessional(prof.id)}
                  style={{
                    padding: '8px 16px',
                    borderRadius: '20px',
                    border: selectedProfessional === prof.id ? `2px solid ${prof.color}` : '2px solid #e5e7eb',
                    background: selectedProfessional === prof.id ? `${prof.color}15` : 'white',
                    color: selectedProfessional === prof.id ? prof.color : '#6b7280',
                    fontSize: '14px',
                    fontWeight: '500',
                    cursor: 'pointer',
                    transition: 'all 0.2s',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '8px'
                  }}
                >
                  <div style={{
                    width: '8px',
                    height: '8px',
                    borderRadius: '50%',
                    background: prof.color
                  }} />
                  {prof.name.split(' ')[1]}
                </button>
              ))}
            </div>
          </div>
          
          {/* Grade de Horários */}
          <div style={{ flex: 1, overflow: 'auto', padding: '0' }}>
            {renderTimeSlots()}
          </div>
        </div>
      </div>
    </div>
  );
};