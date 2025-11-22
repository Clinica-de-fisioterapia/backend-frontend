import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

interface PermissionGuardProps {
  children: React.ReactNode;
  requiredRole?: string;
}

const PermissionGuard: React.FC<PermissionGuardProps> = ({ children, requiredRole }) => {
  const { user, loading } = useAuth();

  if (loading) return null; // or a spinner
  if (!user) return <Navigate to="/login" replace />;

  if (requiredRole && user.role !== requiredRole) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};

export default PermissionGuard;