import React, { useState } from "react";
import CalendarioMensal from "../calendar/CalendarioMensal";
import AgendaDiaria from "../scheduling/AgendaDiaria";

export default function HubRecepcionista() {
  const [selectedDate, setSelectedDate] = useState<Date>(new Date());
  const [selectedProfessional, setSelectedProfessional] = useState<string | null>(null);

  return (
    <div className="hub-container app-shell">
      <div className="hub-left">
        <div className="card">
          <h4>Calendários</h4>
          <CalendarioMensal horizonDays={60} onDayClick={(d) => setSelectedDate(d)} selectedDate={selectedDate} />
        </div>
      </div>

      <div className="hub-right">
        <div className="card">
          <h4>Agenda do dia</h4>
          <AgendaDiaria date={selectedDate} selectedProfessional={selectedProfessional} onSelectProfessional={setSelectedProfessional} />
        </div>
      </div>
    </div>
  );
}