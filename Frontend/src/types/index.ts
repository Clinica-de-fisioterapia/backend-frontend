export type UserRole = 'admin' | 'receptionist' | 'professional';

export interface User {
  id: string;
  full_name: string;
  email: string;
  role: UserRole;
  is_active: boolean;
  created_at: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
  expiresIn: number;
}

export interface Professional extends User {
  specialty?: string;
  registry_code?: string;
  person_id: string;
}

export interface Customer {
  id: string;
  full_name: string;
  email?: string;
  phone?: string;
  cpf?: string;
  person_id: string;
  created_at: string;
}

export interface Booking {
  id: string;
  professional_id: string;
  customer_id: string;
  service_id: string;
  unit_id: string;
  start_time: string;
  end_time: string;
  status: 'confirmed' | 'pending' | 'cancelled' | 'completed';
  created_at: string;
}

export interface Service {
  id: string;
  name: string;
  duration_minutes: number;
  price: number;
  created_at?: string;
}

export interface Unit {
  id: string;
  name: string;
  created_at?: string;
}

export interface ScheduleException {
  id: string;
  title: string;
  start_time: string;
  end_time: string;
  is_blocker: boolean;
  professional_id?: string;
  unit_id?: string;
  created_at?: string;
}

export interface Person {
  id: string;
  full_name: string;
  email?: string;
  phone?: string;
  cpf?: string;
  created_at?: string;
}