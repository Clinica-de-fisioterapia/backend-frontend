import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';
import { authService } from '../../../services/authService';
import { Header } from '../../../components/common/Header';
import { Sidebar } from '../../../components/common/Sidebar';
import { Customer, Person } from '../../../types';
import { Plus, Edit, Trash2, Search, X } from 'lucide-react';
import { customerApi } from '../../../services/api/customerApi';
import { peopleApi } from '../../../services/api/peopleApi';
import { Alert } from '../../../components/common/Alert';
import { LoadingSpinner } from '../../../components/common/LoadingSpinner';

// Interface para o tipo estendido de Customer com campos de Pessoa para exibição
interface CustomerWithExtras extends Customer {
  full_name: string;
  email: string;
  phone: string;
}

// Tipo de dados de Pessoa para o ESTADO do formulário
type FormState = {
  fullName: string;
  email: string;
  phone: string;
  cpf: string;
}

export default function GerenciarClientes() {
  const navigate = useNavigate();
  const { user, clearAuth } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [customers, setCustomers] = useState<CustomerWithExtras[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [searchTerm, setSearchTerm] = useState('');

  // Estado para o modal de criação (único)
  const [showCustomerModal, setShowCustomerModal] = useState(false);

  // Estado do formulário de Pessoa
  const [formDataCustomer, setFormDataCustomer] = useState<FormState>({
    fullName: '',
    email: '',
    phone: '',
    cpf: '',
  });

  useEffect(() => {
    fetchCustomers();
  }, []);

  const fetchCustomers = async () => {
    try {
      setLoading(true);

      // TIPAGEM AJUSTADA PARA REFLETIR O RETORNO DA API
      const [customerData, peopleData] = await Promise.all([
        customerApi.getAll() as Promise<Array<Customer & { personId: string }>>,
        peopleApi.getAll() as Promise<Person[]>,
      ]);

      console.log('✅ Retorno Customer API:', customerData);
      console.log('✅ Retorno People API:', peopleData);

      const peopleMap = new Map<string, Person>(peopleData.map(person => [person.id, person]));

      // ATENÇÃO: Mudança de cust.person_id para cust.personId
      const mergedCustomers = customerData
        .filter(cust => cust.personId && peopleMap.has(cust.personId)) // USA cust.personId
        .map(cust => {

          const person = peopleMap.get(cust.personId)!; // USA cust.personId

          return {
            ...cust,
            full_name: person.fullName || 'Nome não definido',
            email: person.email || '-',
            phone: person.phone || '-',
          } as CustomerWithExtras;
        });

      setCustomers(mergedCustomers);
      setError(''); // Limpa o erro se o merge for bem-sucedido

      // Limpamos a mensagem de erro específica de mesclagem, pois o problema era a chave
      if (customerData.length > 0 && mergedCustomers.length === 0) {
        setError('⚠️ Clientes encontrados, mas dados da Pessoa (People) não puderam ser mesclados. Verifique se o ID de Pessoa existe.');
      }

    } catch (err: any) {
      console.error('🔴 ERRO FATAL AO CARREGAR DADOS:', err);
      const message = err.message || (err.response?.data?.message ? `Erro de API: ${err.response.data.message}` : 'Erro desconhecido ao carregar clientes. O servidor pode estar inacessível.');
      setError(message);
      setCustomers([]);
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    authService.logout();
    clearAuth();
    navigate('/login');
  };

  const handleNavigate = (view: string) => {
    const isAdmin = user?.role === 'admin';

    const routes: Record<string, string> = isAdmin ? {
      'dashboard': '/admin/dashboard',
      'bookings': '/admin/calendar',
      'professionals': '/admin/professionals',
      'customers': '/admin/customers',
      'units': '/admin/units',
      'settings': '/admin/settings',
    } : {
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

  const handleDelete = async (id: string, name: string) => {
    if (window.confirm(`Tem certeza que deseja deletar o cliente ${name}?`)) {
      try {
        await customerApi.delete(id);
        setSuccess('Cliente deletado com sucesso!');
        fetchCustomers();
      } catch (err: any) {
        setError(err.message || 'Erro ao deletar cliente');
      }
    }
  };

  // Funções do Modal Único
  const handleOpenCustomerModal = () => {
    setFormDataCustomer({
      fullName: '',
      email: '',
      phone: '',
      cpf: '',
    });
    setShowCustomerModal(true);
    setError('');
    setSuccess('');
  };

  const handleCloseCustomerModal = () => {
    setShowCustomerModal(false);
    setError('');
    setSuccess('');
  };

  const handleNew = () => {
    handleOpenCustomerModal();
  };

  // Submissão ÚNICA do Formulário
  const handleSubmitCustomer = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    try {
      // 1. Preparar e criar a Pessoa (People)
      const personDataToSend: Omit<Person, 'id' | 'created_at'> = {
        fullName: formDataCustomer.fullName,
        email: formDataCustomer.email,
        // Remove a máscara dos campos (valores já são strings garantidamente)
        phone: formDataCustomer.phone.replace(/\D/g, ''),
        cpf: formDataCustomer.cpf.replace(/\D/g, ''),
      } as Person;

      const personResponse = await peopleApi.create(personDataToSend);
      const newPersonId = personResponse.id;

      // 2. Criar o Cliente (Customer) usando o ID da Pessoa
      await customerApi.create({
        person_id: newPersonId!,
      } as Omit<Customer, 'id' | 'created_at'>);

      setSuccess('Cliente cadastrado com sucesso!');
      handleCloseCustomerModal();
      fetchCustomers(); // Atualiza a lista
    } catch (err: any) {
      // Tratamento de erro
      const errorMessage = err.response?.data?.message || err.message || 'Erro ao cadastrar cliente. Verifique os dados.';
      setError(errorMessage);
    }
  };

  // Filtro de Clientes
  const filteredCustomers = customers.filter((customer) =>
    (customer.full_name && customer.full_name.toLowerCase().includes(searchTerm.toLowerCase())) ||
    (customer.email && customer.email.toLowerCase().includes(searchTerm.toLowerCase())) ||
    (customer.phone && customer.phone.includes(searchTerm))
  );

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
          <h2 style={{ margin: '0 0 8px 0', fontSize: '28px', fontWeight: '700', color: '#111' }}>
            👥 Gerenciar Clientes
          </h2>
          <p style={{ margin: 0, color: '#6b7280', fontSize: '14px', marginBottom: '24px' }}>
            Cadastre e gerencie os clientes
          </p>

          {error && (
            <Alert type="error" message={error} onClose={() => setError('')} style={{ marginBottom: '16px' }} />
          )}
          {success && (
            <Alert type="success" message={success} onClose={() => setSuccess('')} style={{ marginBottom: '16px' }} />
          )}

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px', gap: '16px' }}>
            <div style={{ position: 'relative', flex: 1, maxWidth: '400px' }}>
              <Search size={18} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: '#9ca3af' }} />
              <input
                type="text"
                placeholder="Buscar por nome, email ou telefone..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                style={{
                  width: '100%',
                  padding: '10px 12px 10px 40px',
                  border: '1px solid #e5e7eb',
                  borderRadius: '8px',
                  fontSize: '14px',
                  outline: 'none'
                }}
              />
            </div>

            <button
              onClick={handleNew}
              style={{
                background: 'linear-gradient(135deg, #2563eb, #1d4ed8)',
                color: 'white',
                padding: '10px 16px',
                borderRadius: '8px',
                display: 'flex',
                alignItems: 'center',
                gap: '8px',
                fontSize: '14px',
                fontWeight: '600',
                border: 'none',
                cursor: 'pointer',
                transition: 'all 0.2s',
                boxShadow: '0 4px 6px rgba(37, 99, 235, 0.2)'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = 'translateY(-2px)';
                e.currentTarget.style.boxShadow = '0 6px 12px rgba(37, 99, 235, 0.3)';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = 'translateY(0)';
                e.currentTarget.style.boxShadow = '0 4px 6px rgba(37, 99, 235, 0.2)';
              }}
            >
              <Plus size={18} /> Novo Cliente
            </button>
          </div>

          {loading ? (
            <div style={{ display: 'flex', justifyContent: 'center', padding: '40px' }}>
              <LoadingSpinner size="medium" />
            </div>
          ) : (
            <div style={{ background: 'white', borderRadius: '12px', border: '1px solid #e5e7eb', overflowX: 'auto', boxShadow: '0 1px 3px rgba(0,0,0,0.05)' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ background: 'linear-gradient(to right, #f9fafb, #f3f4f6)', borderBottom: '2px solid #e5e7eb' }}>
                    <th style={{ padding: '16px', textAlign: 'left', fontWeight: '600', color: '#111', fontSize: '14px' }}>Nome</th>
                    <th style={{ padding: '16px', textAlign: 'left', fontWeight: '600', color: '#111', fontSize: '14px' }}>Email</th>
                    <th style={{ padding: '16px', textAlign: 'left', fontWeight: '600', color: '#111', fontSize: '14px' }}>Telefone</th>
                    <th style={{ padding: '16px', textAlign: 'center', fontWeight: '600', color: '#111', fontSize: '14px' }}>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredCustomers.map((customer, index) => (
                    <tr key={customer.id} style={{
                      borderBottom: '1px solid #e5e7eb',
                      transition: 'background 0.2s',
                      background: index % 2 === 0 ? 'white' : '#fafbfc'
                    }}
                      onMouseEnter={(e) => { e.currentTarget.style.background = '#f0f9ff'; }}
                      onMouseLeave={(e) => { e.currentTarget.style.background = index % 2 === 0 ? 'white' : '#fafbfc'; }}>
                      <td style={{ padding: '16px', color: '#111', fontWeight: '500' }}>{customer.full_name || '-'}</td>
                      <td style={{ padding: '16px', color: '#6b7280', fontSize: '14px' }}>{customer.email || '-'}</td>
                      <td style={{ padding: '16px', color: '#6b7280', fontSize: '14px' }}>{customer.phone || '-'}</td>
                      <td style={{ padding: '16px', textAlign: 'center' }}>
                        <div style={{ display: 'flex', gap: '8px', justifyContent: 'center' }}>
                          <button
                            onClick={() => handleEdit(customer.id)}
                            title="Editar"
                            style={{
                              background: '#3b82f6',
                              color: 'white',
                              padding: '8px',
                              borderRadius: '6px',
                              display: 'flex',
                              alignItems: 'center',
                              border: 'none',
                              cursor: 'pointer',
                              transition: 'all 0.2s'
                            }}
                            onMouseEnter={(e) => {
                              e.currentTarget.style.background = '#2563eb';
                              e.currentTarget.style.transform = 'scale(1.1)';
                            }}
                            onMouseLeave={(e) => {
                              e.currentTarget.style.background = '#3b82f6';
                              e.currentTarget.style.transform = 'scale(1)';
                            }}
                          >
                            <Edit size={16} />
                          </button>
                          <button
                            onClick={() => handleDelete(customer.id, customer.full_name)}
                            title="Deletar"
                            style={{
                              background: '#ef4444',
                              color: 'white',
                              padding: '8px',
                              borderRadius: '6px',
                              display: 'flex',
                              alignItems: 'center',
                              border: 'none',
                              cursor: 'pointer',
                              transition: 'all 0.2s'
                            }}
                            onMouseEnter={(e) => {
                              e.currentTarget.style.background = '#dc2626';
                              e.currentTarget.style.transform = 'scale(1.1)';
                            }}
                            onMouseLeave={(e) => {
                              e.currentTarget.style.background = '#ef4444';
                              e.currentTarget.style.transform = 'scale(1)';
                            }}
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                  {filteredCustomers.length === 0 && (
                    <tr>
                      <td colSpan={4} style={{ textAlign: 'center', padding: '40px', color: '#6b7280' }}>
                        {searchTerm ? '🔍 Nenhum cliente encontrado.' : '📋 Nenhum cliente cadastrado.'}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          )}
        </main>
      </div>

      {/* MODAL ÚNICO DE CRIAÇÃO (Pessoa + Cliente) */}
      {showCustomerModal && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'rgba(0,0,0,0.5)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 1000,
          padding: '20px',
          backdropFilter: 'blur(4px)'
        }}>
          <div style={{
            background: 'white',
            borderRadius: '16px',
            width: '100%',
            maxWidth: '600px',
            maxHeight: '90vh',
            overflow: 'auto',
            boxShadow: '0 20px 25px -5px rgba(0,0,0,0.2), 0 10px 10px -5px rgba(0,0,0,0.1)',
            animation: 'slideInUp 0.3s ease-out'
          }}>
            <div style={{
              padding: '24px',
              borderBottom: '1px solid #e5e7eb',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              background: 'linear-gradient(135deg, #2563eb, #1d4ed8)',
              borderRadius: '16px 16px 0 0'
            }}>
              <h3 style={{ margin: 0, fontSize: '20px', fontWeight: '600', color: 'white' }}>
                ➕ Cadastrar Novo Cliente
              </h3>
              <button
                onClick={handleCloseCustomerModal}
                style={{
                  background: 'rgba(255,255,255,0.2)',
                  border: 'none',
                  borderRadius: '8px',
                  cursor: 'pointer',
                  color: 'white',
                  padding: '8px',
                  display: 'flex',
                  alignItems: 'center',
                  transition: 'background 0.2s'
                }}
                onMouseEnter={(e) => { e.currentTarget.style.background = 'rgba(255,255,255,0.3)'; }}
                onMouseLeave={(e) => { e.currentTarget.style.background = 'rgba(255,255,255,0.2)'; }}
              >
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleSubmitCustomer} style={{ padding: '24px', display: 'grid', gap: '20px' }}>
              <div>
                <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                  Nome Completo *
                </label>
                <input
                  type="text"
                  value={formDataCustomer.fullName}
                  onChange={(e) => setFormDataCustomer({ ...formDataCustomer, fullName: e.target.value })}
                  required
                  placeholder="Ex: Maria dos Santos"
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    border: '1px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    outline: 'none'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                  Email
                </label>
                <input
                  type="email"
                  value={formDataCustomer.email}
                  onChange={(e) => setFormDataCustomer({ ...formDataCustomer, email: e.target.value })}
                  placeholder="email@exemplo.com"
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    border: '1px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    outline: 'none'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                  Telefone
                </label>
                <input
                  type="text"
                  value={formDataCustomer.phone}
                  onChange={(e) => {
                    let value = e.target.value.replace(/\D/g, '');
                    if (value.length <= 11) {
                      value = value.replace(/^(\d{2})(\d)/, '($1) $2');
                      value = value.replace(/(\d{5})(\d)/, '$1-$2');
                      setFormDataCustomer({ ...formDataCustomer, phone: value });
                    }
                  }}
                  placeholder="(00) 00000-0000"
                  maxLength={15}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    border: '1px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    outline: 'none'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                  CPF
                </label>
                <input
                  type="text"
                  value={formDataCustomer.cpf}
                  onChange={(e) => {
                    let value = e.target.value.replace(/\D/g, '');
                    if (value.length <= 11) {
                      value = value.replace(/(\d{3})(\d)/, '$1.$2');
                      value = value.replace(/(\d{3})(\d)/, '$1.$2');
                      value = value.replace(/(\d{3})(\d{1,2})$/, '$1-$2');
                      setFormDataCustomer({ ...formDataCustomer, cpf: value });
                    }
                  }}
                  placeholder="000.000.000-00"
                  maxLength={14}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    border: '1px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    outline: 'none'
                  }}
                />
              </div>

              <button
                type="submit"
                style={{
                  padding: '12px',
                  background: 'linear-gradient(135deg, #2563eb, #1d4ed8)',
                  color: 'white',
                  border: 'none',
                  borderRadius: '8px',
                  fontSize: '14px',
                  fontWeight: '600',
                  cursor: 'pointer',
                  transition: 'all 0.2s'
                }}
              >
                Cadastrar Cliente
              </button>
            </form>
          </div>
        </div>
      )}

      <style>{`
        @keyframes slideInUp {
          from {
            opacity: 0;
            transform: translateY(20px);
          }
          to {
            opacity: 1;
            transform: translateY(0);
          }
        }
      `}</style>
    </div>
  );
}