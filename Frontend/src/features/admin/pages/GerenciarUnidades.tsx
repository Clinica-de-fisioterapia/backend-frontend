import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';
import { authService } from '../../../services/authService';
import { Header } from '../../../components/common/Header';
import { Sidebar } from '../../../components/common/Sidebar';
import { Alert } from '../../../components/common/Alert';
import { LoadingSpinner } from '../../../components/common/LoadingSpinner';
import { Unit } from '../../../types';
import { unitApi } from '../../../services/api/unitApi';
import { Plus, Edit, Trash2, Search, X, Building2, MapPin } from 'lucide-react';

export default function GerenciarUnidades() {
  const navigate = useNavigate();
  const { user, clearAuth } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [units, setUnits] = useState<Unit[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingUnit, setEditingUnit] = useState<Unit | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    address: '',
    phone: '',
    operating_hours: ''
  });

  useEffect(() => {
    fetchUnits();
  }, []);

  const fetchUnits = async () => {
    try {
      setLoading(true);
      const data = await unitApi.getAll();
      setUnits(data);
      setError('');
    } catch (err: any) {
      setError(err.message || 'Erro ao carregar unidades');
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

  const handleOpenModal = (unit?: Unit) => {
    if (unit) {
      setEditingUnit(unit);
      setFormData({
        name: unit.name,
        address: (unit as any).address || '',
        phone: (unit as any).phone || '',
        operating_hours: (unit as any).operating_hours || ''
      });
    } else {
      setEditingUnit(null);
      setFormData({
        name: '',
        address: '',
        phone: '',
        operating_hours: ''
      });
    }
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingUnit(null);
    setFormData({
      name: '',
      address: '',
      phone: '',
      operating_hours: ''
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    try {
      if (editingUnit) {
        await unitApi.update(editingUnit.id, formData as any);
        setSuccess('Unidade atualizada com sucesso!');
      } else {
        await unitApi.create(formData as any);
        setSuccess('Unidade criada com sucesso!');
      }
      
      handleCloseModal();
      fetchUnits();
    } catch (err: any) {
      setError(err.message || 'Erro ao salvar unidade');
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (window.confirm(`Tem certeza que deseja deletar a unidade "${name}"?`)) {
      try {
        await unitApi.delete(id);
        setSuccess('Unidade deletada com sucesso!');
        fetchUnits();
      } catch (err: any) {
        setError(err.message || 'Erro ao deletar unidade');
      }
    }
  };

  const filteredUnits = units.filter(unit =>
    unit.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    ((unit as any).address && (unit as any).address.toLowerCase().includes(searchTerm.toLowerCase()))
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
              🏢 Gerenciar Unidades
            </h2>
            <p style={{ margin: 0, color: '#6b7280', fontSize: '14px' }}>
              Cadastre e gerencie as unidades de atendimento
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
                placeholder="Buscar por nome ou endereço..."
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
              <Plus size={18} /> Nova Unidade
            </button>
          </div>

          {loading ? (
            <div style={{ display: 'flex', justifyContent: 'center', padding: '40px' }}>
              <LoadingSpinner size="medium" />
            </div>
          ) : (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))', gap: '20px' }}>
              {filteredUnits.map((unit) => (
                <div 
                  key={unit.id} 
                  style={{
                    background: 'white',
                    borderRadius: '12px',
                    border: '1px solid #e5e7eb',
                    padding: '20px',
                    transition: 'all 0.2s',
                    cursor: 'default'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.boxShadow = '0 10px 25px rgba(0,0,0,0.08)';
                    e.currentTarget.style.transform = 'translateY(-4px)';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.boxShadow = 'none';
                    e.currentTarget.style.transform = 'translateY(0)';
                  }}
                >
                  <div style={{ display: 'flex', alignItems: 'flex-start', gap: '12px', marginBottom: '16px' }}>
                    <div style={{
                      background: 'linear-gradient(135deg, #3b82f6, #2563eb)',
                      borderRadius: '12px',
                      padding: '12px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center'
                    }}>
                      <Building2 size={24} color="white" />
                    </div>
                    <div style={{ flex: 1 }}>
                      <h3 style={{ margin: '0 0 4px 0', fontSize: '18px', fontWeight: '600', color: '#111' }}>
                        {unit.name}
                      </h3>
                      <p style={{ margin: 0, fontSize: '12px', color: '#6b7280' }}>
                        Criada em {new Date(unit.created_at || '').toLocaleDateString('pt-BR')}
                      </p>
                    </div>
                  </div>

                  <div style={{ display: 'grid', gap: '10px', marginBottom: '16px' }}>
                    {(unit as any).address && (
                      <div style={{ display: 'flex', alignItems: 'flex-start', gap: '8px' }}>
                        <MapPin size={16} style={{ color: '#6b7280', marginTop: '2px', flexShrink: 0 }} />
                        <span style={{ fontSize: '14px', color: '#4b5563', lineHeight: '1.4' }}>
                          {(unit as any).address}
                        </span>
                      </div>
                    )}
                    {(unit as any).phone && (
                      <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                        <span style={{ fontSize: '16px' }}>📞</span>
                        <span style={{ fontSize: '14px', color: '#4b5563' }}>
                          {(unit as any).phone}
                        </span>
                      </div>
                    )}
                    {(unit as any).operating_hours && (
                      <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                        <span style={{ fontSize: '16px' }}>🕐</span>
                        <span style={{ fontSize: '14px', color: '#4b5563' }}>
                          {(unit as any).operating_hours}
                        </span>
                      </div>
                    )}
                  </div>

                  <div style={{ 
                    display: 'flex', 
                    gap: '8px', 
                    paddingTop: '16px', 
                    borderTop: '1px solid #e5e7eb' 
                  }}>
                    <button
                      onClick={() => handleOpenModal(unit)}
                      style={{ 
                        flex: 1,
                        background: '#3b82f6', 
                        color: 'white', 
                        padding: '8px 12px', 
                        borderRadius: '6px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        gap: '6px',
                        border: 'none',
                        cursor: 'pointer',
                        fontSize: '14px',
                        fontWeight: '500',
                        transition: 'background 0.2s'
                      }}
                      onMouseEnter={(e) => { e.currentTarget.style.background = '#2563eb'; }}
                      onMouseLeave={(e) => { e.currentTarget.style.background = '#3b82f6'; }}
                    >
                      <Edit size={16} /> Editar
                    </button>
                    <button
                      onClick={() => handleDelete(unit.id, unit.name)}
                      style={{ 
                        background: '#ef4444', 
                        color: 'white', 
                        padding: '8px 12px', 
                        borderRadius: '6px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
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
                </div>
              ))}
              {filteredUnits.length === 0 && (
                <div style={{ 
                  gridColumn: '1 / -1',
                  textAlign: 'center', 
                  padding: '60px 20px', 
                  color: '#6b7280',
                  background: 'white',
                  borderRadius: '12px',
                  border: '2px dashed #e5e7eb'
                }}>
                  <Building2 size={48} style={{ color: '#d1d5db', marginBottom: '16px' }} />
                  <p style={{ margin: 0, fontSize: '16px', fontWeight: '500' }}>
                    {searchTerm ? 'Nenhuma unidade encontrada.' : 'Nenhuma unidade cadastrada.'}
                  </p>
                  {!searchTerm && (
                    <p style={{ margin: '8px 0 0 0', fontSize: '14px' }}>
                      Clique em "Nova Unidade" para começar
                    </p>
                  )}
                </div>
              )}
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
                {editingUnit ? 'Editar Unidade' : 'Nova Unidade'}
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
                  Nome da Unidade *
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  required
                  placeholder="Ex: Unidade Centro, Filial Norte..."
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
                  Endereço Completo
                </label>
                <textarea
                  value={formData.address}
                  onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                  placeholder="Rua, número, bairro, cidade, estado..."
                  rows={3}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    border: '1px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    boxSizing: 'border-box',
                    outline: 'none',
                    fontFamily: 'inherit',
                    resize: 'vertical'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', fontWeight: '500', color: '#111' }}>
                  Telefone
                </label>
                <input
                  type="text"
                  value={formData.phone}
                  onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                  placeholder="(XX) XXXXX-XXXX"
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
                  Horário de Funcionamento
                </label>
                <input
                  type="text"
                  value={formData.operating_hours}
                  onChange={(e) => setFormData({ ...formData, operating_hours: e.target.value })}
                  placeholder="Ex: Seg-Sex 8h às 18h, Sáb 8h às 12h"
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
                  {editingUnit ? 'Atualizar' : 'Criar'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}