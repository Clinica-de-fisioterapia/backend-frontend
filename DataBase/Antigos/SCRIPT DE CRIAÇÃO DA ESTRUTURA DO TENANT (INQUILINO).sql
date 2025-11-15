-- =====================================================================================
-- Creates the full tenant schema with all domain tables and triggers
-- Multi-tenant per schema; NO TenantId columns.
-- =====================================================================================

CREATE OR REPLACE FUNCTION public.create_tenant_schema(p_slug TEXT)
RETURNS VOID
LANGUAGE plpgsql
AS $$
BEGIN
  IF p_slug IS NULL OR length(trim(p_slug)) = 0 THEN
    RAISE EXCEPTION 'Schema name required';
  END IF;

  -- Create schema if not present
  EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I', p_slug);

  -- ===================================================================================
  -- USERS
  -- ===================================================================================
  EXECUTE format($sql$
    CREATE TABLE IF NOT EXISTS %I.users (
      id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
      full_name     TEXT NOT NULL,
      email         CITEXT NOT NULL,
      password_hash TEXT NOT NULL,
      role          TEXT NOT NULL CHECK (role ~ '^[A-Za-z][A-Za-z0-9_]{0,49}$'),
      is_active     BOOLEAN NOT NULL DEFAULT TRUE,
      created_at    TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
      created_by    UUID,
      updated_at    TIMESTAMPTZ,
      updated_by    UUID,
      deleted_at    TIMESTAMPTZ,
      row_version   BIGINT NOT NULL DEFAULT 0
    );
  $sql$, p_slug);

  EXECUTE format(
    'CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.users (email);',
    p_slug||'_ux_users_email', p_slug
  );

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
  $fmt$, p_slug, p_slug);

  -- ===================================================================================
  -- UNITS
  -- ===================================================================================
  EXECUTE format($sql$
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
  $sql$, p_slug);

  EXECUTE format(
    'CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.units (lower(name));',
    p_slug||'_ux_units_name', p_slug
  );

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
  $fmt$, p_slug, p_slug);

  -- ===================================================================================
  -- USERS ↔ UNITS (staff_units)
  -- ===================================================================================
  EXECUTE format($sql$
    CREATE TABLE IF NOT EXISTS %I.staff_units (
      staff_unit_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
      user_id       UUID NOT NULL REFERENCES %I.users(id) ON DELETE CASCADE,
      unit_id       UUID NOT NULL REFERENCES %I.units(id) ON DELETE CASCADE,
      role_in_unit  TEXT CHECK (role_in_unit IN ('admin','receptionist','professional')),
      created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
      created_by    UUID,
      UNIQUE (user_id, unit_id)
    );
  $sql$, p_slug, p_slug, p_slug);

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
  $fmt$, p_slug, p_slug);

  -- ===================================================================================
  -- PROFESSIONALS
  -- ===================================================================================
  EXECUTE format($sql$
    CREATE TABLE IF NOT EXISTS %I.professionals (
      id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
      user_id       UUID NOT NULL REFERENCES %I.users(id) ON DELETE CASCADE,
      registry_code TEXT,
      specialty     TEXT,
      created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
      created_by    UUID,
      updated_at    TIMESTAMPTZ,
      updated_by    UUID,
      deleted_at    TIMESTAMPTZ,
      row_version   BIGINT NOT NULL DEFAULT 0,
      UNIQUE (user_id)
    );
  $sql$, p_slug, p_slug);

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
  $fmt$, p_slug, p_slug);

  -- ===================================================================================
  -- PATIENTS
  -- ===================================================================================
  EXECUTE format($sql$
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
  $sql$, p_slug);

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
  $fmt$, p_slug, p_slug);

  -- ===================================================================================
  -- BOOKINGS
  -- ===================================================================================
  EXECUTE format($sql$
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
  $sql$, p_slug, p_slug, p_slug, p_slug);

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
  $fmt$, p_slug, p_slug);

  -- ===================================================================================
  -- REFRESH TOKENS
  -- ===================================================================================
  EXECUTE format($sql$
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
  $sql$, p_slug, p_slug);

  EXECUTE format('CREATE UNIQUE INDEX IF NOT EXISTS %I ON %I.refresh_tokens (token_hash);',
                 p_slug||'_ux_refresh_token_hash', p_slug);

  EXECUTE format($fmt$
    DO $tg$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_trigger 
                       WHERE tgname = 'trg_refresh_tokens_set_updated_at' AND tgrelid = '%I.refresh_tokens'::regclass) THEN
            CREATE TRIGGER trg_refresh_tokens_set_updated_at
            BEFORE UPDATE ON %I.refresh_tokens
            FOR EACH ROW
            EXECUTE FUNCTION public.fn_set_updated_at();
        END IF;
    END$tg$;
  $fmt$, p_slug, p_slug);

  -- ===================================================================================
  -- SETTINGS
  -- ===================================================================================
  EXECUTE format($sql$
    CREATE TABLE IF NOT EXISTS %I.settings (
      key        TEXT PRIMARY KEY,
      value      JSONB NOT NULL,
      created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
      updated_at TIMESTAMPTZ
    );
  $sql$, p_slug);

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
  $fmt$, p_slug, p_slug, p_slug, p_slug);

END;
$$;
