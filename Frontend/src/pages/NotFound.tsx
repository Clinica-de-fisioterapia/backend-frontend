import React from 'react';
import { Link } from 'react-router-dom';

export default function NotFound() {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', background: '#f9fafb', textAlign: 'center' }}>
      <h1 style={{ fontSize: '72px', color: '#ef4444', margin: '0 0 16px 0' }}>404</h1>
      <h2 style={{ fontSize: '24px', color: '#111', margin: '0 0 8px 0' }}>Página Não Encontrada</h2>
      <p style={{ color: '#6b7280', marginBottom: '24px' }}>
        A página que você está procurando não existe ou foi movida.
      </p>
      <Link to="/" style={{ 
        color: '#2563eb', 
        textDecoration: 'none', 
        fontWeight: '600',
        padding: '10px 16px',
        border: '1px solid #2563eb',
        borderRadius: '8px',
        transition: 'all 0.2s'
      }}
      onMouseEnter={(e) => {
        e.currentTarget.style.background = '#eff6ff';
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.background = 'none';
      }}
      >
        Voltar para a Home
      </Link>
    </div>
  );
}