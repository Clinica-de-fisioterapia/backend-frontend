/* =============================================================================
   FASE 1 — BANCO COMPLETO (VERSÃO FINAL)
   - Sem ENUMs (tudo TEXT)
   - Todas as tabelas mutáveis com row_version
   - Triggers V4 padrão (fn_set_updated_at)
   - refresh_tokens compatível com EF Core
   - create_tenant_schema idempotente
============================================================================= */


-- ============================================================================
-- 1) EXTENSÕES NECESSÁRIAS
-- ============================================================================
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "btree_gist";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";


-- ============================================================================
-- 2) FUNÇÃO GLOBAL — updated_at + row_version (PADRÃO V4)
-- ============================================================================
CREATE OR REPLACE FUNCTION public.fn_set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at := NOW();

  IF TG_OP = 'UPDATE' THEN
      NEW.row_version := COALESCE(OLD.row_version, 0) + 1;
  ELSIF TG_OP = 'INSERT' THEN
      NEW.row_version := COALESCE(NEW.row_version, 1);
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;


-- ============================================================================
-- 3) TABELAS GLOBAIS (PUBLIC)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.plans (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL UNIQUE,
    price_monthly NUMERIC(10,2) NOT NULL DEFAULT 0,
    max_professionals INT NOT NULL DEFAULT 5,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS public.tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL,
    subdomain TEXT UNIQUE NOT NULL,
    plan_id UUID REFERENCES public.plans(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ
);

DROP TRIGGER IF EXISTS trg_tenants_updated ON public.tenants;
CREATE TRIGGER trg_tenants_updated
BEFORE UPDATE ON public.tenants
FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();



-- ============================================================================
-- 4) FUNÇÃO create_tenant_schema (VERSÃO FINAL + V4)
-- ============================================================================
DROP FUNCTION IF EXISTS public.create_tenant_schema(text);

CREATE OR REPLACE FUNCTION public.create_tenant_schema(tenant_subdomain TEXT)
RETURNS VOID
LANGUAGE plpgsql
AS $$
DECLARE
    default_horizon INT := 30;
    default_ttl INT := 15;
