-- =====================================================================================
-- ESTRUTURA GLOBAL DO SISTEMA DE CLÍNICA DE FISIOTERAPIA
-- (executar apenas uma vez no banco principal)
-- =====================================================================================

-- -----------------------------------------------------------------------------
-- 1. Extensões necessárias
-- -----------------------------------------------------------------------------
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "btree_gist";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- -----------------------------------------------------------------------------
-- 2. ENUMs globais
-- -----------------------------------------------------------------------------
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'user_role') THEN
        CREATE TYPE user_role AS ENUM ('admin', 'professional', 'receptionist');
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'schedule_status') THEN
        CREATE TYPE schedule_status AS ENUM ('available', 'unavailable', 'blocked');
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'booking_status') THEN
        CREATE TYPE booking_status AS ENUM ('scheduled', 'confirmed', 'completed', 'cancelled', 'no_show');
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'setting_key') THEN
        CREATE TYPE setting_key AS ENUM ('timezone', 'work_hours', 'booking_policy', 'notifications', 'locale');
    END IF;
END$$;

-- -----------------------------------------------------------------------------
-- 3. Funções globais de auditoria e triggers
-- -----------------------------------------------------------------------------
-- Atualiza updated_at automaticamente
CREATE OR REPLACE FUNCTION public.update_updated_at_column()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;

-- Marca deleted_at automaticamente quando ocorre soft delete
CREATE OR REPLACE FUNCTION public.set_deleted_at_timestamp()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    IF NEW.deleted_at IS NULL AND OLD.deleted_at IS NULL THEN
        NEW.deleted_at := NOW();
    END IF;
    RETURN NEW;
END;
$$;

-- -----------------------------------------------------------------------------
-- 4. Estrutura global (schema public)
-- -----------------------------------------------------------------------------
-- Planos de assinatura
CREATE TABLE IF NOT EXISTS public.plans (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    price_monthly NUMERIC(10,2) NOT NULL,
    max_professionals INT NOT NULL DEFAULT 5,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tenants registrados globalmente
CREATE TABLE IF NOT EXISTS public.tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    subdomain VARCHAR(100) UNIQUE NOT NULL,
    plan_id UUID REFERENCES public.plans(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ
);

DROP TRIGGER IF EXISTS trg_tenants_updated ON public.tenants;
CREATE TRIGGER trg_tenants_updated
BEFORE UPDATE ON public.tenants
FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();

-- -----------------------------------------------------------------------------
-- 5. Aviso final
-- -----------------------------------------------------------------------------
-- ✅ Estrutura global criada.
-- Agora execute o script "FUNÇÃO_PARA_CRIAR_TENANTS.sql" e rode:
--     SELECT create_tenant_schema('empresa_teste');
-- para gerar o schema e tabelas do primeiro tenant.
-- -----------------------------------------------------------------------------
