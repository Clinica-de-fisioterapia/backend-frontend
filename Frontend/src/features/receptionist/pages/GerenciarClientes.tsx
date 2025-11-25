import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';
import { authService } from '../../../services/authService';
import { Header } from '../../../components/common/Header';
import { Sidebar } from '../../../components/common/Sidebar';
import { Customer } from '../../../types';
import { Plus, Edit, Trash2 } from 'lucide-react';

// Interface para o tipo estendido de Customer com campos adicionais
interface CustomerWithExtras extends Customer {
  name?: string;
}

export default function GerenciarClientes() {
  const navigate = useNavigate();
  const { user, clearAuth } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [customers, setCustomers] = useState<CustomerWithExtras[]>([]);

  useEffect(() => {
    const fetchCustomers = async () => {
      const mockCustomers: CustomerWithExtras[] = [
        { 
          id: 'c1', 
          full_name: 'Alice Souza', 
          email: 'alice@email.com', 
          phone: '(11) 98765-4321', 
          cpf: '111.111.111-11', 
          person_id: 'perA', 
          created_at: new Date().toISOString(),
          name: 'Alice Souza',
        },
        { 
          id: 'c2', 
          full_name: 'Roberto Silva', 
          email: 'roberto@email.com', 
          phone: '(21) 99887-7665', 
          cpf: '222.222.222-22', 
          person_id: 'perB', 
          created_at: new Date().toISOString(),
          name: 'Roberto Silva',
        },
      ];
      setCustomers(mockCustomers);
    };
    fetchCustomers();
  }, []);

  const handleLogout = () => {
    authService.logout();
    clearAuth();
    navigate('/login');
  };

  const handleNavigate = (view: string) => {
    // Determinar se é admin ou receptionist
    const isAdmin = user?.role === 'admin';
    
    const routes: Record<string, string> = isAdmin ? {
      // Rotas Admin
      'dashboard': '/admin/dashboard',
      'bookings': '/admin/bookings',
      'professionals': '/admin/professionals',
      'customers': '/admin/customers',
      'units': '/admin/units',
      'settings': '/admin/settings',
    } : {
      // Rotas Receptionist
      'bookings': '/receptionist/hub',
      'customers': '/receptionist/customers'
    };
    
    const route = routes[view];
    if (route) {
      navigate(route);
    }
  };

  const handleEdit = (id: string) => {
    alert(`Editar cliente: ${id}`);
  };

  const handleDelete = (id: string) => {
    if (window.confirm(`Tem certeza que deseja deletar o cliente ${id}?`)) {
      setCustomers((prev) => prev.filter((c) => c.id !== id));
      alert(`Cliente ${id} deletado (simulação).`);
    }
  };

  const handleNew = () => {
    alert('Abrir modal/página para novo cliente');
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100vh', background: '#f9fafb' }}>
      <Header 
        user={user} 
        onLogout={handleLogout} 
        onMenuToggle={() => setSidebarOpen(!sidebarOpen)} 
      />
      <div style={{ display: 'flex', flex: 1, overflow: 'hidden' }}>
        <Sidebar 
          isOpen={sidebarOpen} 
          role={user?.role || 'receptionist'}
          onNavigate={handleNavigate} 
        />
        <main style={{ flex: 1, overflowY: 'auto', padding: '24px' }}>
          <h2 style={{ margin: '0 0 24px 0', fontSize: '28px', fontWeight: '700', color: '#111' }}>
            👥 Gerenciar Clientes 
          </h2>

          <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: '16px' }}>
            <button
              onClick={handleNew}
              style={{
                background: '#2563eb',
                color: 'white',
                padding: '10px 16px',
                borderRadius: '8px',
                display: 'flex',
                alignItems: 'center',
                gap: '8px',
                fontSize: '14px',
                fontWeight: '600',
                border: 'none',
                cursor: 'pointer'
              }}
            >
              <Plus size={18} /> Novo Cliente
            </button>
          </div>

          <div style={{ background: 'white', borderRadius: '12px', border: '1px solid #e5e7eb', overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr style={{ background: '#f3f4f6', borderBottom: '1px solid #e5e7eb' }}>
                  <th style={{ padding: '12px 16px', textAlign: 'left', fontWeight: '600', color: '#111' }}>Nome</th>
                  <th style={{ padding: '12px 16px', textAlign: 'left', fontWeight: '600', color: '#111' }}>Email</th>
                  <th style={{ padding: '12px 16px', textAlign: 'left', fontWeight: '600', color: '#111' }}>Telefone</th>
                  <th style={{ padding: '12px 16px', textAlign: 'center', fontWeight: '600', color: '#111' }}>Ações</th>
                </tr>
              </thead>
              <tbody>
                {customers.map((customer) => (
                  <tr key={customer.id} style={{ borderBottom: '1px solid #e5e7eb' }}>
                    <td style={{ padding: '12px 16px', color: '#111' }}>{customer.full_name}</td>
                    <td style={{ padding: '12px 16px', color: '#111' }}>{customer.email || '-'}</td>
                    <td style={{ padding: '12px 16px', color: '#111' }}>{customer.phone || '-'}</td>
                    <td style={{ padding: '12px 16px', textAlign: 'center' }}>
                      <div style={{ display: 'flex', gap: '8px', justifyContent: 'center' }}>
                        <button
                          onClick={() => handleEdit(customer.id)}
                          style={{ 
                            background: '#3b82f6', 
                            color: 'white', 
                            padding: '6px', 
                            borderRadius: '4px',
                            display: 'flex',
                            alignItems: 'center',
                            border: 'none',
                            cursor: 'pointer'
                          }}
                        >
                          <Edit size={16} />
                        </button>
                        <button
                          onClick={() => handleDelete(customer.id)}
                          style={{ 
                            background: '#ef4444', 
                            color: 'white', 
                            padding: '6px', 
                            borderRadius: '4px',
                            display: 'flex',
                            alignItems: 'center',
                            border: 'none',
                            cursor: 'pointer'
                          }}
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
                {customers.length === 0 && (
                  <tr>
                    <td colSpan={4} style={{ textAlign: 'center', padding: '20px', color: '#6b7280' }}>
                      Nenhum cliente cadastrado.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </main>
      </div>
    </div>
  );
}