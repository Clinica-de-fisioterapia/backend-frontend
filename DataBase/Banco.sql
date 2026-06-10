-- =====================================================================
-- CHRONOSYSTEM · BOOTSTRAP DO BANCO (V3.1)
-- Multi-tenant por schema, auditoria, row_version, CITEXT e índices.
-- =====================================================================

------------------------------------------------------------
-- 0) EXTENSÕES GLOBAIS
------------------------------------------------------------
CREATE EXTENSION IF NOT EXISTS pgcrypto;  -- gen_random_uuid()
CREATE EXTENSION IF NOT EXISTS citext;    -- case-insensitive (email/registry_code)

------------------------------------------------------------
-- 1) TIPOS E FUNÇÕES GLOBAIS
------------------------------------------------------------

-- 1.1) Enum global de status de agendamento (se ainda não existir)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type t WHERE t.typname = 'appointment_status') THEN
        CREATE TYPE public.appointment_status AS ENUM ('Scheduled','Completed','Cancelled','NoShow','Rescheduled');
    END IF;
END$$;

-- 1.2) Função global de auditoria (atualiza updated_at e incrementa row_version)
CREATE OR REPLACE FUNCTION public.fn_set_updated_at()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.updated_at := NOW();
    IF NEW.row_version IS NULL THEN
        NEW.row_version := 0;
    END IF;
    NEW.row_version := NEW.row_version + 1;
    RETURN NEW;
END;
$$;

-- 1.3) Validação de slug (schema) do tenant
CREATE OR REPLACE FUNCTION public.fn_validate_tenant_slug(p_slug TEXT)
RETURNS VOID
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_slug IS NULL OR btrim(p_slug) = '' THEN
        RAISE EXCEPTION 'Tenant slug vazio/NULL.' USING ERRCODE = '22023';
    END IF;

    -- normaliza
    p_slug := lower(btrim(p_slug));

    -- regex: começa com letra, pode conter letras/dígitos/_; 3..30 chars
    IF p_slug !~ '^[a-z][a-z0-9_]{2,30}$' THEN
        RAISE EXCEPTION 'Tenant slug inválido: "%" (esperado: ^[a-z][a-z0-9_]{2,30}$).', p_slug
            USING ERRCODE = '22023';
    END IF;
END;
$$;

------------------------------------------------------------
-- 2) TABELA GLOBAL DE TENANTS
------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.tenants (
    slug        TEXT PRIMARY KEY,                 -- usado como nome do schema
    name        TEXT NOT NULL,
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    row_version BIGINT      NOT NULL DEFAULT 0
);

-- Trigger de auditoria em public.tenants
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_trigger
        WHERE tgname = 'trg_public_tenants_set_updated_at'
          AND tgrelid = 'public.tenants'::regclass
    ) THEN
        CREATE TRIGGER trg_public_tenants_set_updated_at
        BEFORE UPDATE ON public.tenants
        FOR EACH ROW
        EXECUTE FUNCTION public.fn_set_updated_at();
    END IF;
END$$;

------------------------------------------------------------
-- 3) FUNÇÃO: CRIAR O SCHEMA DO TENANT E SUAS TABELAS/ÍNDICES/TRIGGERS
------------------------------------------------------------
CREATE OR REPLACE FUNCTION public.create_tenant_schema(p_slug TEXT)
RETURNS VOID
LANGUAGE plpgsql
AS $function$
DECLARE
    v_schema TEXT := lower(trim(p_slug));
