import React from "react";
import { useNavigate } from "react-router-dom";

interface Props {
  date: Date;
  selectedProfessional?: string | null;
  onSelectProfessional?: (id: string) => void;
}

const sampleProfessionals = [
  { id: "p1", name: "Dra. Maria" },
  { id: "p2", name: "Dr. João" }
];

export default function AgendaDiaria({ date, selectedProfessional, onSelectProfessional }: Props) {
  const navigate = useNavigate();
  const times = ["08:00","08:30","09:00","09:30","10:00","10:30","11:00","11:30","13:00","13:30","14:00","14:30","15:00"];

  const onClickSlot = (professionalId: string, time: string) => {
    navigate("/novo-agendamento", {
      state: {
        unitId: "unit_default",
        unitName: "Unidade Central",
        professionalId,
        professionalName: sampleProfessionals.find(p => p.id === professionalId)?.name,
        date: date.toISOString(),
        time,
        serviceId: "service_default",
        serviceName: "Consulta"
      }
    });
  };

  return (
    <div style={{ display: "flex", gap: 12 }}>
      {(selectedProfessional ? sampleProfessionals.filter(p => p.id === selectedProfessional) : sampleProfessionals).map(prof => (
        <div key={prof.id} className="agenda-column">
          <div className="prof-name">{prof.name}</div>
          <div style={{ display: "grid", gap: 8 }}>
            {times.map(t => (
              <button key={t} onClick={() => onClickSlot(prof.id, t)} className="slot">
                <div className="time">{t}</div>
                <div className="kv">Consulta — 30min</div>
              </button>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}