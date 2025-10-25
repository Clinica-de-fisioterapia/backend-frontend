-- =====================================================================
-- Patch: ensure create_tenant_schema stores roles as TEXT and migrate
-- existing tenant schemas created before this update.
-- =====================================================================

CREATE OR REPLACE FUNCTION public.create_tenant_schema(p_slug TEXT)
RETURNS VOID
LANGUAGE plpgsql
AS $$
DECLARE
    v_schema TEXT := lower(trim(p_slug));
BEGIN
    PERFORM public.fn_validate_tenant_slug(v_schema);

    -- cria o schema
    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = v_schema) THEN
        EXECUTE format('CREATE SCHEMA %I;', v_schema);
    END IF;

    -- ================================================================
    -- TABELAS DO TENANT (todas dentro do schema v_schema)
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
            updated_at   TIMESTAMPTZ,
            updated_by   UUID,
            deleted_at   TIMESTAMPTZ,
            row_version  BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema);

    EXECUTE format('CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.units (lower(name));',
                   v_schema||'_ux_units_name', v_schema);

    -- Users  (role = TEXT, flexível para novos papéis)
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.users (
            id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            full_name     TEXT NOT NULL,
            email         CITEXT NOT NULL,
            password_hash TEXT NOT NULL,
            role          TEXT NOT NULL CHECK (role ~ '^[A-Za-z][A-Za-z0-9_]{0,49}$'),
            is_active     BOOLEAN NOT NULL DEFAULT TRUE,
            created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by    UUID,
            updated_at    TIMESTAMPTZ,
            updated_by    UUID,
            deleted_at    TIMESTAMPTZ,
            row_version   BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema);

    EXECUTE format('CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.users (email);',
                   v_schema||'_ux_users_email', v_schema);

    -- Professionals (1:1 com users)
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.professionals (
            id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id       UUID NOT NULL REFERENCES %I.users(id) ON DELETE CASCADE,
            registry_code TEXT,               -- ex: CREFITO
            specialty     TEXT,
            created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by    UUID,
            updated_at    TIMESTAMPTZ,
            updated_by    UUID,
            deleted_at    TIMESTAMPTZ,
            row_version   BIGINT NOT NULL DEFAULT 0,
            UNIQUE (user_id)
        );
    $fmt$, v_schema, v_schema);

    -- Vínculo explícito User x Unit (staff_units)
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.staff_units (
            staff_unit_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id       UUID NOT NULL REFERENCES %I.users(id) ON DELETE CASCADE,
            unit_id       UUID NOT NULL REFERENCES %I.units(id) ON DELETE CASCADE,
            role_in_unit  TEXT CHECK (role_in_unit IN ('admin','receptionist','professional')),
            created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by    UUID,
            UNIQUE (user_id, unit_id)
        );
    $fmt$, v_schema, v_schema, v_schema);

    -- Agenda padrão por profissional e unidade
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.professional_schedules (
            id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            professional_id UUID NOT NULL REFERENCES %I.professionals(id) ON DELETE CASCADE,
            unit_id         UUID NOT NULL REFERENCES %I.units(id) ON DELETE CASCADE,
            weekday         SMALLINT NOT NULL CHECK (weekday BETWEEN 0 AND 6), -- 0=Dom ... 6=Sáb
            start_time      TIME NOT NULL,
            end_time        TIME NOT NULL,
            slot_minutes    SMALLINT NOT NULL DEFAULT 30 CHECK (slot_minutes IN (15,20,25,30,40,45,50,60)),
            is_active       BOOLEAN NOT NULL DEFAULT TRUE,
            created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by      UUID,
            updated_at      TIMESTAMPTZ,
            updated_by      UUID,
            deleted_at      TIMESTAMPTZ,
            row_version     BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema, v_schema, v_schema);

    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON %I.professional_schedules (professional_id, unit_id, weekday);',
                   v_schema||'_ix_sched_prof_unit_day', v_schema);

    -- Exceções de agenda (folgas, feriados específicos do profissional/unidade)
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.schedule_exceptions (
            id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            professional_id UUID NOT NULL REFERENCES %I.professionals(id) ON DELETE CASCADE,
            unit_id         UUID NOT NULL REFERENCES %I.units(id) ON DELETE CASCADE,
            start_at        TIMESTAMPTZ NOT NULL,
            end_at          TIMESTAMPTZ NOT NULL,
            reason          TEXT,
            created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by      UUID,
            updated_at      TIMESTAMPTZ,
            updated_by      UUID,
            deleted_at      TIMESTAMPTZ,
            row_version     BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema, v_schema, v_schema);

    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON %I.schedule_exceptions (professional_id, unit_id, start_at, end_at);',
                   v_schema||'_ix_sched_ex_prof_unit', v_schema);

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
            updated_at   TIMESTAMPTZ,
            updated_by   UUID,
            deleted_at   TIMESTAMPTZ,
            row_version  BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema);

    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON %I.patients (lower(full_name));',
                   v_schema||'_ix_patients_name', v_schema);

    -- Agendamentos
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.bookings (
            id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            unit_id         UUID NOT NULL REFERENCES %I.units(id) ON DELETE RESTRICT,
            professional_id UUID NOT NULL REFERENCES %I.professionals(id) ON DELETE RESTRICT,
            patient_id      UUID NOT NULL REFERENCES %I.patients(id) ON DELETE RESTRICT,
            start_at        TIMESTAMPTZ NOT NULL,
            end_at          TIMESTAMPTZ NOT NULL,
            status          public.appointment_status NOT NULL DEFAULT 'Scheduled',
            notes           TEXT,
            created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by      UUID,
            updated_at      TIMESTAMPTZ,
            updated_by      UUID,
            deleted_at      TIMESTAMPTZ,
            row_version     BIGINT NOT NULL DEFAULT 0
        );
    $fmt$, v_schema, v_schema, v_schema, v_schema);

    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON %I.bookings (unit_id, professional_id, start_at);',
                   v_schema||'_ix_bookings_unit_prof_dt', v_schema);

    -- Refresh tokens (armazenar HASH do token)
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.refresh_tokens (
            id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            user_id        UUID NOT NULL REFERENCES %I.users(id) ON DELETE CASCADE,
            token_hash     TEXT NOT NULL,
            expires_at_utc TIMESTAMPTZ NOT NULL,
            is_revoked     BOOLEAN NOT NULL DEFAULT FALSE,
            created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_at     TIMESTAMPTZ,
            deleted_at     TIMESTAMPTZ
        );
    $fmt$, v_schema, v_schema);

    EXECUTE format('CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.refresh_tokens (token_hash);',
                   v_schema||'_ux_refresh_token_hash', v_schema);

    -- Feriados por unidade (para cálculo de disponibilidade)
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.unit_holidays (
            id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            unit_id     UUID NOT NULL REFERENCES %I.units(id) ON DELETE CASCADE,
            holiday_on  DATE NOT NULL,
            description TEXT,
            created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            created_by  UUID,
            updated_at  TIMESTAMPTZ,
            updated_by  UUID,
            deleted_at  TIMESTAMPTZ,
            row_version BIGINT NOT NULL DEFAULT 0,
            UNIQUE (unit_id, holiday_on)
        );
    $fmt$, v_schema, v_schema);

    -- Settings por tenant (chave/valor)
    EXECUTE format($fmt$
        CREATE TABLE IF NOT EXISTS %I.settings (
            key          TEXT PRIMARY KEY,
            value        JSONB NOT NULL,
            created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_at   TIMESTAMPTZ
        );
    $fmt$, v_schema);

    -- ================================================================
    -- GATILHOS DE AUDITORIA (updated_at + row_version)
    -- ================================================================
    -- users
    EXECUTE format($fmt$
        DO $tg$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger 
                           WHERE tgname = 'trg_users_set_updated_at' AND tgrelid = '%I.users'::regclass) THEN
                CREATE TRIGGER trg_users_set_updated_at
                BEFORE UPDATE ON %I.users
                FOR EACH ROW
                EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$tg$;
    $fmt$, v_schema, v_schema);

    -- units
    EXECUTE format($fmt$
        DO $tg$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger 
                           WHERE tgname = 'trg_units_set_updated_at' AND tgrelid = '%I.units'::regclass) THEN
                CREATE TRIGGER trg_units_set_updated_at
                BEFORE UPDATE ON %I.units
                FOR EACH ROW
                EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$tg$;
    $fmt$, v_schema, v_schema);

    -- professionals
    EXECUTE format($fmt$
        DO $tg$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger 
                           WHERE tgname = 'trg_prof_set_updated_at' AND tgrelid = '%I.professionals'::regclass) THEN
                CREATE TRIGGER trg_prof_set_updated_at
                BEFORE UPDATE ON %I.professionals
                FOR EACH ROW
                EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$tg$;
    $fmt$, v_schema, v_schema);

    -- staff_units
    EXECUTE format($fmt$
        DO $tg$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger 
                           WHERE tgname = 'trg_staff_units_set_updated_at' AND tgrelid = '%I.staff_units'::regclass) THEN
                CREATE TRIGGER trg_staff_units_set_updated_at
                BEFORE UPDATE ON %I.staff_units
                FOR EACH ROW
                EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$tg$;
    $fmt$, v_schema, v_schema);

    -- professional_schedules
    EXECUTE format($fmt$
        DO $tg$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger 
                           WHERE tgname = 'trg_prof_sched_set_updated_at' AND tgrelid = '%I.professional_schedules'::regclass) THEN
                CREATE TRIGGER trg_prof_sched_set_updated_at
                BEFORE UPDATE ON %I.professional_schedules
                FOR EACH ROW
                EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$tg$;
    $fmt$, v_schema, v_schema);

    -- schedule_exceptions
    EXECUTE format($fmt$
        DO $tg$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger 
                           WHERE tgname = 'trg_sched_exc_set_updated_at' AND tgrelid = '%I.schedule_exceptions'::regclass) THEN
                CREATE TRIGGER trg_sched_exc_set_updated_at
                BEFORE UPDATE ON %I.schedule_exceptions
                FOR EACH ROW
                EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$tg$;
    $fmt$, v_schema, v_schema);

    -- patients
    EXECUTE format($fmt$
        DO $tg$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger 
                           WHERE tgname = 'trg_patients_set_updated_at' AND tgrelid = '%I.patients'::regclass) THEN
                CREATE TRIGGER trg_patients_set_updated_at
                BEFORE UPDATE ON %I.patients
                FOR EACH ROW
                EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$tg$;
    $fmt$, v_schema, v_schema);

    -- bookings
    EXECUTE format($fmt$
        DO $tg$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger 
                           WHERE tgname = 'trg_bookings_set_updated_at' AND tgrelid = '%I.bookings'::regclass) THEN
                CREATE TRIGGER trg_bookings_set_updated_at
                BEFORE UPDATE ON %I.bookings
                FOR EACH ROW
                EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$tg$;
    $fmt$, v_schema, v_schema);

    -- unit_holidays
    EXECUTE format($fmt$
        DO $tg$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger 
                           WHERE tgname = 'trg_unit_holidays_set_updated_at' AND tgrelid = '%I.unit_holidays'::regclass) THEN
                CREATE TRIGGER trg_unit_holidays_set_updated_at
                BEFORE UPDATE ON %I.unit_holidays
                FOR EACH ROW
                EXECUTE FUNCTION public.fn_set_updated_at();
            END IF;
        END$tg$;
    $fmt$, v_schema, v_schema);

    -- settings (apenas updated_at)
    EXECUTE format($fmt$
        CREATE OR REPLACE FUNCTION %I.fn_settings_touch()
        RETURNS TRIGGER LANGUAGE plpgsql AS $b$
        BEGIN
            NEW.updated_at := NOW();
            RETURN NEW;
        END$b$;

        DO $tg$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_trigger 
                           WHERE tgname = 'trg_settings_touch' AND tgrelid = '%I.settings'::regclass) THEN
                CREATE TRIGGER trg_settings_touch
                BEFORE UPDATE ON %I.settings
                FOR EACH ROW
                EXECUTE FUNCTION %I.fn_settings_touch();
            END IF;
        END$tg$;
    $fmt$, v_schema, v_schema, v_schema, v_schema);

END;
$$;

-- Migração: converte role ENUM -> TEXT e força citext em e-mail
DO $$
DECLARE
    rec RECORD;
BEGIN
    FOR rec IN SELECT slug FROM public.tenants LOOP
        BEGIN
            EXECUTE format('ALTER TABLE %I.users ALTER COLUMN role TYPE TEXT USING role::text;', rec.slug);
        EXCEPTION WHEN undefined_column OR undefined_function THEN
            NULL;
        END;

        BEGIN
            EXECUTE format('ALTER TABLE %I.users ALTER COLUMN email TYPE CITEXT USING email::citext;', rec.slug);
        EXCEPTION WHEN undefined_column THEN
            NULL;
        END;

        EXECUTE format('CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.users (email);',
                       rec.slug||'_ux_users_email', rec.slug);
    END LOOP;
END;
$$;
