import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../../../services/authService';
import { Logo } from '../../../components/common/Logo';
import { Alert } from '../../../components/common/Alert';
import { LoadingSpinner } from '../../../components/common/LoadingSpinner';

export default function Register() {
  const [form, setForm] = useState({
    companyName: '',
    subdomain: '',
    adminFullName: '',
    adminEmail: '',
    adminPassword: '',
    confirmPassword: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (form.adminPassword !== form.confirmPassword) {
      setError('As senhas não coincidem.');
      return;
    }

    setLoading(true);
    try {
      await authService.signup({
        companyName: form.companyName,
        subdomain: form.subdomain,
        adminFullName: form.adminFullName,
        adminEmail: form.adminEmail,
        adminPassword: form.adminPassword,
      });
      setSuccess('Conta criada com sucesso! Você será redirecionado para o login.');
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (err: any) {
      setError(err.message || 'Falha ao criar conta. Tente novamente.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '100vh',
        background: '#f3f4f6',
        padding: '20px',
      }}
    >
      <div
        style={{
          width: '100%',
          maxWidth: '500px',
          background: 'white',
          padding: '32px',
          borderRadius: '12px',
          boxShadow: '0 10px 25px rgba(0,0,0,0.1)',
        }}
      >
        <div style={{ textAlign: 'center', marginBottom: '32px' }}>
          <Logo size="large" />
          <h1
            style={{
              margin: '16px 0 0 0',
              fontSize: '24px',
              fontWeight: '700',
              color: '#111',
            }}
          >
            Crie sua conta ChronoSys
          </h1>
        </div>

        <p style={{ 
          textAlign: 'center', 
          marginBottom: '24px' 
        }} >
          Comece a usar o ChronoSys agora
        </p>

        {error && (
          <Alert type="error" message={error} onClose={() => setError('')} style={{ marginBottom: '16px' }} />
        )}
        {success && (
          <Alert type="success" message={success} onClose={() => setSuccess('')} style={{ marginBottom: '16px' }} />
        )}

        <form onSubmit={handleSubmit} style={{ display: 'grid', gap: '16px' }}>
          <div>
            <label
              style={{
                display: 'block',
                marginBottom: '6px',
                fontSize: '14px',
                fontWeight: '500',
                color: '#111',
              }}
            >
              Nome da Empresa
            </label>
            <input
              type="text"
              name="companyName"
              value={form.companyName}
              onChange={handleChange}
              required
              disabled={loading}
              style={{
                width: '100%',
                padding: '10px 12px',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                fontSize: '14px',
                boxSizing: 'border-box',
                outline: 'none',
                background: loading ? '#f9fafb' : 'white'
              }}
            />
          </div>
          <div>
            <label
              style={{
                display: 'block',
                marginBottom: '6px',
                fontSize: '14px',
                fontWeight: '500',
                color: '#111',
              }}
            >
              Subdomínio
            </label>
            <input
              type="text"
              name="subdomain"
              value={form.subdomain}
              onChange={handleChange}
              required
              disabled={loading}
              placeholder="minha-empresa"
              style={{
                width: '100%',
                padding: '10px 12px',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                fontSize: '14px',
                boxSizing: 'border-box',
                outline: 'none',
                background: loading ? '#f9fafb' : 'white'
              }}
            />
            <p style={{ fontSize: '12px', color: '#6b7280', marginTop: '4px' }}>
              Apenas letras, números e hífens
            </p>
          </div>
          <div>
            <label
              style={{
                display: 'block',
                marginBottom: '6px',
                fontSize: '14px',
                fontWeight: '500',
                color: '#111',
              }}
            >
              Nome Completo do Administrador
            </label>
            <input
              type="text"
              name="adminFullName"
              value={form.adminFullName}
              onChange={handleChange}
              required
              disabled={loading}
              style={{
                width: '100%',
                padding: '10px 12px',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                fontSize: '14px',
                boxSizing: 'border-box',
                outline: 'none',
                background: loading ? '#f9fafb' : 'white'
              }}
            />
          </div>
          <div>
            <label
              style={{
                display: 'block',
                marginBottom: '6px',
                fontSize: '14px',
                fontWeight: '500',
                color: '#111',
              }}
            >
              Email do Administrador
            </label>
            <input
              type="email"
              name="adminEmail"
              value={form.adminEmail}
              onChange={handleChange}
              required
              disabled={loading}
              style={{
                width: '100%',
                padding: '10px 12px',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                fontSize: '14px',
                boxSizing: 'border-box',
                outline: 'none',
                background: loading ? '#f9fafb' : 'white'
              }}
            />
          </div>
          <div>
            <label
              style={{
                display: 'block',
                marginBottom: '6px',
                fontSize: '14px',
                fontWeight: '500',
                color: '#111',
              }}
            >
              Senha
            </label>
            <input
              type="password"
              name="adminPassword"
              value={form.adminPassword}
              onChange={handleChange}
              required
              disabled={loading}
              style={{
                width: '100%',
                padding: '10px 12px',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                fontSize: '14px',
                boxSizing: 'border-box',
                outline: 'none',
                background: loading ? '#f9fafb' : 'white'
              }}
            />
          </div>
          <div>
            <label
              style={{
                display: 'block',
                marginBottom: '6px',
                fontSize: '14px',
                fontWeight: '500',
                color: '#111',
              }}
            >
              Confirme a Senha
            </label>
            <input
              type="password"
              name="confirmPassword"
              value={form.confirmPassword}
              onChange={handleChange}
              required
              disabled={loading}
              style={{
                width: '100%',
                padding: '10px 12px',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                fontSize: '14px',
                boxSizing: 'border-box',
                outline: 'none',
                background: loading ? '#f9fafb' : 'white'
              }}
            />
          </div>

          <button
            type="submit"
            disabled={loading || !!success}
            style={{
              background: loading || !!success ? '#93c5fd' : '#2563eb',
              color: 'white',
              padding: '10px 12px',
              borderRadius: '8px',
              fontSize: '16px',
              fontWeight: '600',
              cursor: loading || !!success ? 'not-allowed' : 'pointer',
              transition: 'background-color 0.2s',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: '8px',
              marginTop: '10px'
            }}
          >
            {loading ? (
              <>
                <LoadingSpinner size="small" /> Criando Conta...
              </>
            ) : (
              'Criar Conta'
            )}
          </button>
        </form>

        <div
          style={{
            marginTop: '20px',
            textAlign: 'center',
            fontSize: '13px',
            color: '#9ca3af',
          }}
        >
          Já tem conta?{' '}
          <button
            type="button"
            onClick={() => navigate('/login')}
            style={{
              background: 'none',
              border: 'none',
              color: '#2563eb',
              cursor: 'pointer',
              fontWeight: '600',
            }}
          >
            Fazer Login
          </button>
        </div>
      </div>
    </div>
  );
}