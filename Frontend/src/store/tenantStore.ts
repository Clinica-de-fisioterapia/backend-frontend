import { create } from 'zustand';

interface TenantState {
  tenantId: string | null;
  tenantName: string | null;
  subdomain: string | null;
  setTenant: (id: string, name: string, subdomain: string) => void;
  clearTenant: () => void;
}

export const useTenantStore = create<TenantState>((set) => ({
  tenantId: null,
  tenantName: null,
  subdomain: null,
  setTenant: (id, name, subdomain) =>
    set({
      tenantId: id,
      tenantName: name,
      subdomain
    }),
  clearTenant: () =>
    set({
      tenantId: null,
      tenantName: null,
      subdomain: null
    })
}));