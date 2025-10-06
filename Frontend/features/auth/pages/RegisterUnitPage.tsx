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

    // Chamada futura: apiClient.post("/unidades", form)
    console.log("Unidade cadastrada:", form);
    alert("Unidade cadastrada com sucesso!");
    setForm({ nome: "", endereco: "", cidade: "", estado: "" });
  };

  return (
    <AuthLayout>
      <h2>Cadastro de Unidade</h2>
      <form className="register-form" onSubmit={handleSubmit}>
        <input
          type="text"
          name="nome"
          placeholder="Nome da Unidade"
          value={form.nome}
          onChange={handleChange}
        />
        <input
          type="text"
          name="endereco"
          placeholder="Endereço"
          value={form.endereco}
          onChange={handleChange}
        />
        <input
          type="text"
          name="cidade"
          placeholder="Cidade"
          value={form.cidade}
          onChange={handleChange}
        />
        <input
          type="text"
          name="estado"
          placeholder="Estado"
          value={form.estado}
          onChange={handleChange}
        />
        <button type="submit">Cadastrar</button>
      </form>
    </AuthLayout>
  );
}
