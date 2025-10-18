import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { AuthLayout } from "../../../shared/layouts/AuthLayout";
import "../styles/LoginPage.css";

export default function LoginPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();

    if (!email || !senha) {
      alert("Preencha todos os campos!");
      return;
    }

    // Aqui futuramente chamará apiClient.post("/auth/login", {...})
    console.log("Login realizado:", { email, senha });
    navigate("/cadastro-unidade");
  };

  return (
    <AuthLayout>
      <h2>Login</h2>
      <form className="login-form" onSubmit={handleLogin}>
        <input
          type="email"
          placeholder="E-mail"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
        <input
          type="password"
          placeholder="Senha"
          value={senha}
          onChange={(e) => setSenha(e.target.value)}
        />
        <button type="submit">Entrar</button>
      </form>

      {/* novo link que aponta para a rota /login (mesma porta) */}
      <p>
        <Link to="/login">Ir para outra versão do Login</Link>
      </p>
    </AuthLayout>
  );
}