BEGIN
    ----------------------------------------------------------------------
    -- CRIA SCHEMA
    ----------------------------------------------------------------------
    EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I', tenant_subdomain);
    EXECUTE format('SET LOCAL search_path TO %I, public', tenant_subdomain);

    -- Idempotência
    IF to_regclass('users') IS NOT NULL THEN
        RETURN;
    END IF;



    ----------------------------------------------------------------------
    -- USERS
    ----------------------------------------------------------------------
    CREATE TABLE users (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        full_name TEXT NOT NULL,
        email TEXT NOT NULL UNIQUE,
        password_hash TEXT NOT NULL,
        role TEXT NOT NULL DEFAULT 'receptionist',
        is_active BOOLEAN NOT NULL DEFAULT TRUE,

        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        row_version BIGINT NOT NULL DEFAULT 1,

        created_by UUID NULL,
        updated_by UUID NULL,
        deleted_at TIMESTAMPTZ
    );

    ALTER TABLE users
        ADD CONSTRAINT ck_users_role_allowed_values
        CHECK (role IN ('admin','professional','receptionist'));

    CREATE TRIGGER trg_users_updated
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();

    ALTER TABLE users
        ADD CONSTRAINT fk_users_created_by FOREIGN KEY(created_by)
        REFERENCES users(id) ON DELETE SET NULL;

    ALTER TABLE users
        ADD CONSTRAINT fk_users_updated_by FOREIGN KEY(updated_by)
        REFERENCES users(id) ON DELETE SET NULL;



    ----------------------------------------------------------------------
    -- REFRESH TOKENS (modelo EF)
    ----------------------------------------------------------------------
    CREATE TABLE refresh_tokens (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
        token_hash TEXT NOT NULL UNIQUE,

        expires_at_utc TIMESTAMPTZ NOT NULL,
        is_revoked BOOLEAN NOT NULL DEFAULT FALSE,
        deleted_at TIMESTAMPTZ,

        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        row_version BIGINT NOT NULL DEFAULT 1,

        created_by UUID REFERENCES users(id) ON DELETE SET NULL,
        updated_by UUID REFERENCES users(id) ON DELETE SET NULL
    );

    CREATE TRIGGER trg_refresh_tokens_updated
    BEFORE UPDATE ON refresh_tokens
    FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();



    ----------------------------------------------------------------------
    -- PEOPLE
    ----------------------------------------------------------------------
    CREATE TABLE people (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        full_name TEXT NOT NULL,

        cpf TEXT UNIQUE,
        phone TEXT,
        email TEXT UNIQUE,

        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        row_version BIGINT NOT NULL DEFAULT 1,

        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id),
        deleted_at TIMESTAMPTZ
    );

    CREATE TRIGGER trg_people_updated
    BEFORE UPDATE ON people
    FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();



    ----------------------------------------------------------------------
    -- PROFESSIONALS
    ----------------------------------------------------------------------
    CREATE TABLE professionals (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        person_id UUID NOT NULL REFERENCES people(id) UNIQUE,
        specialty TEXT,

        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        row_version BIGINT NOT NULL DEFAULT 1,

        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id),
        deleted_at TIMESTAMPTZ
    );

    CREATE TRIGGER trg_professionals_updated
    BEFORE UPDATE ON professionals
    FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();



    ----------------------------------------------------------------------
    -- CUSTOMERS
    ----------------------------------------------------------------------
    CREATE TABLE customers (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        person_id UUID NOT NULL REFERENCES people(id) UNIQUE,

        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        row_version BIGINT NOT NULL DEFAULT 1,

        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id),
        deleted_at TIMESTAMPTZ
    );

    CREATE TRIGGER trg_customers_updated
    BEFORE UPDATE ON customers
    FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();



    ----------------------------------------------------------------------
    -- UNITS
    ----------------------------------------------------------------------
    CREATE TABLE units (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        name TEXT NOT NULL,

        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        row_version BIGINT NOT NULL DEFAULT 1,

        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id),
        deleted_at TIMESTAMPTZ
    );

    CREATE TRIGGER trg_units_updated
    BEFORE UPDATE ON units
    FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();



    ----------------------------------------------------------------------
    -- SERVICES
    ----------------------------------------------------------------------
    CREATE TABLE services (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        name TEXT NOT NULL,
        duration_minutes INT NOT NULL,
        price NUMERIC(10,2) NOT NULL,

        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        row_version BIGINT NOT NULL DEFAULT 1,

        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id),
        deleted_at TIMESTAMPTZ
    );

    CREATE TRIGGER trg_services_updated
    BEFORE UPDATE ON services
    FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();



    ----------------------------------------------------------------------
    -- BOOKINGS
    ----------------------------------------------------------------------
    CREATE TABLE bookings (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

        professional_id UUID NOT NULL REFERENCES professionals(id),
        customer_id UUID NOT NULL REFERENCES customers(id),
        service_id UUID NOT NULL REFERENCES services(id),
        unit_id UUID NOT NULL REFERENCES units(id),

        start_time TIMESTAMPTZ NOT NULL,
        end_time   TIMESTAMPTZ NOT NULL,
        status TEXT NOT NULL DEFAULT 'confirmed',

        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        row_version BIGINT NOT NULL DEFAULT 1,

        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id),

        CONSTRAINT no_overlap EXCLUDE USING GIST (
            professional_id WITH =,
            TSTZRANGE(start_time, end_time) WITH &&
        )
    );

    CREATE TRIGGER trg_bookings_updated
    BEFORE UPDATE ON bookings
    FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();



    ----------------------------------------------------------------------
    -- SCHEDULE EXCEPTIONS
    ----------------------------------------------------------------------
    CREATE TABLE schedule_exceptions (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        unit_id UUID REFERENCES units(id),
        professional_id UUID REFERENCES professionals(id),

        title TEXT NOT NULL,
        start_time TIMESTAMPTZ NOT NULL,
        end_time   TIMESTAMPTZ NOT NULL,
        is_blocker BOOLEAN NOT NULL DEFAULT TRUE,

        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        row_version BIGINT NOT NULL DEFAULT 1,

        created_by UUID REFERENCES users(id),

        CONSTRAINT professional_or_unit_required
            CHECK (professional_id IS NOT NULL OR unit_id IS NOT NULL)
    );

    CREATE TRIGGER trg_schedule_exceptions_updated
    BEFORE UPDATE ON schedule_exceptions
    FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();



    ----------------------------------------------------------------------
    -- PROFESSIONAL SCHEDULES
    ----------------------------------------------------------------------
    CREATE TABLE professional_schedules (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

        professional_id UUID NOT NULL REFERENCES professionals(id),
        unit_id UUID NOT NULL REFERENCES units(id),

        start_time TIMESTAMPTZ NOT NULL,
        end_time   TIMESTAMPTZ NOT NULL,
        status TEXT NOT NULL DEFAULT 'available',

        booking_id UUID REFERENCES bookings(id),
        exception_id UUID REFERENCES schedule_exceptions(id),

        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        row_version BIGINT NOT NULL DEFAULT 1
    );

    CREATE TRIGGER trg_professional_schedules_updated
    BEFORE UPDATE ON professional_schedules
    FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();



    ----------------------------------------------------------------------
    -- TENANT SETTINGS
    ----------------------------------------------------------------------
    CREATE TABLE tenant_settings (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        key TEXT NOT NULL UNIQUE,
        value TEXT NOT NULL,

        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        row_version BIGINT NOT NULL DEFAULT 1,

        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id)
    );

    CREATE TRIGGER trg_tenant_settings_updated
    BEFORE UPDATE ON tenant_settings
    FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();

    INSERT INTO tenant_settings (key, value)
    VALUES
        ('booking_horizon_days', '30'),
        ('redis_ttl_seconds', '15'),
        ('week_start_day', 'monday'),
        ('timezone', 'America/Sao_Paulo');


END;
$$;



-- ============================================================================
-- 5) DADOS GLOBAIS INICIAIS (opcional)
-- ============================================================================
INSERT INTO public.plans (name, price_monthly, max_professionals, is_active)
VALUES ('Plano Básico', 0, 3, TRUE)
ON CONFLICT (name) DO NOTHING;

-- Para criar um tenant:
-- INSERT INTO public.tenants (name, subdomain, plan_id)
-- SELECT 'Empresa Teste', 'empresateste', id FROM public.plans WHERE name = 'Plano Básico';
-- SELECT public.create_tenant_schema('empresateste');


