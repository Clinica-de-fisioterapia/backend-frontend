import { useAuthStore } from '../store/authStore';
import { UserRole } from '../types';

export const useRole = () => {
  const user = useAuthStore((state) => state.user);

  const hasRole = (role: UserRole | UserRole[]) => {
    if (!user) return false;

    if (Array.isArray(role)) {
      return role.includes(user.role);
    }
    return user.role === role;
  };

  const isAdmin = () => user?.role === 'admin';
  const isReceptionist = () => user?.role === 'receptionist';
  const isProfessional = () => user?.role === 'professional';

  return {
    role: user?.role,
    hasRole,
    isAdmin,
    isReceptionist,
    isProfessional,
    user
  };
};