import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';
import { authService } from '../../../services/authService';
import { Header } from '../../../components/common/Header';
import { Sidebar } from '../../../components/common/Sidebar';

export default function GerenciarConfiguracoes() {
    const navigate = useNavigate();
    const { user, clearAuth } = useAuth();
    const [sidebarOpen, setSidebarOpen] = useState(true);

    const handleLogout = () => {
        authService.logout();
        clearAuth();
        navigate('/login');
    };

    const handleNavigate = (view: string) => {
        const routes: Record<string, string> = {
            'dashboard': '/admin/dashboard',
            'bookings': '/admin/bookings',
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
                    <h2 style={{ margin: '0 0 24px 0', fontSize: '28px', fontWeight: '700', color: '#111' }}>
                        ⚙️ Configurações do Sistema
                    </h2>
                    <div style={{ background: 'white', padding: '24px', borderRadius: '12px', border: '1px solid #e5e7eb', color: '#4b5563' }}>
                        <p>Aqui serão gerenciadas configurações gerais da empresa, como horários, integrações, etc.</p>
                        <p style={{ marginTop: '12px', fontSize: '14px', color: '#6b7280' }}>
                            Funcionalidades planejadas:
                        </p>
                        <ul style={{ color: '#6b7280', fontSize: '14px' }}>
                            <li>Configuração de horários de funcionamento</li>
                            <li>Integrações com sistemas externos</li>
                            <li>Parâmetros de notificações</li>
                            <li>Configurações de segurança</li>
                        </ul>
                    </div>
                </main>
            </div>
        </div>
    );
}