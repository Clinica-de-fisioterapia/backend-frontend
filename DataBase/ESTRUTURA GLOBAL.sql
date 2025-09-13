-- =====================================================================
-- ESTRUTURA GLOBAL (SCHEMA PUBLIC)
-- Tabelas que gerenciam todos os tenants. Execute apenas uma vez.
-- =====================================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Tabela de Planos (GLOBAL)
CREATE TABLE public.plans (
    plan_id SERIAL PRIMARY KEY,                                     -- Ex: 1
    name VARCHAR(100) NOT NULL UNIQUE,                              -- Ex: 'Básico'
    description TEXT,                                               -- Ex: 'Ideal para profissionais autônomos...'
    price_monthly DECIMAL(10, 2) NOT NULL DEFAULT 0.00,             -- Ex: 89.90
    max_units INT NOT NULL DEFAULT 1,                               -- Ex: 1
    max_professionals INT NOT NULL DEFAULT 5,                       -- Ex: 5
    max_customers INT,                                              -- Ex: 1000 ou NULL para ilimitado
    allow_dedicated_database BOOLEAN NOT NULL DEFAULT FALSE,        -- Ex: false
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 19:46:43-03'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 19:46:43-03'
    is_active BOOLEAN NOT NULL DEFAULT TRUE                         -- Ex: true
);

-- Tabela de Tenants (GLOBAL)
CREATE TABLE public.tenants (
    tenant_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),          -- Ex: 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d'
    company_name VARCHAR(255) NOT NULL,                             -- Ex: 'Barbearia do Zé'
    cnpj CHAR(14) UNIQUE,                                           -- Ex: '12345678000199' (apenas dígitos)
    subdomain VARCHAR(100) NOT NULL UNIQUE CHECK (subdomain ~ '^[a-z0-9]+(?:-[a-z0-9]+)*$'), -- Ex: 'barbearia-do-ze'
    tenancy_model VARCHAR(50) NOT NULL CHECK (tenancy_model IN ('shared_schema', 'dedicated_database')) DEFAULT 'shared_schema', -- Ex: 'shared_schema'
    db_identifier VARCHAR(255) NOT NULL UNIQUE,                     -- Ex: 'tenant_a1b2c3d4' ou 'secret-id-xyz'
    is_active BOOLEAN NOT NULL DEFAULT TRUE,                        -- Ex: true
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 19:46:43-03'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 19:46:43-03'
    deleted_at TIMESTAMP WITH TIME ZONE,                            -- Ex: NULL
    created_by UUID,                                                -- Ex: 'f1g2h3i4-j5k6-7l8m-9n0o-p1q2r3s4t5u6'
    updated_by UUID                                                 -- Ex: NULL
);

-- Tabela de Assinaturas (GLOBAL)
CREATE TABLE public.subscriptions (
    subscription_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),    -- Ex: 'b1c2d3e4-f5g6-7h8i-9j0k-l1m2n3o4p5q6'
    tenant_id UUID NOT NULL REFERENCES public.tenants(tenant_id),   -- Ex: 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d'
    plan_id INT NOT NULL REFERENCES public.plans(plan_id),          -- Ex: 1
    status VARCHAR(50) NOT NULL CHECK (status IN ('active', 'canceled', 'past_due', 'trial')) DEFAULT 'trial', -- Ex: 'active'
    start_date TIMESTAMP WITH TIME ZONE NOT NULL,                   -- Ex: '2025-09-12 19:46:43-03'
    end_date TIMESTAMP WITH TIME ZONE,                              -- Ex: NULL
    trial_ends_at TIMESTAMP WITH TIME ZONE,                         -- Ex: '2025-09-26 19:46:43-03'
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 19:46:43-03'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 19:46:43-03'
    created_by UUID,                                                -- Ex: NULL
    updated_by UUID                                                 -- Ex: NULL
);