BEGIN
    PERFORM public.fn_validate_tenant_slug(v_schema);

    -- cria o schema se não existir
    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = v_schema) THEN
        EXECUTE format('CREATE SCHEMA %I;', v_schema);
    END IF;

    -- ================================================================
    -- TABELAS DO TENANT
    -- ================================================================

    -- Unidades
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.units (
            id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            name         TEXT NOT NULL,
            code         TEXT,
            is_active    BOOLEAN NOT NULL DEFAULT TRUE,
            created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by   UUID,
            updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_by   UUID,
            deleted_at   TIMESTAMPTZ,
            row_version  BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema);

    EXECUTE format(
        'CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.units (lower(name));',
        v_schema||'_ux_units_name', v_schema
    );

    -- Users (role como TEXT flexível; e-mail CITEXT único)
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.users (
            id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            full_name     TEXT   NOT NULL,
            email         CITEXT NOT NULL,
            password_hash TEXT   NOT NULL,
            role          TEXT   NOT NULL CHECK (role ~ '^[A-Za-z][A-Za-z0-9_]{0,49}$'),
            is_active     BOOLEAN NOT NULL DEFAULT TRUE,
            created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by    UUID,
            updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_by    UUID,
            deleted_at    TIMESTAMPTZ,
            row_version   BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema);

    EXECUTE format(
        'CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.users (email);',
        v_schema||'_ux_users_email', v_schema
    );

    -- Professionals (1:1 com users) · registry_code = CITEXT + índice único parcial
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.professionals (
            id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id       UUID   NOT NULL REFERENCES %I.users(id) ON DELETE CASCADE,
            registry_code CITEXT,
            specialty     TEXT,
            created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by    UUID,
            updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_by    UUID,
            deleted_at    TIMESTAMPTZ,
            row_version   BIGINT NOT NULL DEFAULT 0,
            UNIQUE (user_id)
        );
    $fmt$, v_schema, v_schema);

    EXECUTE format(
        'CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.professionals (registry_code) WHERE registry_code IS NOT NULL;',
        v_schema||'_ux_professionals_registry_code', v_schema
    );

    -- Vínculo User x Unit
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.staff_units (
            staff_unit_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id       UUID NOT NULL REFERENCES %I.users(id)  ON DELETE CASCADE,
            unit_id       UUID NOT NULL REFERENCES %I.units(id)  ON DELETE CASCADE,
            role_in_unit  TEXT CHECK (role_in_unit IN ('admin','receptionist','professional')),
            created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by    UUID,
            updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_by    UUID,
            deleted_at    TIMESTAMPTZ,
            row_version   BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema, v_schema, v_schema);

    -- Evita duplicidade de vínculo por usuário/unidade
    EXECUTE format(
        'CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.staff_units (user_id, unit_id);',
        v_schema||'_ux_staff_units_user_unit', v_schema
    );

    -- Agenda padrão por profissional e unidade
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.professional_schedules (
            id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            professional_id UUID NOT NULL REFERENCES %I.professionals(id) ON DELETE CASCADE,
            unit_id         UUID NOT NULL REFERENCES %I.units(id)         ON DELETE CASCADE,
            weekday         SMALLINT NOT NULL CHECK (weekday BETWEEN 0 AND 6),
            start_time      TIME NOT NULL,
            end_time        TIME NOT NULL,
            slot_minutes    SMALLINT NOT NULL DEFAULT 30 CHECK (slot_minutes IN (15,20,25,30,40,45,50,60)),
            is_active       BOOLEAN NOT NULL DEFAULT TRUE,
            created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by      UUID,
            updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_by      UUID,
            deleted_at      TIMESTAMPTZ,
            row_version     BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema, v_schema, v_schema);

    EXECUTE format(
        'CREATE INDEX IF NOT EXISTS %I ON %I.professional_schedules (professional_id, unit_id, weekday);',
        v_schema||'_ix_sched_prof_unit_day', v_schema
    );

    -- Exceções de agenda
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.schedule_exceptions (
            id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            professional_id UUID NOT NULL REFERENCES %I.professionals(id) ON DELETE CASCADE,
            unit_id         UUID NOT NULL REFERENCES %I.units(id)         ON DELETE CASCADE,
            start_at        TIMESTAMPTZ NOT NULL,
            end_at          TIMESTAMPTZ NOT NULL,
            reason          TEXT,
            created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by      UUID,
            updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_by      UUID,
            deleted_at      TIMESTAMPTZ,
            row_version     BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema, v_schema, v_schema);

    EXECUTE format(
        'CREATE INDEX IF NOT EXISTS %I ON %I.schedule_exceptions (professional_id, unit_id, start_at, end_at);',
        v_schema||'_ix_sched_ex_prof_unit', v_schema
    );

    -- Pacientes
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.patients (
            id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            full_name    TEXT NOT NULL,
            email        CITEXT,
            phone        TEXT,
            birth_date   DATE,
            is_active    BOOLEAN NOT NULL DEFAULT TRUE,
            created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by   UUID,
            updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_by   UUID,
            deleted_at   TIMESTAMPTZ,
            row_version  BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema);

    EXECUTE format(
        'CREATE INDEX IF NOT EXISTS %I ON %I.patients (lower(full_name));',
        v_schema||'_ix_patients_name', v_schema
    );

    -- Agendamentos
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.bookings (
            id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            unit_id         UUID NOT NULL REFERENCES %I.units(id)           ON DELETE RESTRICT,
            professional_id UUID NOT NULL REFERENCES %I.professionals(id)   ON DELETE RESTRICT,
            patient_id      UUID NOT NULL REFERENCES %I.patients(id)        ON DELETE RESTRICT,
            start_at        TIMESTAMPTZ NOT NULL,
            end_at          TIMESTAMPTZ NOT NULL,
            status          public.appointment_status NOT NULL DEFAULT 'Scheduled',
            notes           TEXT,
            created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by      UUID,
            updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_by      UUID,
            deleted_at      TIMESTAMPTZ,        -- <--- adicionada para soft delete
            row_version     BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema, v_schema, v_schema, v_schema);

    EXECUTE format(
        'CREATE INDEX IF NOT EXISTS %I ON %I.bookings (unit_id, professional_id, start_at);',
        v_schema||'_ix_bookings_unit_prof_dt', v_schema
    );

    -- Tokens de refresh (hash único) + row_version
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.refresh_tokens (
            id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id        UUID NOT NULL REFERENCES %I.users(id) ON DELETE CASCADE,
            token_hash     TEXT NOT NULL,
            expires_at_utc TIMESTAMPTZ NOT NULL,
            is_revoked     BOOLEAN NOT NULL DEFAULT FALSE,
            created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            deleted_at     TIMESTAMPTZ,
            row_version    BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema, v_schema);

    EXECUTE format(
        'CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.refresh_tokens (token_hash);',
        v_schema||'_ux_refresh_token_hash', v_schema
    );

    -- Feriados por unidade
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.unit_holidays (
            id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            unit_id     UUID NOT NULL REFERENCES %I.units(id) ON DELETE CASCADE,
            holiday_on  DATE NOT NULL,
            description TEXT,
            created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by  UUID,
            updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_by  UUID,
            deleted_at  TIMESTAMPTZ,
            row_version BIGINT NOT NULL DEFAULT 0,
            UNIQUE (unit_id, holiday_on)
        );
    $fmt$, v_schema, v_schema);

    -- Settings (K/V)
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.settings (
            key        TEXT PRIMARY KEY,
            value      JSONB NOT NULL,
            created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
    $fmt$, v_schema);

    -- ================================================================
    -- GATILHOS DE AUDITORIA (updated_at + row_version) POR TABELA
    -- ================================================================

    -- helper para criar trigger condicionalmente
    PERFORM 1;

    -- users
    EXECUTE format($tg$
        DO $b$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger
                           WHERE tgname = 'trg_users_set_updated_at'
                             AND tgrelid = '%I.users'::regclass) THEN
                CREATE TRIGGER trg_users_set_updated_at
                BEFORE UPDATE ON %I.users
                FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$b$;
    $tg$, v_schema, v_schema);

    -- units
    EXECUTE format($tg$
        DO $b$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger
                           WHERE tgname = 'trg_units_set_updated_at'
                             AND tgrelid = '%I.units'::regclass) THEN
                CREATE TRIGGER trg_units_set_updated_at
                BEFORE UPDATE ON %I.units
                FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$b$;
    $tg$, v_schema, v_schema);

    -- professionals
    EXECUTE format($tg$
        DO $b$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger
                           WHERE tgname = 'trg_prof_set_updated_at'
                             AND tgrelid = '%I.professionals'::regclass) THEN
                CREATE TRIGGER trg_prof_set_updated_at
                BEFORE UPDATE ON %I.professionals
                FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$b$;
    $tg$, v_schema, v_schema);

    -- staff_units
    EXECUTE format($tg$
        DO $b$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger
                           WHERE tgname = 'trg_staff_units_set_updated_at'
                             AND tgrelid = '%I.staff_units'::regclass) THEN
                CREATE TRIGGER trg_staff_units_set_updated_at
                BEFORE UPDATE ON %I.staff_units
                FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$b$;
    $tg$, v_schema, v_schema);

    -- professional_schedules
    EXECUTE format($tg$
        DO $b$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger
                           WHERE tgname = 'trg_prof_sched_set_updated_at'
                             AND tgrelid = '%I.professional_schedules'::regclass) THEN
                CREATE TRIGGER trg_prof_sched_set_updated_at
                BEFORE UPDATE ON %I.professional_schedules
                FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$b$;
    $tg$, v_schema, v_schema);

    -- schedule_exceptions
    EXECUTE format($tg$
        DO $b$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger
                           WHERE tgname = 'trg_sched_exc_set_updated_at'
                             AND tgrelid = '%I.schedule_exceptions'::regclass) THEN
                CREATE TRIGGER trg_sched_exc_set_updated_at
                BEFORE UPDATE ON %I.schedule_exceptions
                FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$b$;
    $tg$, v_schema, v_schema);

    -- patients
    EXECUTE format($tg$
        DO $b$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger
                           WHERE tgname = 'trg_patients_set_updated_at'
                             AND tgrelid = '%I.patients'::regclass) THEN
                CREATE TRIGGER trg_patients_set_updated_at
                BEFORE UPDATE ON %I.patients
                FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$b$;
    $tg$, v_schema, v_schema);

    -- bookings
    EXECUTE format($tg$
        DO $b$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger
                           WHERE tgname = 'trg_bookings_set_updated_at'
                             AND tgrelid = '%I.bookings'::regclass) THEN
                CREATE TRIGGER trg_bookings_set_updated_at
                BEFORE UPDATE ON %I.bookings
                FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$b$;
    $tg$, v_schema, v_schema);

    -- unit_holidays
    EXECUTE format($tg$
        DO $b$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger
                           WHERE tgname = 'trg_unit_holidays_set_updated_at'
                             AND tgrelid = '%I.unit_holidays'::regclass) THEN
                CREATE TRIGGER trg_unit_holidays_set_updated_at
                BEFORE UPDATE ON %I.unit_holidays
                FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$b$;
    $tg$, v_schema, v_schema);

    -- refresh_tokens
    EXECUTE format($tg$
        DO $b$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger
                           WHERE tgname = 'trg_refresh_tokens_set_updated_at'
                             AND tgrelid = '%I.refresh_tokens'::regclass) THEN
                CREATE TRIGGER trg_refresh_tokens_set_updated_at
                BEFORE UPDATE ON %I.refresh_tokens
                FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$b$;
    $tg$, v_schema, v_schema);

    -- settings (apenas updated_at)
    EXECUTE format($fmt$
        CREATE OR REPLACE FUNCTION %I.fn_settings_touch()
        RETURNS TRIGGER LANGUAGE plpgsql AS $b$
        BEGIN
            NEW.updated_at := NOW();
            RETURN NEW;
        END$b$;

        DO $t$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger
                           WHERE tgname = 'trg_settings_touch'
                             AND tgrelid = '%I.settings'::regclass) THEN
                CREATE TRIGGER trg_settings_touch
                BEFORE UPDATE ON %I.settings
                FOR EACH ROW EXECUTE FUNCTION %I.fn_settings_touch();
            END IF;
        END$t$;
    $fmt$, v_schema, v_schema, v_schema, v_schema);

