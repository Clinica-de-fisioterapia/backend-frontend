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
    full_name: '',
    email: '',
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
        specialty: professional.specialty || '',
        registry_code: professional.registry_code || '',
        is_active: professional.is_active
      });
    } else {
      setEditingProfessional(null);
      setFormData({
        full_name: '',
        email: '',
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
        await professionalApi.update(editingProfessional.id, formData as any);
        setSuccess('Profissional atualizado com sucesso!');
      } else {
        await professionalApi.create({
          ...formData,
          role: 'professional',
          person_id: `person_${Date.now()}`
        } as any);
        setSuccess('Profissional criado com sucesso!');
      }
      
      handleCloseModal();
      fetchProfessionals();
    } catch (err: any) {
      setError(err.message || 'Erro ao salvar profissional');
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
    prof.full_name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    prof.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
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
                cursor: 'pointer',
                transition: 'background 0.2s'
              }}
              onMouseEnter={(e) => { e.currentTarget.style.background = '#1d4ed8'; }}
              onMouseLeave={(e) => { e.currentTarget.style.background = '#2563eb'; }}
            >
              <Plus size={18} /> Novo Profissional
            </button>
          </div>

          {loading ? (
            <div style={{ display: 'flex', justifyContent: 'center', padding: '40px' }}>
              <LoadingSpinner size="medium" />
            </div>
          ) : (
            <div style={{ background: 'white', borderRadius: '12px', border: '1px solid #e5e7eb', overflow: 'hidden' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ background: '#f9fafb', borderBottom: '2px solid #e5e7eb' }}>
                    <th style={{ padding: '16px', textAlign: 'left', fontWeight: '600', color: '#111', fontSize: '14px' }}>Nome</th>
                    <th style={{ padding: '16px', textAlign: 'left', fontWeight: '600', color: '#111', fontSize: '14px' }}>Email</th>
                    <th style={{ padding: '16px', textAlign: 'left', fontWeight: '600', color: '#111', fontSize: '14px' }}>Especialidade</th>
                    <th style={{ padding: '16px', textAlign: 'left', fontWeight: '600', color: '#111', fontSize: '14px' }}>Registro</th>
                    <th style={{ padding: '16px', textAlign: 'center', fontWeight: '600', color: '#111', fontSize: '14px' }}>Status</th>
                    <th style={{ padding: '16px', textAlign: 'center', fontWeight: '600', color: '#111', fontSize: '14px' }}>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredProfessionals.map((professional) => (
                    <tr key={professional.id} style={{ borderBottom: '1px solid #e5e7eb', transition: 'background 0.2s' }}
                        onMouseEnter={(e) => { e.currentTarget.style.background = '#f9fafb'; }}
                        onMouseLeave={(e) => { e.currentTarget.style.background = 'white'; }}>
                      <td style={{ padding: '16px', color: '#111', fontWeight: '500' }}>{professional.full_name}</td>
                      <td style={{ padding: '16px', color: '#6b7280', fontSize: '14px' }}>{professional.email}</td>
                      <td style={{ padding: '16px', color: '#6b7280', fontSize: '14px' }}>{professional.specialty || '-'}</td>
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
                          {professional.is_active ? 'Ativo' : 'Inativo'}
                        </span>
                      </td>
                      <td style={{ padding: '16px', textAlign: 'center' }}>
                        <div style={{ display: 'flex', gap: '8px', justifyContent: 'center' }}>
                          <button
                            onClick={() => handleOpenModal(professional)}
                            style={{ 
                              background: '#3b82f6', 
                              color: 'white', 
                              padding: '8px', 
                              borderRadius: '6px',
                              display: 'flex',
                              alignItems: 'center',
                              border: 'none',
                              cursor: 'pointer',
                              transition: 'background 0.2s'
                            }}
                            onMouseEnter={(e) => { e.currentTarget.style.background = '#2563eb'; }}
                            onMouseLeave={(e) => { e.currentTarget.style.background = '#3b82f6'; }}
                          >
                            <Edit size={16} />
                          </button>
                          <button
                            onClick={() => handleDelete(professional.id, professional.full_name)}
                            style={{ 
                              background: '#ef4444', 
                              color: 'white', 
                              padding: '8px', 
                              borderRadius: '6px',
                              display: 'flex',
                              alignItems: 'center',
                              border: 'none',
                              cursor: 'pointer',
                              transition: 'background 0.2s'
                            }}
                            onMouseEnter={(e) => { e.currentTarget.style.background = '#dc2626'; }}
                            onMouseLeave={(e) => { e.currentTarget.style.background = '#ef4444'; }}
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
                        {searchTerm ? 'Nenhum profissional encontrado.' : 'Nenhum profissional cadastrado.'}
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
          padding: '20px'
        }}>
          <div style={{
            background: 'white',
            borderRadius: '12px',
            width: '100%',
            maxWidth: '500px',
            maxHeight: '90vh',
            overflow: 'auto',
            boxShadow: '0 20px 25px -5px rgba(0,0,0,0.1)'
          }}>
            <div style={{
              padding: '24px',
              borderBottom: '1px solid #e5e7eb',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center'
            }}>
              <h3 style={{ margin: 0, fontSize: '20px', fontWeight: '600', color: '#111' }}>
                {editingProfessional ? 'Editar Profissional' : 'Novo Profissional'}
              </h3>
              <button
                onClick={handleCloseModal}
                style={{
                  background: 'none',
                  border: 'none',
                  cursor: 'pointer',
                  color: '#6b7280',
                  padding: '4px'
                }}
              >
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleSubmit} style={{ padding: '24px', display: 'grid', gap: '16px' }}>
              <div>
                <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                  Nome Completo *
                </label>
                <input
                  type="text"
                  value={formData.full_name}
                  onChange={(e) => setFormData({ ...formData, full_name: e.target.value })}
                  required
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    border: '1px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    boxSizing: 'border-box',
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
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  required
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    border: '1px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    boxSizing: 'border-box',
                    outline: 'none'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                  Especialidade
                </label>
                <input
                  type="text"
                  value={formData.specialty}
                  onChange={(e) => setFormData({ ...formData, specialty: e.target.value })}
                  placeholder="Ex: Cardiologista, Fisioterapeuta..."
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    border: '1px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    boxSizing: 'border-box',
                    outline: 'none'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                  Número de Registro
                </label>
                <input
                  type="text"
                  value={formData.registry_code}
                  onChange={(e) => setFormData({ ...formData, registry_code: e.target.value })}
                  placeholder="Ex: CRM-12345, CREFITO-6789..."
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    border: '1px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    boxSizing: 'border-box',
                    outline: 'none'
                  }}
                />
              </div>

              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <input
                  type="checkbox"
                  id="is_active"
                  checked={formData.is_active}
                  onChange={(e) => setFormData({ ...formData, is_active: e.target.checked })}
                  style={{ width: '16px', height: '16px', cursor: 'pointer' }}
                />
                <label htmlFor="is_active" style={{ fontSize: '14px', color: '#111', cursor: 'pointer' }}>
                  Profissional Ativo
                </label>
              </div>

              <div style={{ display: 'flex', gap: '12px', marginTop: '8px' }}>
                <button
                  type="button"
                  onClick={handleCloseModal}
                  style={{
                    flex: 1,
                    padding: '10px',
                    background: '#f3f4f6',
                    color: '#111',
                    border: 'none',
                    borderRadius: '8px',
                    fontSize: '14px',
                    fontWeight: '600',
                    cursor: 'pointer'
                  }}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  style={{
                    flex: 1,
                    padding: '10px',
                    background: '#2563eb',
                    color: 'white',
                    border: 'none',
                    borderRadius: '8px',
                    fontSize: '14px',
                    fontWeight: '600',
                    cursor: 'pointer'
                  }}
                >
                  {editingProfessional ? 'Atualizar' : 'Criar'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}