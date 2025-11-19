import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { authService } from "../../../services/authService";

export default function Login() {
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [tenant, setTenant] = useState("default");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const nav = useNavigate();

  const handle = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await authService.login(email.trim(), senha, tenant || "default");
      nav("/hub");
    } catch (err: any) {
      setError(err?.message ?? "Erro ao fazer login");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="card auth-card">
        <div className="auth-title">Entrar na ChronoSys</div>
        <div className="kv" style={{ marginBottom: 12 }}>Coloque suas credenciais</div>

        {error && <div style={{ color: "var(--danger)", marginBottom: 8 }}>{error}</div>}
        <form onSubmit={handle} style={{ display: "grid", gap: 10 }}>
          <input className="input" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="E-mail" required />
          <input className="input" value={senha} onChange={(e) => setSenha(e.target.value)} placeholder="Senha" type="password" required />
          <input className="input" value={tenant} onChange={(e) => setTenant(e.target.value)} placeholder="Tenant (opcional)" />
          <button disabled={loading}>{loading ? "Entrando..." : "Entrar"}</button>
        </form>

        <div style={{ marginTop: 12, display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <small className="kv">Ajuda: esqueceu a senha? fale com o admin</small>
          <a href="/register" style={{ color: "var(--primary)", textDecoration: "none" }}>Criar conta</a>
        </div>
      </div>
    </div>
  );
}