END;
$function$;

------------------------------------------------------------
-- 4) FUNÇÃO: DROPAR O SCHEMA DO TENANT (com CASCADE)
------------------------------------------------------------
CREATE OR REPLACE FUNCTION public.drop_tenant_schema(p_slug TEXT)
RETURNS VOID
LANGUAGE plpgsql
AS $$
DECLARE
    v_schema TEXT := lower(trim(p_slug));
BEGIN
    PERFORM public.fn_validate_tenant_slug(v_schema);

    IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = v_schema) THEN
        EXECUTE format('DROP SCHEMA %I CASCADE;', v_schema);
    END IF;
END;
$$;

------------------------------------------------------------
-- 5) FUNÇÃO: CRIAR TENANT (LINHA GLOBAL + SCHEMA)
------------------------------------------------------------
CREATE OR REPLACE FUNCTION public.create_tenant(p_slug TEXT, p_name TEXT)
RETURNS VOID
LANGUAGE plpgsql
AS $$
DECLARE
    v_schema TEXT := lower(trim(p_slug));
BEGIN
    PERFORM public.fn_validate_tenant_slug(v_schema);

    -- upsert simples em public.tenants
    INSERT INTO public.tenants (slug, name)
    VALUES (v_schema, p_name)
    ON CONFLICT (slug) DO UPDATE
        SET name = EXCLUDED.name;

    PERFORM public.create_tenant_schema(v_schema);
