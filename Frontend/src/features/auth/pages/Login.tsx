import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../../store/authStore';
import { authService } from '../../../services/authService';
import { ROLE_ROUTES } from '../../../constants/roles';
import { Logo } from '../../../components/common/Logo';
import { Alert } from '../../../components/common/Alert';
import { LoadingSpinner } from '../../../components/common/LoadingSpinner';

const demoAccounts = [
  { email: 'admin@clinic.com', role: 'admin' },
  { email: 'maria@clinic.com', role: 'receptionist' },
  { email: 'joao@clinic.com', role: 'professional' },
];

export default function Login() {
  const [email, setEmail] = useState('maria@clinic.com');
  const [password, setPassword] = useState('senha123');
  const [tenant, setTenant] = useState('empresateste');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  const navigate = useNavigate();
  const setAuth = useAuthStore((state) => state.setAuth);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const authUser = useAuthStore((state) => state.user);

  useEffect(() => {
    if (isAuthenticated && authUser) {
      const route = ROLE_ROUTES[authUser.role];
      navigate(route, { replace: true });
    }
  }, [isAuthenticated, authUser, navigate]);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const response = await authService.login(email, password, tenant);
      setAuth(response);
      localStorage.setItem('tenant', tenant);
    } catch (err: any) {
      setError(err.message || 'Falha ao conectar. Verifique as credenciais.');
    } finally {
      setLoading(false);
    }
  };

  const handleDemoLogin = (acc: typeof demoAccounts[0]) => {
    setEmail(acc.email);
    setPassword('senha123');
    setTenant('empresateste');
    // A submissão será feita manualmente ou o usuário clica em Entrar
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
          maxWidth: '400px',
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
            Acesse sua conta
          </h1>
        </div>

        {error && (
          <Alert
            type="error"
            message={error}
            onClose={() => setError('')}
            style={{ marginBottom: '16px' }}
          />
        )}

        <form onSubmit={handleLogin} style={{ display: 'grid', gap: '16px' }}>
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
              Subdomínio / Tenant
            </label>
            <input
              type="text"
              value={tenant}
              onChange={(e) => setTenant(e.target.value)}
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
                transition: 'all 0.2s',
                background: loading ? '#f9fafb' : 'white'
              }}
              onFocus={(e) => (e.target.style.borderColor = '#2563eb')}
              onBlur={(e) => (e.target.style.borderColor = '#e5e7eb')}
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
              Email
            </label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
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
                transition: 'all 0.2s',
                background: loading ? '#f9fafb' : 'white'
              }}
              onFocus={(e) => (e.target.style.borderColor = '#2563eb')}
              onBlur={(e) => (e.target.style.borderColor = '#e5e7eb')}
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
            <div style={{ position: 'relative' }}>
              <input
                type={showPassword ? 'text' : 'password'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
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
                  transition: 'all 0.2s',
                  background: loading ? '#f9fafb' : 'white'
                }}
                onFocus={(e) => (e.target.style.borderColor = '#2563eb')}
                onBlur={(e) => (e.target.style.borderColor = '#e5e7eb')}
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                style={{
                  position: 'absolute',
                  right: '12px',
                  top: '50%',
                  transform: 'translateY(-50%)',
                  background: 'none',
                  border: 'none',
                  cursor: 'pointer',
                  color: '#6b7280',
                  fontSize: '14px'
                }}
              >
                {showPassword ? 'Ocultar' : 'Mostrar'}
              </button>
            </div>
          </div>

          <button
            type="submit"
            disabled={loading}
            style={{
              background: loading ? '#93c5fd' : '#2563eb',
              color: 'white',
              padding: '10px 12px',
              borderRadius: '8px',
              fontSize: '16px',
              fontWeight: '600',
              cursor: loading ? 'not-allowed' : 'pointer',
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
                <LoadingSpinner size="small" /> Entrando...
              </>
            ) : (
              'Entrar'
            )}
          </button>
        </form>
        <div style={{ marginTop: '20px', borderTop: '1px solid #e5e7eb', paddingTop: '20px' }}>
          <p style={{
            margin: '0 0 10px 0',
            fontSize: '14px',
            fontWeight: '600',
            color: '#111'
          }}>
            Acessos Rápidos (Demo):
          </p>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
            {demoAccounts.map((account) => (
              <button
                key={account.email}
                type="button"
                onClick={() => handleDemoLogin(account)}
                style={{
                  textAlign: 'left',
                  padding: '8px 12px',
                  borderRadius: '6px',
                  border: '1px solid #e5e7eb',
                  background: 'none',
                  transition: 'background 0.2s'
                }}
                onMouseEnter={(e) => e.currentTarget.style.background = '#f3f4f6'}
                onMouseLeave={(e) => e.currentTarget.style.background = 'none'}
              >
                <div style={{ fontSize: '14px', fontWeight: '500', color: '#111' }}>
                  {account.email}
                </div>
                <div style={{ fontSize: '11px', color: '#6b7280' }}>
                  {account.role}
                </div>
              </button>
            ))}
          </div>
        </div>
        <div
          style={{
            marginTop: '20px',
            textAlign: 'center',
            fontSize: '13px',
            color: '#9ca3af',
          }}
        >
          Não tem conta?{' '}
          <button
            type="button"
            onClick={() => navigate('/register')}
            style={{
              background: 'none',
              border: 'none',
              color: '#2563eb',
              cursor: 'pointer',
              fontWeight: '600',
              textDecoration: 'none',
              transition: 'all 0.2s',
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.textDecoration = 'underline';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.textDecoration = 'none';
            }}
          >
            Criar Conta
          </button>
        </div>
      </div>
    </div>
  );
}