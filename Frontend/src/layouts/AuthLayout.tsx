import React from 'react';
import './AuthLayout.css';

const AuthLayout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    return (
        <div className="auth-layout">
            <div className="auth-container">
                {children}
            </div>
        </div>
    );
};

export default AuthLayout;