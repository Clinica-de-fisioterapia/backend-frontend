// Frontend/pages/Register/Register.tsx
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { authService } from "../../services/authService";
import "./Register.css";

interface RegisterFormData {
  nome: string;
  email: string;
  senha: string;
  confirmarSenha: string;
  telefone: string;
  companyName: string;
  subdomain: string;
  tenant?: string;
}

export default function Register() {
  const [formData, setFormData] = useState<RegisterFormData>({
    nome: "",
    email: "",
    senha: "",
    confirmarSenha: "",
    telefone: "",
    companyName: "",
    subdomain: "",
    tenant: "default" // Tenant padrão, usuário não precisa preencher
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const validateForm = (): boolean => {
    if (!formData.nome || !formData.email || !formData.senha || !formData.telefone || !formData.companyName || !formData.subdomain) {
      setError("Preencha todos os campos obrigatórios");
      return false;
    }

    if (formData.senha.length < 6) {
      setError("A senha deve ter no mínimo 6 caracteres");
      return false;
    }

    if (formData.senha !== formData.confirmarSenha) {
      setError("As senhas não conferem");
      return false;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.email)) {
      setError("Email inválido");
      return false;
    }

    return true;
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!validateForm()) {
      return;
    }

    setLoading(true);

    try {
      // Chama o serviço de cadastro
      await authService.register({
        companyName: formData.companyName,
        subdomain: formData.subdomain,
        adminFullName: formData.nome,
        adminEmail: formData.email,
        adminPassword: formData.senha,
        tenant: formData.tenant
      });

      setSuccess(true);
      
      // Aguarda 2 segundos e redireciona para login
      setTimeout(() => {
        navigate("/login");
      }, 2000);

    } catch (err: any) {
      setError(err.message || "Erro ao criar conta. Tente novamente.");
      console.error("Erro no cadastro:", err);
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div className="register-container">
        <div className="register-card">
          <div className="success-message">
            <h2>✅ Cadastro realizado com sucesso!</h2>
            <p>Redirecionando para o login...</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="register-container">
      <div className="register-card">
        <h2>Criar Conta</h2>
        <p>Preencha seus dados para criar uma conta</p>

        {error && (
          <div className="error-message">
            {error}
          </div>
        )}

        <form onSubmit={handleRegister}>
          <input
            type="text"
            name="companyName"
            placeholder="Nome da empresa *"
            value={formData.companyName}
            onChange={handleChange}
            required
            disabled={loading}
          />

          <input
            type="text"
            name="subdomain"
            placeholder="Subdomínio (ex: minha-empresa) *"
            value={formData.subdomain}
            onChange={handleChange}
            required
            disabled={loading}
          />

          <input
            type="text"
            name="nome"
            placeholder="Nome completo *"
            value={formData.nome}
            onChange={handleChange}
            required
            disabled={loading}
          />

          <input
            type="email"
            name="email"
            placeholder="E-mail *"
            value={formData.email}
            onChange={handleChange}
            required
            disabled={loading}
          />

          <input
            type="tel"
            name="telefone"
            placeholder="Telefone (WhatsApp) *"
            value={formData.telefone}
            onChange={handleChange}
            required
            disabled={loading}
          />

          <input
            type="password"
            name="senha"
            placeholder="Senha (mínimo 6 caracteres) *"
            value={formData.senha}
            onChange={handleChange}
            required
            disabled={loading}
            minLength={6}
          />

          <input
            type="password"
            name="confirmarSenha"
            placeholder="Confirmar senha *"
            value={formData.confirmarSenha}
            onChange={handleChange}
            required
            disabled={loading}
          />

          <button type="submit" disabled={loading}>
            {loading ? "Criando conta..." : "Criar Conta"}
          </button>
        </form>

        <div className="login-link">
          Já tem uma conta?{" "}
          <a href="/login" onClick={(e) => {
            e.preventDefault();
            navigate("/login");
          }}>
            Faça login
          </a>
        </div>
      </div>
    </div>
  );
}