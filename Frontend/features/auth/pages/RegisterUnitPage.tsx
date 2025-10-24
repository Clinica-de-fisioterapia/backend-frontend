import React, { useState } from "react";
import { AuthLayout } from "../../../shared/layouts/AuthLayout";
import "../styles/RegisterUnitPage.css";

export default function RegisterUnitPage() {
  const [form, setForm] = useState({
    nome: "",
    endereco: "",
    cidade: "",
    estado: "",
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!form.nome || !form.endereco || !form.cidade || !form.estado) {
      alert("Preencha todos os campos!");
      return;
    }

    console.log("Unidade cadastrada:", form);
    alert("Unidade cadastrada com sucesso!");
    setForm({ nome: "", endereco: "", cidade: "", estado: "" });
  };

  return (
    <AuthLayout>
      <div className="register-unit-container">
        <div className="register-unit-header">
          <div className="header-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
              <polyline points="9 22 9 12 15 12 15 22"></polyline>
            </svg>
          </div>
          <h2>Cadastro de Unidade</h2>
          <p className="header-subtitle">Registre uma nova unidade de atendimento</p>
        </div>

        <form className="register-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="nome">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
              </svg>
              Nome da Unidade
            </label>
            <input
              id="nome"
              type="text"
              name="nome"
              placeholder="Ex: Clínica São Paulo Centro"
              value={form.nome}
              onChange={handleChange}
            />
          </div>

          <div className="form-group">
            <label htmlFor="endereco">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path>
                <circle cx="12" cy="10" r="3"></circle>
              </svg>
              Endereço Completo
            </label>
            <input
              id="endereco"
              type="text"
              name="endereco"
              placeholder="Ex: Rua das Flores, 123"
              value={form.endereco}
              onChange={handleChange}
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="cidade">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
                  <polyline points="9 22 9 12 15 12 15 22"></polyline>
                </svg>
                Cidade
              </label>
              <input
                id="cidade"
                type="text"
                name="cidade"
                placeholder="Ex: São Paulo"
                value={form.cidade}
                onChange={handleChange}
              />
            </div>

            <div className="form-group">
              <label htmlFor="estado">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <circle cx="12" cy="12" r="10"></circle>
                  <line x1="2" y1="12" x2="22" y2="12"></line>
                  <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path>
                </svg>
                Estado
              </label>
              <input
                id="estado"
                type="text"
                name="estado"
                placeholder="Ex: SP"
                value={form.estado}
                onChange={handleChange}
                maxLength={2}
              />
            </div>
          </div>

          <button type="submit" className="btn-submit">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <polyline points="20 6 9 17 4 12"></polyline>
            </svg>
            Cadastrar Unidade
          </button>
        </form>

        <div className="info-box">
          <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="16" x2="12" y2="12"></line>
            <line x1="12" y1="8" x2="12.01" y2="8"></line>
          </svg>
          <p>Preencha todos os campos com atenção. As informações cadastradas serão utilizadas para identificação da unidade no sistema.</p>
        </div>
      </div>
    </AuthLayout>
  );
}