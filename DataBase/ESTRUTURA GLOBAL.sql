-- ====================================================================================
-- PARTE 1: ESTRUTURA GLOBAL (SCHEMA PUBLIC)
-- Descrição: Prepara o banco de dados com objetos compartilhados por todos os tenants.
-- Execução: Deve ser executado apenas UMA VEZ.
-- ====================================================================================

-- 1. HABILITA EXTENSÕES GLOBAIS
-- Para UUIDs, busca de texto por similaridade, e suporte a tipos B-tree em índices GiST.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
CREATE EXTENSION IF NOT EXISTS "btree_gist";

-- 2. CRIA FUNÇÕES E TIPOS GLOBAIS
-- Função para atualizar o campo 'updated_at' automaticamente.
CREATE OR REPLACE FUNCTION public.trigger_set_timestamp()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Tipo ENUM para os status de agendamento.
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'booking_status') THEN
        CREATE TYPE public.booking_status AS ENUM (
            'confirmed',
            'completed',
            'canceled',
            'no_show'
        );
    END IF;
END$$;

-- 3. CRIAÇÃO DAS TABELAS DE GERENCIAMENTO GLOBAL

-- Tabela de Planos
CREATE TABLE IF NOT EXISTS public.plans (
    plan_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    price_monthly DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    max_units INT NOT NULL DEFAULT 1,
    max_professionals INT NOT NULL DEFAULT 5,
    max_customers INT,
    allow_dedicated_database BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

-- Tabela de Tenants (Inquilinos)
CREATE TABLE IF NOT EXISTS public.tenants (
    tenant_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_name VARCHAR(255) NOT NULL,
    cnpj CHAR(14) UNIQUE,
    subdomain VARCHAR(100) NOT NULL UNIQUE CHECK (subdomain ~ '^[a-z0-9]+(?:-[a-z0-9]+)*$'),
    tenancy_model VARCHAR(50) NOT NULL CHECK (tenancy_model IN ('shared_schema', 'dedicated_database')) DEFAULT 'shared_schema',
    db_identifier VARCHAR(255) NOT NULL UNIQUE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE,
    created_by UUID,
    updated_by UUID
);

-- Tabela de Assinaturas
CREATE TABLE IF NOT EXISTS public.subscriptions (
    subscription_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL REFERENCES public.tenants(tenant_id),
    plan_id INT NOT NULL REFERENCES public.plans(plan_id),
    status VARCHAR(50) NOT NULL CHECK (status IN ('active', 'canceled', 'past_due', 'trial')) DEFAULT 'trial',
    start_date TIMESTAMP WITH TIME ZONE NOT NULL,
    end_date TIMESTAMP WITH TIME ZONE,
    trial_ends_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_by UUID
);