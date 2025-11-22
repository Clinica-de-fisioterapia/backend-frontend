export const ROLES = {
  ADMIN: 'admin',
  RECEPTIONIST: 'receptionist',
  PROFESSIONAL: 'professional'
} as const;

export const ROLE_LABELS = {
  admin: 'Administrador',
  receptionist: 'Recepcionista',
  professional: 'Profissional'
} as const;

export const ROLE_ROUTES = {
  admin: '/admin/dashboard',
  receptionist: '/receptionist/hub',
  professional: '/professional/bookings'
} as const;

export const API_ENDPOINTS = {
  AUTH_LOGIN: '/auth/login',
  AUTH_SIGNUP: '/auth/signup',
  AUTH_LOGOUT: '/auth/logout',
  AUTH_REFRESH: '/auth/refresh',
  
  USERS: '/users',
  PROFESSIONALS: '/professionals',
  CUSTOMERS: '/customers',
  BOOKINGS: '/bookings',
  SERVICES: '/services',
  UNITS: '/units',
  PEOPLE: '/people',
} as const;