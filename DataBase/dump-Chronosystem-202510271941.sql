--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.4

-- Started on 2025-10-27 19:41:27

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

DROP DATABASE "Chronosystem";
--
-- TOC entry 5103 (class 1262 OID 27058)
-- Name: Chronosystem; Type: DATABASE; Schema: -; Owner: -
--

CREATE DATABASE "Chronosystem" WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'pt-BR';


\connect "Chronosystem"

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 10 (class 2615 OID 27654)
-- Name: empresateste; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA empresateste;


--
-- TOC entry 3 (class 3079 OID 27096)
-- Name: citext; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS citext WITH SCHEMA public;


--
-- TOC entry 5104 (class 0 OID 0)
-- Dependencies: 3
-- Name: EXTENSION citext; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION citext IS 'data type for case-insensitive character strings';


--
-- TOC entry 2 (class 3079 OID 27059)
-- Name: pgcrypto; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;


--
-- TOC entry 5105 (class 0 OID 0)
-- Dependencies: 2
-- Name: EXTENSION pgcrypto; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';


--
-- TOC entry 953 (class 1247 OID 27202)
-- Name: appointment_status; Type: TYPE; Schema: public; Owner: -
--

CREATE TYPE public.appointment_status AS ENUM (
    'Scheduled',
    'CheckedIn',
    'Completed',
    'Cancelled',
    'NoShow'
);


--
-- TOC entry 320 (class 1255 OID 27861)
-- Name: fn_settings_touch(); Type: FUNCTION; Schema: empresateste; Owner: -
--

CREATE FUNCTION empresateste.fn_settings_touch() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
        BEGIN
            NEW.updated_at := NOW();
            RETURN NEW;
        END$$;


--
-- TOC entry 307 (class 1255 OID 27229)
-- Name: create_tenant_schema(text); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.create_tenant_schema(p_slug text) RETURNS void
    LANGUAGE plpgsql
    AS $_$
DECLARE
    v_schema TEXT := lower(trim(p_slug));
BEGIN
    PERFORM public.fn_validate_tenant_slug(v_schema);

    -- cria o schema
    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = v_schema) THEN
        EXECUTE format('CREATE SCHEMA %I;', v_schema);
    END IF;

    -- ================================================================
    -- TABELAS DO TENANT (dentro de v_schema)
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

    EXECUTE format('CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.units (lower(name));',
                   v_schema||'_ux_units_name', v_schema);

    -- Users  (role = TEXT, flexível)
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
            updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
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
            updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
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
            updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_by    UUID,
            deleted_at    TIMESTAMPTZ
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
            updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
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
            updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
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
            updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
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
            updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
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
            updated_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
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
            updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
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
            updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
    $fmt$, v_schema);

    -- ================================================================
    -- GATILHOS DE AUDITORIA (updated_at + row_version) POR TABELA
    -- ================================================================
    -- users
    EXECUTE format($fmt$
        DO $tg$
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM pg_trigger
                WHERE tgname = 'trg_users_set_updated_at'
                  AND tgrelid = '%I.users'::regclass
            ) THEN
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
            IF NOT EXISTS (
                SELECT 1 FROM pg_trigger
                WHERE tgname = 'trg_units_set_updated_at'
                  AND tgrelid = '%I.units'::regclass
            ) THEN
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
            IF NOT EXISTS (
                SELECT 1 FROM pg_trigger
                WHERE tgname = 'trg_prof_set_updated_at'
                  AND tgrelid = '%I.professionals'::regclass
            ) THEN
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
            IF NOT EXISTS (
                SELECT 1 FROM pg_trigger
                WHERE tgname = 'trg_staff_units_set_updated_at'
                  AND tgrelid = '%I.staff_units'::regclass
            ) THEN
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
            IF NOT EXISTS (
                SELECT 1 FROM pg_trigger
                WHERE tgname = 'trg_prof_sched_set_updated_at'
                  AND tgrelid = '%I.professional_schedules'::regclass
            ) THEN
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
            IF NOT EXISTS (
                SELECT 1 FROM pg_trigger
                WHERE tgname = 'trg_sched_exc_set_updated_at'
                  AND tgrelid = '%I.schedule_exceptions'::regclass
            ) THEN
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
            IF NOT EXISTS (
                SELECT 1 FROM pg_trigger
                WHERE tgname = 'trg_patients_set_updated_at'
                  AND tgrelid = '%I.patients'::regclass
            ) THEN
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
            IF NOT EXISTS (
                SELECT 1 FROM pg_trigger
                WHERE tgname = 'trg_bookings_set_updated_at'
                  AND tgrelid = '%I.bookings'::regclass
            ) THEN
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
            IF NOT EXISTS (
                SELECT 1 FROM pg_trigger
                WHERE tgname = 'trg_unit_holidays_set_updated_at'
                  AND tgrelid = '%I.unit_holidays'::regclass
            ) THEN
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
            IF NOT EXISTS (
                SELECT 1 FROM pg_trigger
                WHERE tgname = 'trg_settings_touch'
                  AND tgrelid = '%I.settings'::regclass
            ) THEN
                CREATE TRIGGER trg_settings_touch
                BEFORE UPDATE ON %I.settings
                FOR EACH ROW
                EXECUTE FUNCTION %I.fn_settings_touch();
            END IF;
        END$tg$;
    $fmt$, v_schema, v_schema, v_schema, v_schema);

