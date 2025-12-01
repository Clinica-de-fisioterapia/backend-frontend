import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';
import { authService } from '../../../services/authService';
import { Header } from '../../../components/common/Header';
import { Sidebar } from '../../../components/common/Sidebar';
import { Alert } from '../../../components/common/Alert';
import { LoadingSpinner } from '../../../components/common/LoadingSpinner';
import { Professional, Person } from '../../../types'; // Certifique-se de que Person está definido em types
import { professionalApi } from '../../../services/api/professionalApi';
import { peopleApi } from '../../../services/api/peopleApi'; // Importando peopleApi
import apiClient from '../../../services/apiClient';
import { Plus, Edit, Trash2, Search, X } from 'lucide-react';

export default function GerenciarProfissionais() {
  const navigate = useNavigate();
  const { user, clearAuth } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [professionals, setProfessionals] = useState<Professional[]>([]);
  const [people, setPeople] = useState<Person[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [showPeopleModal, setShowPeopleModal] = useState(false); // Modal para criar People
  const [showProfissionalModal, setShowProfissionalModal] = useState(false); // Modal para criar Profissional
  const [formDataPeople, setFormDataPeople] = useState({
    fullName: '',
    email: '',
    phone: '',
    cpf: '',
  });
  const [formDataProfessional, setFormDataProfessional] = useState({
    specialty: '',
    registry_code: '',
    is_active: true,
  });
  const [newPersonId, setNewPersonId] = useState<string | null>(null);

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
      dashboard: '/admin/dashboard',
      bookings: '/admin/calendar',
      professionals: '/admin/professionals',
      customers: '/admin/customers',
      units: '/admin/units',
      settings: '/admin/settings',
    };

    const route = routes[view];
    if (route) {
      navigate(route);
    }
  };

  const handleOpenPeopleModal = () => {
    setFormDataPeople({
      fullName: '',
      email: '',
      phone: '',
      cpf: '',
    });
    setShowPeopleModal(true);
  };

  const handleClosePeopleModal = () => {
    setShowPeopleModal(false);
  };

  const handleOpenProfissionalModal = () => {
    setShowProfissionalModal(true);
  };

  const handleCloseProfissionalModal = () => {
    setShowProfissionalModal(false);
  };

  const handleSubmitPeople = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    try {
      const response = await peopleApi.create(formDataPeople);
      setNewPersonId(response.id); // Salva o ID da nova pessoa
      handleClosePeopleModal(); // Fecha o modal de criação da pessoa

      // Abre o modal de criação do profissional
      handleOpenProfissionalModal();
    } catch (err: any) {
      setError('Erro ao criar pessoa. Verifique os dados.');
    }
  };

  const handleSubmitProfessional = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    try {
      await professionalApi.create({
        personId: newPersonId!,
        specialty: formDataProfessional.specialty,
      });
      setSuccess('Profissional criado com sucesso!');
      handleCloseProfissionalModal();
      fetchProfessionals(); // Atualiza a lista de profissionais
    } catch (err: any) {
      setError('Erro ao criar profissional. Verifique os dados.');
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

  const filteredPeople = people.filter((peo) =>
    peo.full_name && peo.full_name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    peo.email && peo.email.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const filteredProfessionals = professionals.filter((prof) =>
    prof.registry_code && prof.registry_code.toLowerCase().includes(searchTerm.toLowerCase()) ||
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
              onClick={handleOpenPeopleModal} // Abre o modal de criação de pessoas
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
                            onClick={() => handleOpenProfissionalModal()}
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

      {/* Modal para criar People */}
      {showPeopleModal && (
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
                ➕ Novo Pessoa
              </h3>
              <button
                onClick={handleClosePeopleModal}
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

            <form onSubmit={handleSubmitPeople} style={{ padding: '24px', display: 'grid', gap: '20px' }}>
              <div>
                <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                  Nome Completo *
                </label>
                <input
                  type="text"
                  value={formDataPeople.fullName}
                  onChange={(e) => setFormDataPeople({ ...formDataPeople, fullName: e.target.value })}
                  required
                  placeholder="Ex: João da Silva"
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
                  Email *
                </label>
                <input
                  type="email"
                  value={formDataPeople.email}
                  onChange={(e) => setFormDataPeople({ ...formDataPeople, email: e.target.value })}
                  required
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
                  Telefone *
                </label>
                <input
                  type="text"
                  value={formDataPeople.phone}
                  onChange={(e) => {
                    let value = e.target.value.replace(/\D/g, '');
                    if (value.length <= 11) {
                      value = value.replace(/^(\d{2})(\d)/, '($1) $2');
                      value = value.replace(/(\d{5})(\d)/, '$1-$2');
                      setFormDataPeople({ ...formDataPeople, phone: value });
                    }
                  }}
                  required
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
                  CPF *
                </label>
                <input
                  type="text"
                  value={formDataPeople.cpf}
                  onChange={(e) => {
                    let value = e.target.value.replace(/\D/g, '');
                    if (value.length <= 11) {
                      value = value.replace(/(\d{3})(\d)/, '$1.$2');
                      value = value.replace(/(\d{3})(\d)/, '$1.$2');
                      value = value.replace(/(\d{3})(\d{1,2})$/, '$1-$2');
                      setFormDataPeople({ ...formDataPeople, cpf: value });
                    }
                  }}
                  required
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
                Criar Pessoa
              </button>
            </form>
          </div>
        </div>
      )}

      {/* Modal para criar Profissional */}
      {showProfissionalModal && (
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
                ➕ Novo Profissional
              </h3>
              <button
                onClick={handleCloseProfissionalModal}
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

            <form onSubmit={handleSubmitProfessional} style={{ padding: '24px', display: 'grid', gap: '20px' }}>
              <div>
                <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                  Especialidade *
                </label>
                <input
                  type="text"
                  value={formDataProfessional.specialty}
                  onChange={(e) => setFormDataProfessional({ ...formDataProfessional, specialty: e.target.value })}
                  required
                  placeholder="Ex: Cardiologista, Fisioterapeuta..."
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
                  Número de Registro Profissional
                </label>
                <input
                  type="text"
                  value={formDataProfessional.registry_code}
                  onChange={(e) => setFormDataProfessional({ ...formDataProfessional, registry_code: e.target.value })}
                  placeholder="Ex: CRM-12345"
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
                  Ativo?
                </label>
                <input
                  type="checkbox"
                  checked={formDataProfessional.is_active}
                  onChange={(e) => setFormDataProfessional({ ...formDataProfessional, is_active: e.target.checked })}
                /> Sim
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
                Criar Profissional
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