import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';
import { authService } from '../../../services/authService';
import { Header } from '../../../components/common/Header';
import { Sidebar } from '../../../components/common/Sidebar';
import { Alert } from '../../../components/common/Alert';
import { LoadingSpinner } from '../../../components/common/LoadingSpinner';
import { Professional } from '../../../types';
import { professionalApi } from '../../../services/api/professionalApi';
import apiClient from '../../../services/apiClient';
import { Plus, Edit, Trash2, Search, X } from 'lucide-react';

export default function GerenciarProfissionais() {
  const navigate = useNavigate();
  const { user, clearAuth } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [professionals, setProfessionals] = useState<Professional[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingProfessional, setEditingProfessional] = useState<Professional | null>(null);
  const [formData, setFormData] = useState({
    // Dados da Pessoa
    full_name: '',
    email: '',
    phone: '',
    cpf: '',
    // Dados do Profissional
    specialty: '',
    registry_code: '',
    is_active: true
  });

  useEffect(() => {
    fetchProfessionals();
  }, []);

  const fetchProfessionals = async () => {
    try {
      setLoading(true);
      const data = await professionalApi.getAll();
      setProfessionals(data);
      setError('');
    } catch (err: any) {
      setError(err.message || 'Erro ao carregar profissionais');
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
    const routes: Record<string, string> = {
      'dashboard': '/admin/dashboard',
      'bookings': '/admin/calendar',
      'professionals': '/admin/professionals',
      'customers': '/admin/customers',
      'units': '/admin/units',
      'settings': '/admin/settings',
    };

    const route = routes[view];
    if (route) {
      navigate(route);
    }
  };

  const handleOpenModal = (professional?: Professional) => {
    if (professional) {
      setEditingProfessional(professional);
      setFormData({
        full_name: professional.full_name,
        email: professional.email,
        phone: (professional as any).phone || '',
        cpf: (professional as any).cpf || '',
        specialty: professional.specialty || '',
        registry_code: professional.registry_code || '',
        is_active: professional.is_active
      });
    } else {
      setEditingProfessional(null);
      setFormData({
        full_name: '',
        email: '',
        phone: '',
        cpf: '',
        specialty: '',
        registry_code: '',
        is_active: true
      });
    }
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingProfessional(null);
    setFormData({
      full_name: '',
      email: '',
      phone: '',
      cpf: '',
      specialty: '',
      registry_code: '',
      is_active: true
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    try {
      if (editingProfessional) {
        // Atualizar apenas dados profissionais
        await professionalApi.update(editingProfessional.id, {
          specialty: formData.specialty,
          registry_code: formData.registry_code,
          is_active: formData.is_active
        } as any);
        setSuccess('Profissional atualizado com sucesso!');
      } else {
        // ✅ Usando a nova função que encapsula os dois passos
        await professionalApi.create({
          fullName: formData.full_name,
          cpf: formData.cpf,
          phone: formData.phone,
          email: formData.email,
          specialty: formData.specialty,
          registryCode: formData.registry_code,
        });

        setSuccess('Profissional criado com sucesso!');
      }

      handleCloseModal();
      fetchProfessionals();
    } catch (err: any) {
      let errorMessage = 'Erro ao salvar profissional. Verifique os dados.';
      
      const responseData = err?.response?.data;
      
      if (responseData) {
          // Se for um erro de validação do ASP.NET Core (Bad Request)
          if (responseData.errors) {
              // Converte o objeto de erros em uma string legível
              const validationErrors = Object.keys(responseData.errors)
                  .map(key => {
                      const messages = responseData.errors[key];
                      // Junta as mensagens de erro (ex: "CPF: CPF inválido. Nome: Nome é obrigatório.")
                      return `${key}: ${messages.join('; ')}`;
                  })
                  .join(' | ');
              
              errorMessage = `Falha na validação: ${validationErrors}`;
          } 
          // Se for um erro genérico com título (comum em APIs .NET)
          else if (responseData.title) {
              errorMessage = responseData.title;
          }
          // Se for uma mensagem de erro direta (como um erro lançado no service)
          else if (err.message) {
              errorMessage = err.message;
          }
      }
      
      setError(errorMessage);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (window.confirm(`Tem certeza que deseja deletar ${name}?`)) {
      try {
        await professionalApi.delete(id);
        setSuccess('Profissional deletado com sucesso!');
        fetchProfessionals();
      } catch (err: any) {
        setError(err.message || 'Erro ao deletar profissional');
      }
    }
  };

  const filteredProfessionals = professionals.filter(prof =>
    prof.registry_code && prof.registry_code.toLowerCase().includes(searchTerm.toLowerCase()) ||
    prof.full_name && prof.full_name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    prof.email && prof.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (prof.specialty && prof.specialty.toLowerCase().includes(searchTerm.toLowerCase()))
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
          role="admin"
          onNavigate={handleNavigate}
        />
        <main style={{ flex: 1, overflowY: 'auto', padding: '24px' }}>
          <div style={{ marginBottom: '24px' }}>
            <h2 style={{ margin: '0 0 8px 0', fontSize: '28px', fontWeight: '700', color: '#111' }}>
              👩‍⚕️ Gerenciar Profissionais
            </h2>
            <p style={{ margin: 0, color: '#6b7280', fontSize: '14px' }}>
              Cadastre e gerencie os profissionais de saúde
            </p>
          </div>

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
                placeholder="Buscar por nome, email ou especialidade..."
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
              onClick={() => handleOpenModal()}
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
              <Plus size={18} /> Novo Profissional
            </button>
          </div>

          {loading ? (
            <div style={{ display: 'flex', justifyContent: 'center', padding: '40px' }}>
              <LoadingSpinner size="medium" />
            </div>
          ) : (
            <div style={{ background: 'white', borderRadius: '12px', border: '1px solid #e5e7eb', overflow: 'hidden', boxShadow: '0 1px 3px rgba(0,0,0,0.05)' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ background: 'linear-gradient(to right, #f9fafb, #f3f4f6)', borderBottom: '2px solid #e5e7eb' }}>
                    <th style={{ padding: '16px', textAlign: 'left', fontWeight: '600', color: '#111', fontSize: '14px' }}>Nome</th>
                    <th style={{ padding: '16px', textAlign: 'left', fontWeight: '600', color: '#111', fontSize: '14px' }}>Email</th>
                    <th style={{ padding: '16px', textAlign: 'left', fontWeight: '600', color: '#111', fontSize: '14px' }}>Especialidade</th>
                    <th style={{ padding: '16px', textAlign: 'left', fontWeight: '600', color: '#111', fontSize: '14px' }}>Registro</th>
                    <th style={{ padding: '16px', textAlign: 'center', fontWeight: '600', color: '#111', fontSize: '14px' }}>Status</th>
                    <th style={{ padding: '16px', textAlign: 'center', fontWeight: '600', color: '#111', fontSize: '14px' }}>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredProfessionals.map((professional, index) => (
                    <tr key={professional.id} style={{
                      borderBottom: '1px solid #e5e7eb',
                      transition: 'background 0.2s',
                      background: index % 2 === 0 ? 'white' : '#fafbfc'
                    }}
                      onMouseEnter={(e) => { e.currentTarget.style.background = '#f0f9ff'; }}
                      onMouseLeave={(e) => { e.currentTarget.style.background = index % 2 === 0 ? 'white' : '#fafbfc'; }}>
                      <td style={{ padding: '16px', color: '#111', fontWeight: '500' }}>{professional.full_name}</td>
                      <td style={{ padding: '16px', color: '#6b7280', fontSize: '14px' }}>{professional.email}</td>
                      <td style={{ padding: '16px', color: '#6b7280', fontSize: '14px' }}>
                        {professional.specialty || '-'}
                      </td>
                      <td style={{ padding: '16px', color: '#6b7280', fontSize: '14px' }}>{professional.registry_code || '-'}</td>
                      <td style={{ padding: '16px', textAlign: 'center' }}>
                        <span style={{
                          background: professional.is_active ? '#dcfce7' : '#fee2e2',
                          color: professional.is_active ? '#166534' : '#991b1b',
                          padding: '4px 12px',
                          borderRadius: '12px',
                          fontSize: '12px',
                          fontWeight: '600'
                        }}>
                          {professional.is_active ? '✓ Ativo' : '✗ Inativo'}
                        </span>
                      </td>
                      <td style={{ padding: '16px', textAlign: 'center' }}>
                        <div style={{ display: 'flex', gap: '8px', justifyContent: 'center' }}>
                          <button
                            onClick={() => handleOpenModal(professional)}
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
                            onClick={() => handleDelete(professional.id, professional.full_name)}
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
                  {filteredProfessionals.length === 0 && (
                    <tr>
                      <td colSpan={6} style={{ textAlign: 'center', padding: '40px', color: '#6b7280' }}>
                        {searchTerm ? '🔍 Nenhum profissional encontrado.' : '📋 Nenhum profissional cadastrado.'}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          )}
        </main>
      </div>

      {/* Modal */}
      {showModal && (
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
                {editingProfessional ? '✏️ Editar Profissional' : '➕ Novo Profissional'}
              </h3>
              <button
                onClick={handleCloseModal}
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

            <form onSubmit={handleSubmit} style={{ padding: '24px', display: 'grid', gap: '20px' }}>
              {/* Seção: Dados Pessoais */}
              <div>
                <div style={{
                  fontSize: '16px',
                  fontWeight: '600',
                  color: '#2563eb',
                  marginBottom: '16px',
                  paddingBottom: '8px',
                  borderBottom: '2px solid #e0e7ff',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '8px'
                }}>
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                    <circle cx="12" cy="7" r="4"></circle>
                  </svg>
                  Dados Pessoais
                </div>

                <div style={{ display: 'grid', gap: '16px' }}>
                  <div>
                    <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                      Nome Completo *
                    </label>
                    <input
                      type="text"
                      value={formData.full_name}
                      onChange={(e) => setFormData({ ...formData, full_name: e.target.value })}
                      required
                      disabled={!!editingProfessional}
                      placeholder="Ex: Dr. João da Silva"
                      style={{
                        width: '100%',
                        padding: '10px 12px',
                        border: '1px solid #e5e7eb',
                        borderRadius: '8px',
                        fontSize: '14px',
                        boxSizing: 'border-box',
                        outline: 'none',
                        background: editingProfessional ? '#f9fafb' : 'white',
                        transition: 'border-color 0.2s'
                      }}
                      onFocus={(e) => !editingProfessional && (e.target.style.borderColor = '#2563eb')}
                      onBlur={(e) => !editingProfessional && (e.target.style.borderColor = '#e5e7eb')}
                    />
                    {editingProfessional && (
                      <p style={{ fontSize: '12px', color: '#6b7280', marginTop: '4px', fontStyle: 'italic' }}>
                        ℹ️ Não é possível alterar dados pessoais ao editar
                      </p>
                    )}
                  </div>

                  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
                    <div>
                      <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                        CPF *
                      </label>
                      <input
                        type="text"
                        value={formData.cpf}
                        onChange={(e) => {
                          let value = e.target.value.replace(/\D/g, '');
                          if (value.length <= 11) {
                            value = value.replace(/(\d{3})(\d)/, '$1.$2');
                            value = value.replace(/(\d{3})(\d)/, '$1.$2');
                            value = value.replace(/(\d{3})(\d{1,2})$/, '$1-$2');
                            setFormData({ ...formData, cpf: value });
                          }
                        }}
                        required={!editingProfessional}
                        disabled={!!editingProfessional}
                        placeholder="000.000.000-00"
                        maxLength={14}
                        style={{
                          width: '100%',
                          padding: '10px 12px',
                          border: '1px solid #e5e7eb',
                          borderRadius: '8px',
                          fontSize: '14px',
                          boxSizing: 'border-box',
                          outline: 'none',
                          background: editingProfessional ? '#f9fafb' : 'white',
                          transition: 'border-color 0.2s'
                        }}
                        onFocus={(e) => !editingProfessional && (e.target.style.borderColor = '#2563eb')}
                        onBlur={(e) => !editingProfessional && (e.target.style.borderColor = '#e5e7eb')}
                      />
                    </div>

                    <div>
                      <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                        Telefone *
                      </label>
                      <input
                        type="text"
                        value={formData.phone}
                        onChange={(e) => {
                          let value = e.target.value.replace(/\D/g, '');
                          if (value.length <= 11) {
                            value = value.replace(/^(\d{2})(\d)/, '($1) $2');
                            value = value.replace(/(\d{5})(\d)/, '$1-$2');
                            setFormData({ ...formData, phone: value });
                          }
                        }}
                        required={!editingProfessional}
                        disabled={!!editingProfessional}
                        placeholder="(00) 00000-0000"
                        maxLength={15}
                        style={{
                          width: '100%',
                          padding: '10px 12px',
                          border: '1px solid #e5e7eb',
                          borderRadius: '8px',
                          fontSize: '14px',
                          boxSizing: 'border-box',
                          outline: 'none',
                          background: editingProfessional ? '#f9fafb' : 'white',
                          transition: 'border-color 0.2s'
                        }}
                        onFocus={(e) => !editingProfessional && (e.target.style.borderColor = '#2563eb')}
                        onBlur={(e) => !editingProfessional && (e.target.style.borderColor = '#e5e7eb')}
                      />
                    </div>
                  </div>

                  <div>
                    <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                      Email *
                    </label>
                    <input
                      type="email"
                      value={formData.email}
                      onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                      required={!editingProfessional}
                      disabled={!!editingProfessional}
                      placeholder="email@exemplo.com"
                      style={{
                        width: '100%',
                        padding: '10px 12px',
                        border: '1px solid #e5e7eb',
                        borderRadius: '8px',
                        fontSize: '14px',
                        boxSizing: 'border-box',
                        outline: 'none',
                        background: editingProfessional ? '#f9fafb' : 'white',
                        transition: 'border-color 0.2s'
                      }}
                      onFocus={(e) => !editingProfessional && (e.target.style.borderColor = '#2563eb')}
                      onBlur={(e) => !editingProfessional && (e.target.style.borderColor = '#e5e7eb')}
                    />
                  </div>
                </div>
              </div>

              {/* Seção: Dados Profissionais */}
              <div>
                <div style={{
                  fontSize: '16px',
                  fontWeight: '600',
                  color: '#2563eb',
                  marginBottom: '16px',
                  paddingBottom: '8px',
                  borderBottom: '2px solid #e0e7ff',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '8px'
                }}>
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                    <polyline points="14 2 14 8 20 8"></polyline>
                    <line x1="16" y1="13" x2="8" y2="13"></line>
                    <line x1="16" y1="17" x2="8" y2="17"></line>
                    <polyline points="10 9 9 9 8 9"></polyline>
                  </svg>
                  Dados Profissionais
                </div>

                <div style={{ display: 'grid', gap: '16px' }}>
                  <div>
                    <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                      Especialidade *
                    </label>
                    <input
                      type="text"
                      value={formData.specialty}
                      onChange={(e) => setFormData({ ...formData, specialty: e.target.value })}
                      required
                      placeholder="Ex: Cardiologista, Fisioterapeuta, Enfermeiro..."
                      style={{
                        width: '100%',
                        padding: '10px 12px',
                        border: '1px solid #e5e7eb',
                        borderRadius: '8px',
                        fontSize: '14px',
                        boxSizing: 'border-box',
                        outline: 'none',
                        transition: 'border-color 0.2s'
                      }}
                      onFocus={(e) => (e.target.style.borderColor = '#2563eb')}
                      onBlur={(e) => (e.target.style.borderColor = '#e5e7eb')}
                    />
                  </div>

                  <div>
                    <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                      Número de Registro Profissional
                    </label>
                    <input
                      type="text"
                      value={formData.registry_code}
                      onChange={(e) => setFormData({ ...formData, registry_code: e.target.value })}
                      placeholder="Ex: CRM-12345, CREFITO-6789, COREN-3456..."
                      style={{
                        width: '100%',
                        padding: '10px 12px',
                        border: '1px solid #e5e7eb',
                        borderRadius: '8px',
                        fontSize: '14px',
                        boxSizing: 'border-box',
                        outline: 'none',
                        transition: 'border-color 0.2s'
                      }}
                      onFocus={(e) => (e.target.style.borderColor = '#2563eb')}
                      onBlur={(e) => (e.target.style.borderColor = '#e5e7eb')}
                    />
                  </div>

                  <div style={{
                    background: 'linear-gradient(135deg, #eff6ff, #dbeafe)',
                    padding: '12px',
                    borderRadius: '8px',
                    border: '1px solid #bfdbfe'
                  }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                      <input
                        type="checkbox"
                        id="is_active"
                        checked={formData.is_active}
                        onChange={(e) => setFormData({ ...formData, is_active: e.target.checked })}
                        style={{
                          width: '18px',
                          height: '18px',
                          cursor: 'pointer',
                          accentColor: '#2563eb'
                        }}
                      />
                      <label htmlFor="is_active" style={{ fontSize: '14px', color: '#111', cursor: 'pointer', fontWeight: '500' }}>
                        Profissional Ativo
                      </label>
                    </div>
                    <p style={{ fontSize: '12px', color: '#1e40af', margin: '6px 0 0 28px' }}>
                      Profissionais inativos não aparecem para agendamento
                    </p>
                  </div>
                </div>
              </div>

              {/* Botões */}
              <div style={{
                display: 'flex',
                gap: '12px',
                marginTop: '8px',
                paddingTop: '16px',
                borderTop: '1px solid #e5e7eb'
              }}>
                <button
                  type="button"
                  onClick={handleCloseModal}
                  style={{
                    flex: 1,
                    padding: '12px',
                    background: 'white',
                    color: '#6b7280',
                    border: '1px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    fontWeight: '600',
                    cursor: 'pointer',
                    transition: 'all 0.2s'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.background = '#f9fafb';
                    e.currentTarget.style.borderColor = '#d1d5db';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.background = 'white';
                    e.currentTarget.style.borderColor = '#e5e7eb';
                  }}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  style={{
                    flex: 1,
                    padding: '12px',
                    background: 'linear-gradient(135deg, #2563eb, #1d4ed8)',
                    color: 'white',
                    border: 'none',
                    borderRadius: '8px',
                    fontSize: '14px',
                    fontWeight: '600',
                    cursor: 'pointer',
                    transition: 'all 0.2s',
                    boxShadow: '0 4px 6px rgba(37, 99, 235, 0.2)'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.transform = 'translateY(-1px)';
                    e.currentTarget.style.boxShadow = '0 6px 12px rgba(37, 99, 235, 0.3)';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.transform = 'translateY(0)';
                    e.currentTarget.style.boxShadow = '0 4px 6px rgba(37, 99, 235, 0.2)';
                  }}
                >
                  {editingProfessional ? '✓ Atualizar Profissional' : '+ Criar Profissional'}
                </button>
              </div>
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