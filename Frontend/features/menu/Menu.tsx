import React from "react";
import { Link } from "react-router-dom";
import "./Menu.css";

export default function Menu() {
  return (
    <div className="menu-container">
      <div className="menu-card">
        <h2>Menu</h2>
        <p>Escolha uma ação para continuar</p>

        <div className="menu-actions">
          <Link className="menu-button primary" to="/agendamento">
            Agendar Horário
          </Link>

          <Link className="menu-button" to="/cadastro-unidade">
            Cadastrar Unidade
          </Link>
        </div>

        <p className="menu-note">Você pode voltar ao login a qualquer momento.</p>
        <Link className="menu-back" to="/">
          Voltar ao Login
        </Link>
      </div>
    </div>
  );
}