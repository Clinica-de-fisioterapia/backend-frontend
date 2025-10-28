// Frontend/pages/Login/Login.tsx
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { authService } from "../../services/authService";
import "./Login.css";

export default function Login() {
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [tenant, setTenant] = useState("default"); // Tenant padrão
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    if (!email.trim() || !senha) {
      setError("Preencha e-mail e senha.");
      setLoading(false);
      return;
    }

    try {
      // authService.login envia X-Tenant e salva tokens em localStorage
      await authService.login(email.trim(), senha, tenant);
      navigate("/menu");
    } catch (err: any) {
      const msg =
        err?.message ||
        err?.response?.data?.message ||
        "Erro ao fazer login. Verifique suas credenciais e se o backend está ativo.";
      setError(msg);
      console.error("Erro no login:", err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h2>Bem-vindo</h2>
        <p>Faça login para agendar seu horário</p>

        {error && (
          <div
            className="error-message"
            style={{
              backgroundColor: "#fee",
              border: "1px solid #fcc",
              color: "#c33",
              padding: "10px",
              borderRadius: "4px",
              marginBottom: "15px",
            }}
          >
            {error}
          </div>
        )}

        <form onSubmit={handleLogin}>
          <input
            type="email"
            placeholder="E-mail"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            disabled={loading}
          />
          <input
            type="password"
            placeholder="Senha"
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            required
            disabled={loading}
          />

          {/* Tenant opcional — mantém compatibilidade com backend multitenant */}
          <input
            type="text"
            placeholder="Tenant (opcional)"
            value={tenant}
            onChange={(e) => setTenant(e.target.value || "default")}
            disabled={loading}
            style={{ marginTop: 8, opacity: 0.9 }}
          />

          <button type="submit" disabled={loading} style={{ marginTop: 12 }}>
            {loading ? "Entrando..." : "Entrar"}
          </button>
        </form>

        <div
          className="register-link"
          style={{ textAlign: "center", marginTop: "20px", color: "#666", fontSize: "14px" }}
        >
          Não tem uma conta?{" "}
          <a
            href="/register"
            onClick={(e) => {
              e.preventDefault();
              navigate("/register");
            }}
            style={{ color: "#667eea", textDecoration: "none", fontWeight: "600" }}
          >
            Cadastre-se
          </a>
        </div>
      </div>
    </div>
  );
}