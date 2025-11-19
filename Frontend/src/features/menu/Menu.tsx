import React from "react";
import { Link } from "react-router-dom";
import ApplyChangesButton from "../../components/ApplyChangesButton";

export default function Menu() {
  return (
    <div className="app-shell">
      <div style={{ display: "flex", gap: 18 }}>
        <aside className="menu-sidebar card">
          <h3>ChronoSys</h3>
          <div className="kv">Painel de recepção</div>
          <div style={{ marginTop: 12, display: "grid", gap: 8 }}>
            <Link to="/hub"><button>Hub Recepcionista</button></Link>
            <Link to="/agendamento"><button className="secondary">Agendamentos (lista)</button></Link>
            <Link to="/register"><button className="secondary">Criar conta</button></Link>
          </div>
          <div style={{ marginTop: 18 }}>
            <ApplyChangesButton />
          </div>
        </aside>

        <main className="menu-main card">
          <h2>Bem-vindo</h2>
          <p className="kv">Escolha uma opção à esquerda</p>
        </main>
      </div>
    </div>
  );
}