END;
$_$;


--
-- TOC entry 239 (class 1255 OID 27228)
-- Name: drop_tenant_schema(text); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.drop_tenant_schema(p_slug text) RETURNS void
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


--
-- TOC entry 254 (class 1255 OID 27652)
-- Name: ensure_audit_on_schema(text); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.ensure_audit_on_schema(p_schema text) RETURNS void
    LANGUAGE plpgsql
    AS $_$
DECLARE
    v_tbl TEXT;
BEGIN
    IF p_schema IS NULL OR btrim(p_schema) = '' THEN
        RAISE EXCEPTION 'schema name is required';
    END IF;

    FOR v_tbl IN
        SELECT c.table_name
        FROM information_schema.columns c
        WHERE c.table_schema = p_schema
          AND c.column_name IN ('created_at','updated_at')
        GROUP BY c.table_name
        HAVING COUNT(*) FILTER (WHERE c.column_name = 'created_at') > 0
           AND COUNT(*) FILTER (WHERE c.column_name = 'updated_at') > 0
    LOOP
        -- DEFAULTs de criação
        EXECUTE format(
            'ALTER TABLE %I.%I
               ALTER COLUMN created_at SET DEFAULT now(),
               ALTER COLUMN updated_at SET DEFAULT now()',
            p_schema, v_tbl
        );

        -- Trigger BEFORE UPDATE (idempotente)
        EXECUTE format($fmt$
            DO $do$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM pg_trigger t
                    JOIN pg_class   r ON r.oid = t.tgrelid
                    JOIN pg_namespace n ON n.oid = r.relnamespace
                    WHERE n.nspname = %L
                      AND r.relname  = %L
                      AND t.tgname   = %L
                ) THEN
                    EXECUTE format(
                        'CREATE TRIGGER %I
                           BEFORE UPDATE ON %I.%I
                           FOR EACH ROW
                           EXECUTE FUNCTION public.fn_set_updated_at()',
                        'trg_' || %L || '_set_updated_at',
                        %I, %I,
                        %I
                    );
                END IF;
            END
            $do$;
        $fmt$, p_schema, v_tbl, 'trg_' || v_tbl || '_set_updated_at',
              'trg_' || v_tbl || '_set_updated_at', p_schema, v_tbl, p_schema,
              v_tbl);
    END LOOP;
END;
$_$;


--
-- TOC entry 236 (class 1255 OID 27653)
-- Name: fix_infinity_timestamps(text); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.fix_infinity_timestamps(p_schema text) RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_tbl TEXT;
BEGIN
    FOR v_tbl IN
        SELECT c.table_name
        FROM information_schema.columns c
        WHERE c.table_schema = p_schema
          AND c.column_name IN ('created_at','updated_at')
        GROUP BY c.table_name
        HAVING COUNT(*) FILTER (WHERE c.column_name = 'created_at') > 0
           AND COUNT(*) FILTER (WHERE c.column_name = 'updated_at') > 0
    LOOP
        EXECUTE format(
            'UPDATE %I.%I
               SET created_at = NOW()
             WHERE created_at = ''-infinity''::timestamptz;',
            p_schema, v_tbl
        );
        EXECUTE format(
            'UPDATE %I.%I
               SET updated_at = NOW()
             WHERE updated_at = ''-infinity''::timestamptz;',
            p_schema, v_tbl
        );
    END LOOP;
END;
$$;


--
-- TOC entry 269 (class 1255 OID 27213)
-- Name: fn_set_updated_at(); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.fn_set_updated_at() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
  NEW.updated_at := now();

  -- Se a tabela tiver row_version, incrementa (0 -> 1 -> 2 -> ...)
  IF EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema = TG_TABLE_SCHEMA
      AND table_name   = TG_TABLE_NAME
      AND column_name  = 'row_version'
  ) THEN
    NEW.row_version := COALESCE(OLD.row_version, 0) + 1;
  END IF;

  RETURN NEW;
END;
$$;


--
-- TOC entry 264 (class 1255 OID 27227)
-- Name: fn_validate_tenant_slug(text); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.fn_validate_tenant_slug(p_slug text) RETURNS void
    LANGUAGE plpgsql
    AS $_$
