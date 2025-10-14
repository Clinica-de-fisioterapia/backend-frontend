-- =================================================================================
-- ARQUIVO: ESTRUTURA GLOBAL.sql
-- OBJETIVO: Prepara o banco de dados com objetos compartilhados por todos os tenants.
-- EXECUÇÃO: Deve ser executado apenas UMA VEZ na configuração inicial do banco.
-- =================================================================================

-- 1. HABILITA EXTENSÕES GLOBAIS NECESSÁRIAS
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "btree_gist";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- 2. CRIA TIPOS ENUM GLOBAIS (se não existirem)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'user_role') THEN
        CREATE TYPE user_role AS ENUM ('admin', 'professional', 'receptionist');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'booking_status') THEN
        CREATE TYPE booking_status AS ENUM ('confirmed', 'canceled', 'completed', 'no_show');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'schedule_status') THEN
        CREATE TYPE schedule_status AS ENUM ('available', 'booked', 'blocked');
    END IF;
END$$;

-- 3. FUNÇÕES GLOBAIS DE AUDITORIA
CREATE OR REPLACE FUNCTION public.update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ✅ Função global para SOFT DELETE (usada por todos os tenants)
CREATE OR REPLACE FUNCTION public.set_deleted_at_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.deleted_at := NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 4. TABELAS GLOBAIS (schema 'public')

-- Tabela de Planos de Assinatura
CREATE TABLE IF NOT EXISTS public.plans (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    price_monthly NUMERIC(10, 2) NOT NULL,
    max_professionals INT NOT NULL DEFAULT 5,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela Mestra de Tenants (Inquilinos)
CREATE TABLE IF NOT EXISTS public.tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    subdomain VARCHAR(100) UNIQUE NOT NULL, -- Usaremos isso como nome do schema
    plan_id UUID REFERENCES public.plans(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ
);

DROP TRIGGER IF EXISTS trg_tenants_updated ON public.tenants;
CREATE TRIGGER trg_tenants_updated 
BEFORE UPDATE ON public.tenants 
FOR EACH ROW 
EXECUTE FUNCTION public.update_updated_at_column();

-- ========================== FIM DA ESTRUTURA GLOBAL ==========================
