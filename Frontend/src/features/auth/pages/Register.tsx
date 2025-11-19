import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { authService } from "../../../services/authService";

export default function Register() {
  const [form, setForm] = useState({ companyName: "", subdomain: "", nome: "", email: "", senha: "" });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const nav = useNavigate();

  const onChange = (e: React.ChangeEvent<HTMLInputElement>) => setForm({ ...form, [e.target.name]: e.target.value });

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await authService.signup({
        CompanyName: form.companyName,
        Subdomain: form.subdomain,
        AdminFullName: form.nome,
        AdminEmail: form.email,
        AdminPassword: form.senha,
        tenant: form.subdomain || "default",
      });
      nav("/login");
    } catch (err: any) {
      setError(err?.message ?? "Erro ao criar conta");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="card auth-card">
        <div className="auth-title">Criar conta</div>
        {error && <div style={{ color: "var(--danger)" }}>{error}</div>}

        <form onSubmit={submit} style={{ display: "grid", gap: 10 }}>
          <input name="companyName" className="input" value={form.companyName} onChange={onChange} placeholder="Nome da empresa" required />
          <input name="subdomain" className="input" value={form.subdomain} onChange={onChange} placeholder="Subdomínio" required />
          <input name="nome" className="input" value={form.nome} onChange={onChange} placeholder="Nome do admin" required />
          <input name="email" className="input" value={form.email} onChange={onChange} placeholder="E-mail" required />
          <input name="senha" className="input" value={form.senha} onChange={onChange} placeholder="Senha" type="password" required />
          <div style={{ display: "flex", gap: 8 }}>
            <button disabled={loading} type="submit">{loading ? "Criando..." : "Criar Conta"}</button>
            <button type="button" className="secondary" onClick={() => nav("/login")}>Voltar</button>
          </div>
        </form>
      </div>
    </div>
  );
}