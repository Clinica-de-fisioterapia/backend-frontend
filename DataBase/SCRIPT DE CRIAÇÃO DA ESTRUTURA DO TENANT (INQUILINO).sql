-- =================================================================================
-- ARQUIVO: FUNÇÃO PARA CRIAR TENANTS.sql
-- OBJETIVO: Cria um schema completo para um novo tenant (multi-tenant por schema).
-- EXECUÇÃO: Execute UMA VEZ para registrar; depois chame: SELECT create_tenant_schema('empresa_teste');
-- PRÉ-REQUISITOS (em ESTRUTURA GLOBAL.sql):
--   - Enums: user_role, booking_status, schedule_status
--   - Funções globais: public.update_updated_at_column(), public.set_deleted_at_timestamp()
--   - Extensões: pgcrypto, btree_gist, pg_trgm
-- =================================================================================

CREATE OR REPLACE FUNCTION create_tenant_schema(tenant_subdomain TEXT)
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
    -- 0) Cria o schema do tenant (caso não exista)
    EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I', tenant_subdomain);

    -- 0.1) Garante que o search_path use o schema do tenant + public (funções globais)
    EXECUTE format('SET LOCAL search_path TO %I, public', tenant_subdomain);

    -- =================================================================================
    -- 1) USERS & AUTH
    -- =================================================================================
    -- users
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.users (
            id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            full_name       VARCHAR(255) NOT NULL,
            email           VARCHAR(255) NOT NULL UNIQUE,
            password_hash   VARCHAR(255) NOT NULL,
            role            user_role NOT NULL DEFAULT 'receptionist',
            is_active       BOOLEAN NOT NULL DEFAULT TRUE,
            created_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            row_version     BIGINT NOT NULL DEFAULT 1,
            deleted_at      TIMESTAMPTZ
        );
    $q$, tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_users_updated ON %I.users;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_users_updated BEFORE UPDATE ON %I.users FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_set_deleted_at_users ON %I.users;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_set_deleted_at_users BEFORE UPDATE OF deleted_at ON %I.users FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL) EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

    -- user_refresh_tokens
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.user_refresh_tokens (
            id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id       UUID NOT NULL REFERENCES %I.users(id) ON DELETE CASCADE,
            token_hash    VARCHAR(255) NOT NULL UNIQUE,
            expires_at    TIMESTAMPTZ NOT NULL,
            created_at    TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            revoked_at    TIMESTAMPTZ
        );
    $q$, tenant_subdomain, tenant_subdomain);

    -- =================================================================================
    -- 2) RBAC (Roles por Unidade)
    -- =================================================================================
    -- roles (tabela de papéis por tenant)
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.roles (
            id    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            name  VARCHAR(50) NOT NULL UNIQUE
        );
    $q$, tenant_subdomain);

    -- user_roles (vínculo usuário/role/unidade)
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.user_roles (
            id        UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id   UUID NOT NULL REFERENCES %I.users(id),
            role_id   UUID NOT NULL REFERENCES %I.roles(id),
            unit_id   UUID NOT NULL REFERENCES %I.units(id),
            UNIQUE (user_id, role_id, unit_id)
        );
    $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain);

    -- =================================================================================
    -- 3) PEOPLE / PROFESSIONALS / CUSTOMERS
    -- =================================================================================
    -- people
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.people (
            id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            full_name    VARCHAR(255) NOT NULL,
            cpf          VARCHAR(11) UNIQUE,
            phone        VARCHAR(20),
            email        VARCHAR(255) UNIQUE,
            created_at   TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at   TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            created_by   UUID REFERENCES %I.users(id),
            updated_by   UUID REFERENCES %I.users(id),
            deleted_at   TIMESTAMPTZ
        );
    $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_people_updated ON %I.people;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_people_updated BEFORE UPDATE ON %I.people FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_set_deleted_at_people ON %I.people;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_set_deleted_at_people BEFORE UPDATE OF deleted_at ON %I.people FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL) EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

    -- professionals
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.professionals (
            id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            person_id    UUID NOT NULL REFERENCES %I.people(id) UNIQUE,
            specialty    VARCHAR(255),
            created_at   TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at   TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            created_by   UUID REFERENCES %I.users(id),
            updated_by   UUID REFERENCES %I.users(id),
            deleted_at   TIMESTAMPTZ
        );
    $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_professionals_updated ON %I.professionals;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_professionals_updated BEFORE UPDATE ON %I.professionals FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_set_deleted_at_professionals ON %I.professionals;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_set_deleted_at_professionals BEFORE UPDATE OF deleted_at ON %I.professionals FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL) EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

    -- customers
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.customers (
            id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            person_id    UUID NOT NULL REFERENCES %I.people(id) UNIQUE,
            created_at   TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at   TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            created_by   UUID REFERENCES %I.users(id),
            updated_by   UUID REFERENCES %I.users(id),
            deleted_at   TIMESTAMPTZ
        );
    $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_customers_updated ON %I.customers;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_customers_updated BEFORE UPDATE ON %I.customers FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_set_deleted_at_customers ON %I.customers;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_set_deleted_at_customers BEFORE UPDATE OF deleted_at ON %I.customers FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL) EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

    -- =================================================================================
    -- 4) UNITS / SERVICES
    -- =================================================================================
    -- units
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.units (
            id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            name         VARCHAR(255) NOT NULL,
            created_at   TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at   TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            created_by   UUID REFERENCES %I.users(id),
            updated_by   UUID REFERENCES %I.users(id),
            deleted_at   TIMESTAMPTZ
        );
    $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_units_updated ON %I.units;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_units_updated BEFORE UPDATE ON %I.units FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_set_deleted_at_units ON %I.units;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_set_deleted_at_units BEFORE UPDATE OF deleted_at ON %I.units FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL) EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

    -- services
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.services (
            id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            name              VARCHAR(255) NOT NULL,
            duration_minutes  INT NOT NULL,
            price             NUMERIC(10,2) NOT NULL,
            created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            created_by        UUID REFERENCES %I.users(id),
            updated_by        UUID REFERENCES %I.users(id),
            deleted_at        TIMESTAMPTZ
        );
    $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_services_updated ON %I.services;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_services_updated BEFORE UPDATE ON %I.services FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_set_deleted_at_services ON %I.services;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_set_deleted_at_services BEFORE UPDATE OF deleted_at ON %I.services FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL) EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

    -- =================================================================================
    -- 5) SCHEDULES (AGENDA PADRÃO + EXCEÇÕES)
    -- =================================================================================
    -- professional_schedules (agenda recorrente por dia da semana)
    -- day_of_week: 0=Dom, 1=Seg, ... 6=Sáb
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.professional_schedules (
            id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            professional_id UUID NOT NULL REFERENCES %I.professionals(id),
            unit_id        UUID NOT NULL REFERENCES %I.units(id),
            day_of_week    SMALLINT NOT NULL CHECK (day_of_week BETWEEN 0 AND 6),
            start_time     TIME NOT NULL,
            end_time       TIME NOT NULL,
            status         schedule_status NOT NULL DEFAULT 'available',
            created_at     TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at     TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            created_by     UUID REFERENCES %I.users(id),
            updated_by     UUID REFERENCES %I.users(id),
            deleted_at     TIMESTAMPTZ,
            UNIQUE (professional_id, unit_id, day_of_week, start_time, end_time)
        );
    $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_professional_schedules_updated ON %I.professional_schedules;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_professional_schedules_updated BEFORE UPDATE ON %I.professional_schedules FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_set_deleted_at_professional_schedules ON %I.professional_schedules;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_set_deleted_at_professional_schedules BEFORE UPDATE OF deleted_at ON %I.professional_schedules FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL) EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

    -- schedule_exceptions (ex.: férias, feriados, bloqueios)
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.schedule_exceptions (
            id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            professional_id UUID NOT NULL REFERENCES %I.professionals(id),
            unit_id        UUID NOT NULL REFERENCES %I.units(id),
            date_from      TIMESTAMPTZ NOT NULL,
            date_to        TIMESTAMPTZ NOT NULL,
            status         schedule_status NOT NULL DEFAULT 'blocked',
            reason         VARCHAR(255),
            created_at     TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at     TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            created_by     UUID REFERENCES %I.users(id),
            updated_by     UUID REFERENCES %I.users(id),
            deleted_at     TIMESTAMPTZ
        );
    $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_schedule_exceptions_updated ON %I.schedule_exceptions;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_schedule_exceptions_updated BEFORE UPDATE ON %I.schedule_exceptions FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_set_deleted_at_schedule_exceptions ON %I.schedule_exceptions;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_set_deleted_at_schedule_exceptions BEFORE UPDATE OF deleted_at ON %I.schedule_exceptions FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL) EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

    -- =================================================================================
    -- 6) BOOKINGS (AGENDAMENTOS)
    -- =================================================================================
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.bookings (
            id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            professional_id UUID NOT NULL REFERENCES %I.professionals(id),
            customer_id     UUID NOT NULL REFERENCES %I.customers(id),
            service_id      UUID NOT NULL REFERENCES %I.services(id),
            unit_id         UUID NOT NULL REFERENCES %I.units(id),
            start_time      TIMESTAMPTZ NOT NULL,
            end_time        TIMESTAMPTZ NOT NULL,
            status          booking_status NOT NULL DEFAULT 'confirmed',
            row_version     BIGINT NOT NULL DEFAULT 1,
            created_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            created_by      UUID REFERENCES %I.users(id),
            updated_by      UUID REFERENCES %I.users(id),
            deleted_at      TIMESTAMPTZ,
            CONSTRAINT no_overlapping_bookings EXCLUDE USING GIST (
                professional_id WITH =,
                TSTZRANGE(start_time, end_time) WITH &&
            )
        );
    $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_bookings_updated ON %I.bookings;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_bookings_updated BEFORE UPDATE ON %I.bookings FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_set_deleted_at_bookings ON %I.bookings;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_set_deleted_at_bookings BEFORE UPDATE OF deleted_at ON %I.bookings FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL) EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);
    -- Índices úteis
    EXECUTE format('CREATE INDEX IF NOT EXISTS idx_bookings_professional_time ON %I.bookings (professional_id, start_time);', tenant_subdomain);
    EXECUTE format('CREATE INDEX IF NOT EXISTS idx_bookings_customer_time ON %I.bookings (customer_id, start_time);', tenant_subdomain);

    -- =================================================================================
    -- 7) TENANT SETTINGS (chave/valor)
    -- =================================================================================
    EXECUTE format($q$
        CREATE TABLE IF NOT EXISTS %I.tenant_settings (
            id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            key        VARCHAR(100) NOT NULL,
            value      TEXT,
            created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            CONSTRAINT uq_tenant_settings_key UNIQUE (key)
        );
    $q$, tenant_subdomain);
    EXECUTE format('DROP TRIGGER IF EXISTS trg_tenant_settings_updated ON %I.tenant_settings;', tenant_subdomain);
    EXECUTE format('CREATE TRIGGER trg_tenant_settings_updated BEFORE UPDATE ON %I.tenant_settings FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);

END;
$$;
