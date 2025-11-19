import apiClient from './apiClient';

export interface LoginReq { Email: string; Password: string; }
export interface SignupReq {
  CompanyName: string;
  Subdomain: string;
  AdminFullName: string;
  AdminEmail: string;
  AdminPassword: string;
}

export const authService = {
  login: async (email: string, password: string, tenant = 'default') => {
    const payload: LoginReq = { Email: email, Password: password };
    const resp = await apiClient.post('/auth/login', payload, { headers: { 'X-Tenant': tenant } });
    return resp.data;
  },
  signup: async (data: SignupReq & { tenant?: string }) => {
    const payload = {
      CompanyName: data.CompanyName,
      Subdomain: data.Subdomain,
      AdminFullName: data.AdminFullName,
      AdminEmail: data.AdminEmail,
      AdminPassword: data.AdminPassword,
    };
    await apiClient.post('/auth/signup', payload, { headers: { 'X-Tenant': data.tenant ?? 'default' } });
  }
};