BEGIN
    IF p_slug IS NULL OR length(trim(p_slug)) = 0 THEN
        RAISE EXCEPTION 'Tenant slug inválido (vazio).';
    END IF;

    IF p_slug !~ '^[a-z0-9]([a-z0-9\-]*[a-z0-9])?$' THEN
        RAISE EXCEPTION
            'Tenant slug inválido. Use: [a-z0-9-], sem espaços, não começar/terminar com hífen.';
    END IF;
END;
$_$;


--
-- TOC entry 325 (class 1255 OID 27867)
-- Name: update_updated_at_column(); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.update_updated_at_column() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
  NEW.updated_at := now();

  IF EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema = TG_TABLE_SCHEMA
      AND table_name   = TG_TABLE_NAME
      AND column_name  = 'row_version'
  ) THEN
    NEW.row_version := COALESCE(OLD.row_version, 0) + 1;
  END IF;

  RETURN NEW;
END;
$$;


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 230 (class 1259 OID 27780)
-- Name: bookings; Type: TABLE; Schema: empresateste; Owner: -
--

CREATE TABLE empresateste.bookings (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    unit_id uuid NOT NULL,
    professional_id uuid NOT NULL,
    patient_id uuid NOT NULL,
    start_at timestamp with time zone NOT NULL,
    end_at timestamp with time zone NOT NULL,
    status public.appointment_status DEFAULT 'Scheduled'::public.appointment_status NOT NULL,
    notes text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by uuid,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_by uuid,
    deleted_at timestamp with time zone,
    row_version bigint DEFAULT 0 NOT NULL
);


--
-- TOC entry 229 (class 1259 OID 27767)
-- Name: patients; Type: TABLE; Schema: empresateste; Owner: -
--

CREATE TABLE empresateste.patients (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    full_name text NOT NULL,
    email public.citext,
    phone text,
    birth_date date,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by uuid,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_by uuid,
    deleted_at timestamp with time zone,
    row_version bigint DEFAULT 0 NOT NULL
);


--
-- TOC entry 227 (class 1259 OID 27721)
-- Name: professional_schedules; Type: TABLE; Schema: empresateste; Owner: -
--

CREATE TABLE empresateste.professional_schedules (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    professional_id uuid NOT NULL,
    unit_id uuid NOT NULL,
    weekday smallint NOT NULL,
    start_time time without time zone NOT NULL,
    end_time time without time zone NOT NULL,
    slot_minutes smallint DEFAULT 30 NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by uuid,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_by uuid,
    deleted_at timestamp with time zone,
    row_version bigint DEFAULT 0 NOT NULL,
    CONSTRAINT professional_schedules_slot_minutes_check CHECK ((slot_minutes = ANY (ARRAY[15, 20, 25, 30, 40, 45, 50, 60]))),
    CONSTRAINT professional_schedules_weekday_check CHECK (((weekday >= 0) AND (weekday <= 6)))
);


--
-- TOC entry 225 (class 1259 OID 27682)
-- Name: professionals; Type: TABLE; Schema: empresateste; Owner: -
--

CREATE TABLE empresateste.professionals (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid NOT NULL,
    registry_code text,
    specialty text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by uuid,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_by uuid,
    deleted_at timestamp with time zone,
    row_version bigint DEFAULT 0 NOT NULL
);


--
-- TOC entry 231 (class 1259 OID 27808)
-- Name: refresh_tokens; Type: TABLE; Schema: empresateste; Owner: -
--

CREATE TABLE empresateste.refresh_tokens (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid NOT NULL,
    token_hash text NOT NULL,
    expires_at_utc timestamp with time zone NOT NULL,
    is_revoked boolean DEFAULT false NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    deleted_at timestamp with time zone,
    row_version bigint DEFAULT 0 NOT NULL
);


--
-- TOC entry 228 (class 1259 OID 27745)
-- Name: schedule_exceptions; Type: TABLE; Schema: empresateste; Owner: -
--

CREATE TABLE empresateste.schedule_exceptions (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    professional_id uuid NOT NULL,
    unit_id uuid NOT NULL,
    start_at timestamp with time zone NOT NULL,
    end_at timestamp with time zone NOT NULL,
    reason text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by uuid,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_by uuid,
    deleted_at timestamp with time zone,
    row_version bigint DEFAULT 0 NOT NULL
);


--
-- TOC entry 233 (class 1259 OID 27843)
-- Name: settings; Type: TABLE; Schema: empresateste; Owner: -
--

