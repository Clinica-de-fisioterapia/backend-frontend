import { useState } from "react";
import Calendar from "react-calendar";
import "react-calendar/dist/Calendar.css";
import "./Agendamento.css";

export default function Agendamento() {
  const [dataSelecionada, setDataSelecionada] = useState<Date | null>(new Date());
  const [hora, setHora] = useState("");

  const handleAgendar = () => {
    if (!dataSelecionada) return alert("Selecione uma data!");
    if (!hora) return alert("Selecione um horário!");
    alert(`Agendado para ${dataSelecionada.toLocaleDateString()} às ${hora}`);
  };

  return (
    <div className="agenda-container">
      <div className="agenda-card">
        <h2>Agendar Horário</h2>
        <Calendar
          onChange={(value) => {
            if (!value) return;
            if (Array.isArray(value)) {
              // if range/array selection is returned, take the first date
              if (value[0]) {
                setDataSelecionada(value[0]);
              }
            } else {
              setDataSelecionada(value);
            }
          }}
          value={dataSelecionada}
          minDate={new Date()}
        />
        <div className="horario-section">
          <label>Escolha o horário:</label>
          <select value={hora} onChange={(e) => setHora(e.target.value)}>
            <option value="">Selecione...</option>
            <option value="09:00">09:00</option>
            <option value="10:00">10:00</option>
            <option value="11:00">11:00</option>
            <option value="14:00">14:00</option>
            <option value="15:00">15:00</option>
            <option value="16:00">16:00</option>
          </select>
        </div>
        <button onClick={handleAgendar}>Confirmar Agendamento</button>
      </div>
    </div>
  );
}
