--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.4

-- Started on 2025-10-25 16:43:06

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
-- TOC entry 4976 (class 1262 OID 27058)
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
-- TOC entry 6 (class 2615 OID 2200)
-- Name: public; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA public;


--
-- TOC entry 4977 (class 0 OID 0)
-- Dependencies: 6
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON SCHEMA public IS 'standard public schema';


--
-- TOC entry 954 (class 1247 OID 27202)
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
-- TOC entry 308 (class 1255 OID 27229)
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
-- TOC entry 240 (class 1255 OID 27228)
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
-- TOC entry 255 (class 1255 OID 27652)
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
-- TOC entry 237 (class 1255 OID 27653)
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
-- TOC entry 270 (class 1255 OID 27213)
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
-- TOC entry 265 (class 1255 OID 27227)
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
-- TOC entry 326 (class 1255 OID 27867)
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
-- TOC entry 4970 (class 0 OID 27214)
-- Dependencies: 222
-- Data for Name: tenants; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.tenants (tenant_id, slug, name, is_active, created_at, updated_at, deleted_at) FROM stdin;
e22d8bd5-7369-4d0f-8865-3341df7f2179	clinica-sol	Clínica Sol	t	2025-10-23 21:22:29.974239-03	\N	\N
9e75cdbf-2d0e-47c5-8173-e879672bb638	empresa_teste	empresa_teste	t	2025-10-23 21:23:14.74901-03	\N	\N
9672f168-ce8a-4030-8a6b-8e9fe8c523c1	empresateste	Empresa Teste	t	2025-10-25 13:45:12.329686-03	\N	\N
\.


--
-- TOC entry 4822 (class 2606 OID 27223)
-- Name: tenants tenants_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tenants
    ADD CONSTRAINT tenants_pkey PRIMARY KEY (tenant_id);


--
-- TOC entry 4824 (class 2606 OID 27225)
-- Name: tenants tenants_slug_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tenants
    ADD CONSTRAINT tenants_slug_key UNIQUE (slug);


--
-- TOC entry 4820 (class 1259 OID 27226)
-- Name: ix_tenants_slug; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_tenants_slug ON public.tenants USING btree (slug);


-- Completed on 2025-10-25 16:43:06

--
-- PostgreSQL database dump complete
--

