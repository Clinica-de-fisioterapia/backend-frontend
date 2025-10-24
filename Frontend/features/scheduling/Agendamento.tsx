import { useState } from "react";
import Calendar from "react-calendar";
import "react-calendar/dist/Calendar.css";
import "./Agendamento.css";

interface Agendamento {
  data: Date;
  hora: string;
  id: number;
}

export default function Agendamento() {
  const [dataSelecionada, setDataSelecionada] = useState<Date | null>(new Date());
  const [hora, setHora] = useState("");
  const [historico, setHistorico] = useState<Agendamento[]>([]);

  const handleAgendar = () => {
    if (!dataSelecionada) return alert("Selecione uma data!");
    if (!hora) return alert("Selecione um horário!");
    
    // Verificar se já existe um agendamento para a mesma data e hora
    const agendamentoExistente = historico.find(item => {
      const mesmaData = item.data.toDateString() === dataSelecionada.toDateString();
      const mesmaHora = item.hora === hora;
      return mesmaData && mesmaHora;
    });
    
    if (agendamentoExistente) {
      return alert("⚠️ Já existe um agendamento para esta data e horário!\nPor favor, escolha outro horário.");
    }
    
    const novoAgendamento: Agendamento = {
      data: dataSelecionada,
      hora: hora,
      id: Date.now()
    };
    
    setHistorico([...historico, novoAgendamento]);
    alert(`✅ Agendado com sucesso!\n${dataSelecionada.toLocaleDateString('pt-BR')} às ${hora}`);
    setHora("");
  };

  const removerAgendamento = (id: number) => {
    setHistorico(historico.filter(item => item.id !== id));
  };

  const horariosDisponiveis = [
    { value: "09:00", label: "09:00 - Manhã" },
    { value: "10:00", label: "10:00 - Manhã" },
    { value: "11:00", label: "11:00 - Manhã" },
    { value: "14:00", label: "14:00 - Tarde" },
    { value: "15:00", label: "15:00 - Tarde" },
    { value: "16:00", label: "16:00 - Tarde" },
  ];

  return (
    <div className="agenda-container">
      <div className="agenda-card">
        <div className="card-header">
          <div className="icon-wrapper">
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
              <line x1="16" y1="2" x2="16" y2="6"></line>
              <line x1="8" y1="2" x2="8" y2="6"></line>
              <line x1="3" y1="10" x2="21" y2="10"></line>
            </svg>
          </div>
          <h2>Agendar Horário</h2>
          <p className="subtitle">Escolha a melhor data e horário para seu atendimento</p>
        </div>

        <div className="calendar-wrapper">
          <Calendar
            onChange={(value) => {
              if (!value) return;
              if (Array.isArray(value)) {
                if (value[0]) {
                  setDataSelecionada(value[0]);
                }
              } else {
                setDataSelecionada(value);
              }
            }}
            value={dataSelecionada}
            minDate={new Date()}
            locale="pt-BR"
          />
        </div>

        <div className="horario-section">
          <label>
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <circle cx="12" cy="12" r="10"></circle>
              <polyline points="12 6 12 12 16 14"></polyline>
            </svg>
            Horário disponível
          </label>
          <div className="select-wrapper">
            <select value={hora} onChange={(e) => setHora(e.target.value)}>
              <option value="">Selecione um horário...</option>
              {horariosDisponiveis.map((horario) => (
                <option key={horario.value} value={horario.value}>
                  {horario.label}
                </option>
              ))}
            </select>
            <svg className="select-icon" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <polyline points="6 9 12 15 18 9"></polyline>
            </svg>
          </div>
        </div>

        {dataSelecionada && hora && (
          <div className="resumo-agendamento">
            <div className="resumo-item">
              <span className="resumo-label">Data selecionada:</span>
              <span className="resumo-valor">{dataSelecionada.toLocaleDateString('pt-BR', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}</span>
            </div>
            <div className="resumo-item">
              <span className="resumo-label">Horário:</span>
              <span className="resumo-valor">{hora}</span>
            </div>
          </div>
        )}

        <button className="btn-confirmar" onClick={handleAgendar}>
          <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <polyline points="20 6 9 17 4 12"></polyline>
          </svg>
          Confirmar Agendamento
        </button>
      </div>

      {historico.length > 0 && (
        <div className="historico-card">
          <div className="historico-header">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M3 3v18h18"></path>
              <path d="M18 17V9"></path>
              <path d="M13 17V5"></path>
              <path d="M8 17v-3"></path>
            </svg>
            <h3>Histórico de Agendamentos</h3>
          </div>
          <div className="historico-lista">
            {historico.map((agendamento) => (
              <div key={agendamento.id} className="historico-item">
                <div className="historico-info">
                  <div className="historico-data">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
                      <line x1="16" y1="2" x2="16" y2="6"></line>
                      <line x1="8" y1="2" x2="8" y2="6"></line>
                      <line x1="3" y1="10" x2="21" y2="10"></line>
                    </svg>
                    <span>{agendamento.data.toLocaleDateString('pt-BR')}</span>
                  </div>
                  <div className="historico-hora">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <circle cx="12" cy="12" r="10"></circle>
                      <polyline points="12 6 12 12 16 14"></polyline>
                    </svg>
                    <span>{agendamento.hora}</span>
                  </div>
                </div>
                <button 
                  className="btn-remover"
                  onClick={() => removerAgendamento(agendamento.id)}
                  title="Cancelar agendamento"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <line x1="18" y1="6" x2="6" y2="18"></line>
                    <line x1="6" y1="6" x2="18" y2="18"></line>
                  </svg>
                </button>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}