END;
$$;

------------------------------------------------------------
-- 6) FUNÇÃO: CRIAR TENANT + PRIMEIRO ADMIN (password já deve vir hash)
------------------------------------------------------------
CREATE OR REPLACE FUNCTION public.create_tenant_with_admin(
    p_slug           TEXT,
    p_name           TEXT,
    p_admin_name     TEXT,
    p_admin_email    CITEXT,
    p_admin_password_hash TEXT
)
RETURNS VOID
LANGUAGE plpgsql
AS $$
DECLARE
    v_schema TEXT := lower(trim(p_slug));
    v_admin  UUID;
BEGIN
    PERFORM public.create_tenant(v_slug := v_schema, p_name := p_name);

    -- cria admin dentro do schema do tenant (role = 'admin')
    EXECUTE format(
        $$INSERT INTO %I.users (full_name, email, password_hash, role, is_active)
          VALUES ($1, $2, $3, 'admin', TRUE)
          ON CONFLICT (email) DO NOTHING
          RETURNING id;$$,
        v_schema
    )
    INTO v_admin
    USING p_admin_name, p_admin_email, p_admin_password_hash;

    -- opcional: garantir que exista ao menos uma unidade "Matriz"
    EXECUTE format(
        $$INSERT INTO %I.units (name, code, is_active)
          VALUES ('Matriz', 'HQ', TRUE)
          ON CONFLICT DO NOTHING;$$,
        v_schema
    );
END;
$$;

-- =====================================================================
-- FIM · Para criar um tenant:
--   SELECT public.create_tenant_with_admin('empresateste','Empresa Teste','Admin','admin@empresateste.com','<hash>');
-- Ou:
--   SELECT public.create_tenant('empresateste','Empresa Teste');
-- =====================================================================
