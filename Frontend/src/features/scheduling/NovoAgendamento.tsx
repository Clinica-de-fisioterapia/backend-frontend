import React, { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";

type Prefill = {
  unitId?: string;
  unitName?: string;
  professionalId?: string;
  professionalName?: string;
  date?: string;
  time?: string;
  serviceId?: string;
  serviceName?: string;
};

export default function NovoAgendamento() {
  const loc = useLocation();
  const navigate = useNavigate();
  const pre = (loc.state || {}) as Prefill;

  const [step, setStep] = useState(1);
  const [context, setContext] = useState<Prefill>({
    unitId: pre.unitId ?? "unit_default",
    unitName: pre.unitName ?? "Unidade Central",
    professionalId: pre.professionalId ?? "",
    professionalName: pre.professionalName ?? "",
    date: pre.date ?? new Date().toISOString(),
    time: pre.time ?? "08:00",
    serviceId: pre.serviceId ?? "service_default",
    serviceName: pre.serviceName ?? "Consulta",
  });

  const next = () => setStep((s) => Math.min(4, s + 1));
  const prev = () => setStep((s) => Math.max(1, s - 1));

  const submit = async () => {
    console.log("Criando agendamento:", context);
    navigate("/agendamento");
  };

  return (
    <div className="wizard app-shell">
      <div className="card">
        <div className="wizard-steps">
          <div className={`step ${step === 1 ? "active" : ""}`}>1 Contexto</div>
          <div className={`step ${step === 2 ? "active" : ""}`}>2 Cliente</div>
          <div className={`step ${step === 3 ? "active" : ""}`}>3 Horário</div>
          <div className={`step ${step === 4 ? "active" : ""}`}>4 Confirmação</div>
        </div>

        {step === 1 && (
          <div style={{ display: "grid", gap: 10 }}>
            <label>
              Unidade
              <input
                className="input"
                value={context.unitName}
                onChange={(e) => setContext((c) => ({ ...c, unitName: e.target.value }))}
              />
            </label>
            <label>
              Profissional
              <input
                className="input"
                value={context.professionalName}
                onChange={(e) => setContext((c) => ({ ...c, professionalName: e.target.value }))}
              />
            </label>
            <label>
              Serviço
              <input
                className="input"
                value={context.serviceName}
                onChange={(e) => setContext((c) => ({ ...c, serviceName: e.target.value }))}
              />
            </label>
            <label>
              Data
              <input
                className="input"
                type="date"
                value={new Date(context.date!).toISOString().substring(0, 10)}
                onChange={(e) => setContext((c) => ({ ...c, date: new Date(e.target.value).toISOString() }))}
              />
            </label>
            <label>
              Horário
              <input
                className="input"
                type="time"
                value={context.time}
                onChange={(e) => setContext((c) => ({ ...c, time: e.target.value }))}
              />
            </label>
          </div>
        )}

        <div style={{ display: "flex", justifyContent: "space-between", marginTop: 16 }}>
          <div>{step > 1 && <button className="secondary" onClick={prev}>Voltar</button>}</div>
          <div>
            {step < 4 && <button onClick={next}>Próximo</button>}
            {step === 4 && <button onClick={submit} style={{ marginLeft: 12 }}>Confirmar</button>}
          </div>
        </div>
      </div>
    </div>
  );
}