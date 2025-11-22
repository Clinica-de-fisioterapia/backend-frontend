import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';
import { authService } from '../../../services/authService';
import { Header } from '../../../components/common/Header';
import { Sidebar } from '../../../components/common/Sidebar';
import { Customer } from '../../../types';
import { Plus, Edit, Trash2 } from 'lucide-react';
import { customerApi } from '../../../services/api/customerApi';

export default function GerenciarClientes() {
  const navigate = useNavigate();
  const { user, clearAuth } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(true);

  // Mocking customers loading for demo
  const [customers, setCustomers] = useState<
    (Customer & { name: string; phone: string })[]
  >([]);

  useEffect(() => {
    // Simulação de carregamento de clientes
    const fetchCustomers = async () => {
      // Na aplicação real, usaria customerApi.getAll()
      const mockCustomers = [
        { 
          id: 'c1', 
          full_name: 'Alice Souza', 
          email: 'alice@email.com', 
          phone: '(11) 98765-4321', 
          cpf: '111.111.111-11', 
          person_id: 'perA', 
          created_at: new Date().toISOString(),
          name: 'Alice Souza', // Adicionando para simplificar a tabela de mock
        },
        { 
          id: 'c2', 
          full_name: 'Roberto Silva', 
          email: 'roberto@email.com', 
          phone: '(21) 99887-7665', 
          cpf: '222.222.222-22', 
          person_id: 'perB', 
          created_at: new Date().toISOString(),
          name: 'Roberto Silva', // Adicionando para simplificar a tabela de mock
        },
      ];
      setCustomers(mockCustomers as any);
    };
    fetchCustomers();
  }, []);

  const handleLogout = () => {
    authService.logout();
    clearAuth();
    navigate('/login');
  };

  const handleNavigate = (view: string) => {
    const routes: Record<string, string> = {
      'bookings': '/receptionist/hub',
      'customers': '/receptionist/customers'
    };
    navigate(routes[view] || '/receptionist/hub');
  };

  const handleEdit = (id: string) => {
    alert(`Editar cliente: ${id}`);
  };

  const handleDelete = (id: string) => {
    if (window.confirm(`Tem certeza que deseja deletar o cliente ${id}?`)) {
      // Na aplicação real: await customerApi.delete(id);
      setCustomers(prev => prev.filter(c => c.id !== id));
      alert(`Cliente ${id} deletado (simulação).`);
    }
  };

  const handleNew = () => {
    alert('Abrir modal/página para novo cliente');
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100vh', background: '#f9fafb' }} >
      <Header 
        user={user} 
        onLogout={handleLogout} 
        onMenuToggle={() => setSidebarOpen(!sidebarOpen)} 
      />
      <div style={{ display: 'flex', flex: 1, overflow: 'hidden' }}>
        <Sidebar 
          isOpen={sidebarOpen} 
          role={user?.role || 'receptionist'} // Usar role do usuário
          onNavigate={handleNavigate} 
        />
        <main style={{ flex: 1, overflowY: 'auto', padding: '24px' }}>
          <h2 style={{ margin: '0 0 24px 0', fontSize: '28px', fontWeight: '700', color: '#111' }} > 
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
                fontWeight: '600'
              }}
            >
              <Plus size={18} /> Novo Cliente
            </button>
          </div>

          <div style={{ background: 'white', borderRadius: '12px', border: '1px solid #e5e7eb', overflowX: 'auto' }} >
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr style={{ background: '#f3f4f6', borderBottom: '1px solid #e5e7eb' }}>
                  <th style={{ padding: '12px 16px', textAlign: 'left', fontWeight: '600', color: '#111' }}> Nome </th>
                  <th style={{ padding: '12px 16px', textAlign: 'left', fontWeight: '600', color: '#111' }}> Email </th>
                  <th style={{ padding: '12px 16px', textAlign: 'left', fontWeight: '600', color: '#111' }}> Telefone </th>
                  <th style={{ padding: '12px 16px', textAlign: 'center', fontWeight: '600', color: '#111' }}> Ações </th>
                </tr>
              </thead>
              <tbody>
                {customers.map((customer) => (
                  <tr key={customer.id} style={{ borderBottom: '1px solid #e5e7eb' }}>
                    <td style={{ padding: '12px 16px', color: '#111' }}>{customer.full_name}</td>
                    <td style={{ padding: '12px 16px', color: '#111' }}>{customer.email}</td>
                    <td style={{ padding: '12px 16px', color: '#111' }}>{customer.phone}</td>
                    <td style={{ padding: '12px 16px', textAlign: 'center' }} >
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
                          }}
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
                customers.length === 0 && (
                  <tr>
                    <td colSpan={4} style={{ textAlign: 'center', padding: '20px', color: '#6b7280' }}>
                      Nenhum cliente cadastrado.
                    </td>
                  </tr>
                )
              </tbody>
            </table>
          </div>
        </main>
      </div>
    </div>
  );
}