CREATE TABLE empresateste.settings (
    key text NOT NULL,
    value jsonb NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


--
-- TOC entry 226 (class 1259 OID 27700)
-- Name: staff_units; Type: TABLE; Schema: empresateste; Owner: -
--

CREATE TABLE empresateste.staff_units (
    staff_unit_id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid NOT NULL,
    unit_id uuid NOT NULL,
    role_in_unit text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by uuid,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_by uuid,
    deleted_at timestamp with time zone,
    CONSTRAINT staff_units_role_in_unit_check CHECK ((role_in_unit = ANY (ARRAY['admin'::text, 'receptionist'::text, 'professional'::text])))
);


--
-- TOC entry 232 (class 1259 OID 27825)
-- Name: unit_holidays; Type: TABLE; Schema: empresateste; Owner: -
--

CREATE TABLE empresateste.unit_holidays (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    unit_id uuid NOT NULL,
    holiday_on date NOT NULL,
    description text,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by uuid,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_by uuid,
    deleted_at timestamp with time zone,
    row_version bigint DEFAULT 0 NOT NULL
);


--
-- TOC entry 223 (class 1259 OID 27655)
-- Name: units; Type: TABLE; Schema: empresateste; Owner: -
--

CREATE TABLE empresateste.units (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    name text NOT NULL,
    code text,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by uuid,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_by uuid,
    deleted_at timestamp with time zone,
    row_version bigint DEFAULT 0 NOT NULL
);


--
-- TOC entry 224 (class 1259 OID 27668)
-- Name: users; Type: TABLE; Schema: empresateste; Owner: -
--

CREATE TABLE empresateste.users (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    full_name text NOT NULL,
    email public.citext NOT NULL,
    password_hash text NOT NULL,
    role text NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    created_by uuid,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_by uuid,
    deleted_at timestamp with time zone,
    row_version bigint DEFAULT 0 NOT NULL,
    CONSTRAINT users_role_check CHECK ((role ~ '^[A-Za-z][A-Za-z0-9_]{0,49}$'::text))
);


--
-- TOC entry 222 (class 1259 OID 27214)
-- Name: tenants; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.tenants (
    tenant_id uuid DEFAULT gen_random_uuid() NOT NULL,
    slug text NOT NULL,
    name text NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone
);


--
-- TOC entry 5094 (class 0 OID 27780)
-- Dependencies: 230
-- Data for Name: bookings; Type: TABLE DATA; Schema: empresateste; Owner: -
--

COPY empresateste.bookings (id, unit_id, professional_id, patient_id, start_at, end_at, status, notes, created_at, created_by, updated_at, updated_by, deleted_at, row_version) FROM stdin;
\.


--
-- TOC entry 5093 (class 0 OID 27767)
-- Dependencies: 229
-- Data for Name: patients; Type: TABLE DATA; Schema: empresateste; Owner: -
--

COPY empresateste.patients (id, full_name, email, phone, birth_date, is_active, created_at, created_by, updated_at, updated_by, deleted_at, row_version) FROM stdin;
\.


--
-- TOC entry 5091 (class 0 OID 27721)
-- Dependencies: 227
-- Data for Name: professional_schedules; Type: TABLE DATA; Schema: empresateste; Owner: -
--

COPY empresateste.professional_schedules (id, professional_id, unit_id, weekday, start_time, end_time, slot_minutes, is_active, created_at, created_by, updated_at, updated_by, deleted_at, row_version) FROM stdin;
\.


--
-- TOC entry 5089 (class 0 OID 27682)
-- Dependencies: 225
-- Data for Name: professionals; Type: TABLE DATA; Schema: empresateste; Owner: -
--

COPY empresateste.professionals (id, user_id, registry_code, specialty, created_at, created_by, updated_at, updated_by, deleted_at, row_version) FROM stdin;
\.


--
-- TOC entry 5095 (class 0 OID 27808)
-- Dependencies: 231
-- Data for Name: refresh_tokens; Type: TABLE DATA; Schema: empresateste; Owner: -
--

COPY empresateste.refresh_tokens (id, user_id, token_hash, expires_at_utc, is_revoked, created_at, updated_at, deleted_at, row_version) FROM stdin;
aaba8a4f-2425-476c-9d6d-671e22cc712f	60768bfc-9f81-44fc-a868-b8a74dfbacd2	uIapLO+4zF03Zyz+5VSWTruap25w3hyfVs35TsNCcSE=	2025-11-01 15:43:48.542749-03	f	2025-10-25 15:43:48.544585-03	2025-10-25 15:43:48.544585-03	\N	0
84203879-9a00-45f1-b9bd-1ce999159cfd	60768bfc-9f81-44fc-a868-b8a74dfbacd2	GxkuSa3iTz5QVR9JaTDyW3JVA786lV+Dxzgt7T115Fk=	2025-11-01 15:55:42.692083-03	t	2025-10-25 15:55:42.782439-03	2025-10-25 15:56:00.403789-03	\N	1
0dcef918-38d3-452c-8a06-d756852fd5cd	60768bfc-9f81-44fc-a868-b8a74dfbacd2	bc3QR1Ry27vbv2Uedxb2SWQ8FCCLriN2I6ws4uUhUEA=	2025-11-01 15:56:00.4112-03	f	2025-10-25 15:56:00.412859-03	2025-10-25 15:56:00.412859-03	\N	0
0c1de354-bac9-4be8-84af-3eade7e6c21d	60768bfc-9f81-44fc-a868-b8a74dfbacd2	9CZAjtYc/lnk0yqFKVTcYkeENDo8CDVQg8BetB+bOso=	2025-11-01 16:12:09.151858-03	f	2025-10-25 16:12:09.239196-03	2025-10-25 16:12:09.239196-03	\N	0
f3af70a4-7481-4eb5-ab53-00a2b295ac0f	476be406-2b55-449c-8eda-0c60a585c87b	rnpkz5zRGahwipuatyuediaA4ovOREnI07gIsIZDE0c=	2025-11-01 16:23:09.519307-03	f	2025-10-25 16:23:09.614616-03	2025-10-25 16:23:09.614616-03	\N	0
837466fd-24a2-4343-897d-6f5a967ac7d4	476be406-2b55-449c-8eda-0c60a585c87b	7n+27VvUkP9Rcf6mTb9zUch7cbc89bdJBS6OzVwDFyw=	2025-11-03 19:35:09.083594-03	t	2025-10-27 19:35:09.173825-03	2025-10-27 19:37:04.022756-03	\N	1
a2390f67-dbfa-4e33-82e8-16ea02867c28	476be406-2b55-449c-8eda-0c60a585c87b	PlJfEVRQ//6vwSX0BoFF7GFoCHTgUcTv4He3bR8xSLQ=	2025-11-03 19:37:04.052152-03	f	2025-10-27 19:37:04.054003-03	2025-10-27 19:37:04.054003-03	\N	0
\.


--
-- TOC entry 5092 (class 0 OID 27745)
-- Dependencies: 228
-- Data for Name: schedule_exceptions; Type: TABLE DATA; Schema: empresateste; Owner: -
--

COPY empresateste.schedule_exceptions (id, professional_id, unit_id, start_at, end_at, reason, created_at, created_by, updated_at, updated_by, deleted_at, row_version) FROM stdin;
\.


--
-- TOC entry 5097 (class 0 OID 27843)
-- Dependencies: 233
-- Data for Name: settings; Type: TABLE DATA; Schema: empresateste; Owner: -
--

COPY empresateste.settings (key, value, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 5090 (class 0 OID 27700)
-- Dependencies: 226
-- Data for Name: staff_units; Type: TABLE DATA; Schema: empresateste; Owner: -
--

COPY empresateste.staff_units (staff_unit_id, user_id, unit_id, role_in_unit, created_at, created_by, updated_at, updated_by, deleted_at) FROM stdin;
\.


--
-- TOC entry 5096 (class 0 OID 27825)
-- Dependencies: 232
-- Data for Name: unit_holidays; Type: TABLE DATA; Schema: empresateste; Owner: -
--

COPY empresateste.unit_holidays (id, unit_id, holiday_on, description, created_at, created_by, updated_at, updated_by, deleted_at, row_version) FROM stdin;
\.


--
-- TOC entry 5087 (class 0 OID 27655)
-- Dependencies: 223
-- Data for Name: units; Type: TABLE DATA; Schema: empresateste; Owner: -
--

COPY empresateste.units (id, name, code, is_active, created_at, created_by, updated_at, updated_by, deleted_at, row_version) FROM stdin;
019a1cbb-3e65-7653-812a-bffb68aa9112	teste1	\N	t	2025-10-25 15:57:04.630745-03	60768bfc-9f81-44fc-a868-b8a74dfbacd2	2025-10-25 15:57:04.630745-03	\N	\N	0
019a1cb0-07a3-7c15-8624-bd2821355a8e	Testenovo	\N	t	2025-10-25 15:44:49.718908-03	60768bfc-9f81-44fc-a868-b8a74dfbacd2	2025-10-25 16:11:09.87911-03	60768bfc-9f81-44fc-a868-b8a74dfbacd2	2025-10-25 16:11:09.87911-03	5
\.


--
-- TOC entry 5088 (class 0 OID 27668)
-- Dependencies: 224
-- Data for Name: users; Type: TABLE DATA; Schema: empresateste; Owner: -
--

COPY empresateste.users (id, full_name, email, password_hash, role, is_active, created_at, created_by, updated_at, updated_by, deleted_at, row_version) FROM stdin;
476be406-2b55-449c-8eda-0c60a585c87b	LuisTeste	Luis@gmail.com	$2a$11$ZP5cSDvmx2CV.L8yH7d1TuGENk.ARkYQrG6F2Sv6QmmFy1jDovRdm	admin	t	2025-10-25 15:30:24.8857-03	\N	2025-10-25 15:30:24.8857-03	\N	\N	1
60768bfc-9f81-44fc-a868-b8a74dfbacd2	LuisTeste2	Luis2@gmail.com	$2a$11$UAhoC31x1t9m.cV2axO.auVMY9Zuyw5onJfwcM8iDjIUeL6K5TxgC	admin	t	2025-10-25 15:32:00.90208-03	\N	2025-10-25 15:32:00.90208-03	\N	\N	0
e6dcafad-64ef-4f39-ac23-38d13c32dbb7	Teste Auditoria 2	teste.auditoria@ex.com	$2a$11$hash_bcrypt_fake	admin	t	2025-10-25 15:24:19.46532-03	\N	2025-10-25 16:12:21.614405-03	\N	2025-10-25 16:12:21.591187-03	2
33b11613-3704-4235-bddc-908e579557d5	Novoteste	string@gmail.com	$2a$11$lwcYqeEM88qqzJelq21vEeZ98fE6vHWXnkB3mvqTjAbf97D3VibcG	admin	t	2025-10-25 15:30:24.8857-03	\N	2025-10-25 16:26:16.049414-03	\N	\N	2
\.


--
-- TOC entry 5086 (class 0 OID 27214)
-- Dependencies: 222
-- Data for Name: tenants; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.tenants (tenant_id, slug, name, is_active, created_at, updated_at, deleted_at) FROM stdin;
e22d8bd5-7369-4d0f-8865-3341df7f2179	clinica-sol	Clínica Sol	t	2025-10-23 21:22:29.974239-03	\N	\N
9e75cdbf-2d0e-47c5-8173-e879672bb638	empresa_teste	empresa_teste	t	2025-10-23 21:23:14.74901-03	\N	\N
9672f168-ce8a-4030-8a6b-8e9fe8c523c1	empresateste	Empresa Teste	t	2025-10-25 13:45:12.329686-03	\N	\N
\.


--
-- TOC entry 4907 (class 2606 OID 27791)
-- Name: bookings bookings_pkey; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.bookings
    ADD CONSTRAINT bookings_pkey PRIMARY KEY (id);


--
-- TOC entry 4905 (class 2606 OID 27778)
-- Name: patients patients_pkey; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.patients
    ADD CONSTRAINT patients_pkey PRIMARY KEY (id);


--
-- TOC entry 4899 (class 2606 OID 27733)
-- Name: professional_schedules professional_schedules_pkey; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.professional_schedules
    ADD CONSTRAINT professional_schedules_pkey PRIMARY KEY (id);


--
-- TOC entry 4892 (class 2606 OID 27692)
-- Name: professionals professionals_pkey; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.professionals
    ADD CONSTRAINT professionals_pkey PRIMARY KEY (id);


--
-- TOC entry 4894 (class 2606 OID 27694)
-- Name: professionals professionals_user_id_key; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.professionals
    ADD CONSTRAINT professionals_user_id_key UNIQUE (user_id);


--
-- TOC entry 4911 (class 2606 OID 27818)
-- Name: refresh_tokens refresh_tokens_pkey; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.refresh_tokens
    ADD CONSTRAINT refresh_tokens_pkey PRIMARY KEY (id);


--
-- TOC entry 4902 (class 2606 OID 27755)
-- Name: schedule_exceptions schedule_exceptions_pkey; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.schedule_exceptions
    ADD CONSTRAINT schedule_exceptions_pkey PRIMARY KEY (id);


--
-- TOC entry 4917 (class 2606 OID 27851)
-- Name: settings settings_pkey; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.settings
    ADD CONSTRAINT settings_pkey PRIMARY KEY (key);


--
-- TOC entry 4896 (class 2606 OID 27710)
-- Name: staff_units staff_units_pkey; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.staff_units
    ADD CONSTRAINT staff_units_pkey PRIMARY KEY (staff_unit_id);


--
-- TOC entry 4913 (class 2606 OID 27835)
-- Name: unit_holidays unit_holidays_pkey; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.unit_holidays
    ADD CONSTRAINT unit_holidays_pkey PRIMARY KEY (id);


--
-- TOC entry 4915 (class 2606 OID 27837)
-- Name: unit_holidays unit_holidays_unit_id_holiday_on_key; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.unit_holidays
    ADD CONSTRAINT unit_holidays_unit_id_holiday_on_key UNIQUE (unit_id, holiday_on);


--
-- TOC entry 4887 (class 2606 OID 27666)
-- Name: units units_pkey; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.units
    ADD CONSTRAINT units_pkey PRIMARY KEY (id);


--
-- TOC entry 4890 (class 2606 OID 27680)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- TOC entry 4882 (class 2606 OID 27223)
-- Name: tenants tenants_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tenants
    ADD CONSTRAINT tenants_pkey PRIMARY KEY (tenant_id);


--
-- TOC entry 4884 (class 2606 OID 27225)
-- Name: tenants tenants_slug_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tenants
    ADD CONSTRAINT tenants_slug_key UNIQUE (slug);


--
-- TOC entry 4908 (class 1259 OID 27807)
-- Name: empresateste_ix_bookings_unit_prof_dt; Type: INDEX; Schema: empresateste; Owner: -
--

CREATE INDEX empresateste_ix_bookings_unit_prof_dt ON empresateste.bookings USING btree (unit_id, professional_id, start_at);


--
-- TOC entry 4903 (class 1259 OID 27779)
-- Name: empresateste_ix_patients_name; Type: INDEX; Schema: empresateste; Owner: -
--

CREATE INDEX empresateste_ix_patients_name ON empresateste.patients USING btree (lower(full_name));


--
-- TOC entry 4900 (class 1259 OID 27766)
-- Name: empresateste_ix_sched_ex_prof_unit; Type: INDEX; Schema: empresateste; Owner: -
--

CREATE INDEX empresateste_ix_sched_ex_prof_unit ON empresateste.schedule_exceptions USING btree (professional_id, unit_id, start_at, end_at);


--
-- TOC entry 4897 (class 1259 OID 27744)
-- Name: empresateste_ix_sched_prof_unit_day; Type: INDEX; Schema: empresateste; Owner: -
--

CREATE INDEX empresateste_ix_sched_prof_unit_day ON empresateste.professional_schedules USING btree (professional_id, unit_id, weekday);


--
-- TOC entry 4909 (class 1259 OID 27824)
-- Name: empresateste_ux_refresh_token_hash; Type: INDEX; Schema: empresateste; Owner: -
--

CREATE UNIQUE INDEX empresateste_ux_refresh_token_hash ON empresateste.refresh_tokens USING btree (token_hash);


--
-- TOC entry 4885 (class 1259 OID 27667)
-- Name: empresateste_ux_units_name; Type: INDEX; Schema: empresateste; Owner: -
--

CREATE UNIQUE INDEX empresateste_ux_units_name ON empresateste.units USING btree (lower(name));


--
-- TOC entry 4888 (class 1259 OID 27681)
-- Name: empresateste_ux_users_email; Type: INDEX; Schema: empresateste; Owner: -
--

CREATE UNIQUE INDEX empresateste_ux_users_email ON empresateste.users USING btree (email);


--
-- TOC entry 4880 (class 1259 OID 27226)
-- Name: ix_tenants_slug; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_tenants_slug ON public.tenants USING btree (slug);


--
-- TOC entry 4937 (class 2620 OID 27859)
-- Name: bookings trg_bookings_set_updated_at; Type: TRIGGER; Schema: empresateste; Owner: -
--

CREATE TRIGGER trg_bookings_set_updated_at BEFORE UPDATE ON empresateste.bookings FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();


--
-- TOC entry 4936 (class 2620 OID 27858)
-- Name: patients trg_patients_set_updated_at; Type: TRIGGER; Schema: empresateste; Owner: -
--

CREATE TRIGGER trg_patients_set_updated_at BEFORE UPDATE ON empresateste.patients FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();


--
-- TOC entry 4934 (class 2620 OID 27856)
-- Name: professional_schedules trg_prof_sched_set_updated_at; Type: TRIGGER; Schema: empresateste; Owner: -
--

CREATE TRIGGER trg_prof_sched_set_updated_at BEFORE UPDATE ON empresateste.professional_schedules FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();


--
-- TOC entry 4932 (class 2620 OID 27854)
-- Name: professionals trg_prof_set_updated_at; Type: TRIGGER; Schema: empresateste; Owner: -
--

CREATE TRIGGER trg_prof_set_updated_at BEFORE UPDATE ON empresateste.professionals FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();


--
-- TOC entry 4938 (class 2620 OID 27869)
-- Name: refresh_tokens trg_refresh_tokens_updated; Type: TRIGGER; Schema: empresateste; Owner: -
--

CREATE TRIGGER trg_refresh_tokens_updated BEFORE UPDATE ON empresateste.refresh_tokens FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();


--
-- TOC entry 4935 (class 2620 OID 27857)
-- Name: schedule_exceptions trg_sched_exc_set_updated_at; Type: TRIGGER; Schema: empresateste; Owner: -
--

CREATE TRIGGER trg_sched_exc_set_updated_at BEFORE UPDATE ON empresateste.schedule_exceptions FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();


--
-- TOC entry 4940 (class 2620 OID 27862)
-- Name: settings trg_settings_touch; Type: TRIGGER; Schema: empresateste; Owner: -
--

CREATE TRIGGER trg_settings_touch BEFORE UPDATE ON empresateste.settings FOR EACH ROW EXECUTE FUNCTION empresateste.fn_settings_touch();


--
-- TOC entry 4933 (class 2620 OID 27855)
-- Name: staff_units trg_staff_units_set_updated_at; Type: TRIGGER; Schema: empresateste; Owner: -
--

CREATE TRIGGER trg_staff_units_set_updated_at BEFORE UPDATE ON empresateste.staff_units FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();


--
-- TOC entry 4939 (class 2620 OID 27860)
-- Name: unit_holidays trg_unit_holidays_set_updated_at; Type: TRIGGER; Schema: empresateste; Owner: -
--

CREATE TRIGGER trg_unit_holidays_set_updated_at BEFORE UPDATE ON empresateste.unit_holidays FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();


--
-- TOC entry 4930 (class 2620 OID 27853)
-- Name: units trg_units_set_updated_at; Type: TRIGGER; Schema: empresateste; Owner: -
--

CREATE TRIGGER trg_units_set_updated_at BEFORE UPDATE ON empresateste.units FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();


--
-- TOC entry 4931 (class 2620 OID 27852)
-- Name: users trg_users_set_updated_at; Type: TRIGGER; Schema: empresateste; Owner: -
--

CREATE TRIGGER trg_users_set_updated_at BEFORE UPDATE ON empresateste.users FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();


--
-- TOC entry 4925 (class 2606 OID 27802)
-- Name: bookings bookings_patient_id_fkey; Type: FK CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.bookings
    ADD CONSTRAINT bookings_patient_id_fkey FOREIGN KEY (patient_id) REFERENCES empresateste.patients(id) ON DELETE RESTRICT;


--
-- TOC entry 4926 (class 2606 OID 27797)
-- Name: bookings bookings_professional_id_fkey; Type: FK CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.bookings
    ADD CONSTRAINT bookings_professional_id_fkey FOREIGN KEY (professional_id) REFERENCES empresateste.professionals(id) ON DELETE RESTRICT;


--
-- TOC entry 4927 (class 2606 OID 27792)
-- Name: bookings bookings_unit_id_fkey; Type: FK CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.bookings
    ADD CONSTRAINT bookings_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES empresateste.units(id) ON DELETE RESTRICT;


--
-- TOC entry 4921 (class 2606 OID 27734)
-- Name: professional_schedules professional_schedules_professional_id_fkey; Type: FK CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.professional_schedules
    ADD CONSTRAINT professional_schedules_professional_id_fkey FOREIGN KEY (professional_id) REFERENCES empresateste.professionals(id) ON DELETE CASCADE;


--
-- TOC entry 4922 (class 2606 OID 27739)
-- Name: professional_schedules professional_schedules_unit_id_fkey; Type: FK CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.professional_schedules
    ADD CONSTRAINT professional_schedules_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES empresateste.units(id) ON DELETE CASCADE;


--
-- TOC entry 4918 (class 2606 OID 27695)
-- Name: professionals professionals_user_id_fkey; Type: FK CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.professionals
    ADD CONSTRAINT professionals_user_id_fkey FOREIGN KEY (user_id) REFERENCES empresateste.users(id) ON DELETE CASCADE;


--
-- TOC entry 4928 (class 2606 OID 27819)
-- Name: refresh_tokens refresh_tokens_user_id_fkey; Type: FK CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.refresh_tokens
    ADD CONSTRAINT refresh_tokens_user_id_fkey FOREIGN KEY (user_id) REFERENCES empresateste.users(id) ON DELETE CASCADE;


--
-- TOC entry 4923 (class 2606 OID 27756)
-- Name: schedule_exceptions schedule_exceptions_professional_id_fkey; Type: FK CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.schedule_exceptions
    ADD CONSTRAINT schedule_exceptions_professional_id_fkey FOREIGN KEY (professional_id) REFERENCES empresateste.professionals(id) ON DELETE CASCADE;


--
-- TOC entry 4924 (class 2606 OID 27761)
-- Name: schedule_exceptions schedule_exceptions_unit_id_fkey; Type: FK CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.schedule_exceptions
    ADD CONSTRAINT schedule_exceptions_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES empresateste.units(id) ON DELETE CASCADE;


--
-- TOC entry 4919 (class 2606 OID 27716)
-- Name: staff_units staff_units_unit_id_fkey; Type: FK CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.staff_units
    ADD CONSTRAINT staff_units_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES empresateste.units(id) ON DELETE CASCADE;


--
-- TOC entry 4920 (class 2606 OID 27711)
-- Name: staff_units staff_units_user_id_fkey; Type: FK CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.staff_units
    ADD CONSTRAINT staff_units_user_id_fkey FOREIGN KEY (user_id) REFERENCES empresateste.users(id) ON DELETE CASCADE;


--
-- TOC entry 4929 (class 2606 OID 27838)
-- Name: unit_holidays unit_holidays_unit_id_fkey; Type: FK CONSTRAINT; Schema: empresateste; Owner: -
--

ALTER TABLE ONLY empresateste.unit_holidays
    ADD CONSTRAINT unit_holidays_unit_id_fkey FOREIGN KEY (unit_id) REFERENCES empresateste.units(id) ON DELETE CASCADE;


-- Completed on 2025-10-27 19:41:27

--
-- PostgreSQL database dump complete
--

