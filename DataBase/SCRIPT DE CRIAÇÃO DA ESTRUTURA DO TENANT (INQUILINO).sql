-- =====================================================================================
-- FUNÇÃO COMPLETA PARA CRIAÇÃO DE TENANTS (MULTI-SCHEMA)
-- Inclui todas as tabelas principais: users, units, services, people,
-- professionals, customers, bookings, schedules, exceptions, refresh_tokens, settings
-- =====================================================================================

CREATE OR REPLACE FUNCTION public.create_tenant_schema(tenant_subdomain TEXT)
RETURNS void LANGUAGE plpgsql AS $$
BEGIN
    RAISE NOTICE '🏗️  Criando schema %...', tenant_subdomain;
    EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I', tenant_subdomain);

    -- ============================================
    -- 1️⃣ USERS
    -- ============================================
    BEGIN
        EXECUTE format($q$
            CREATE TABLE IF NOT EXISTS %I.users (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                full_name VARCHAR(255) NOT NULL,
                email VARCHAR(255) NOT NULL UNIQUE,
                password_hash VARCHAR(255) NOT NULL,
                role user_role NOT NULL DEFAULT 'receptionist',
                is_active BOOLEAN NOT NULL DEFAULT TRUE,
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                row_version BIGINT NOT NULL DEFAULT 1,
                deleted_at TIMESTAMPTZ
            );
        $q$, tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_users_updated
                        BEFORE UPDATE ON %I.users
                        FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_set_deleted_at_users
                        BEFORE UPDATE OF deleted_at ON %I.users
                        FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL)
                        EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

        RAISE NOTICE '✅ users criada';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Erro em users: %', SQLERRM;
    END;

    -- ============================================
    -- 2️⃣ UNITS
    -- ============================================
    BEGIN
        EXECUTE format($q$
            CREATE TABLE IF NOT EXISTS %I.units (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(255) NOT NULL,
                address TEXT,
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                created_by UUID REFERENCES %I.users(id),
                updated_by UUID REFERENCES %I.users(id),
                deleted_at TIMESTAMPTZ
            );
        $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_units_updated
                        BEFORE UPDATE ON %I.units
                        FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_set_deleted_at_units
                        BEFORE UPDATE OF deleted_at ON %I.units
                        FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL)
                        EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

        RAISE NOTICE '✅ units criada';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Erro em units: %', SQLERRM;
    END;

    -- ============================================
    -- 3️⃣ SERVICES
    -- ============================================
    BEGIN
        EXECUTE format($q$
            CREATE TABLE IF NOT EXISTS %I.services (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(255) NOT NULL,
                duration_minutes INT NOT NULL,
                price NUMERIC(10,2) NOT NULL,
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                created_by UUID REFERENCES %I.users(id),
                updated_by UUID REFERENCES %I.users(id),
                deleted_at TIMESTAMPTZ
            );
        $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_services_updated
                        BEFORE UPDATE ON %I.services
                        FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_set_deleted_at_services
                        BEFORE UPDATE OF deleted_at ON %I.services
                        FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL)
                        EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

        RAISE NOTICE '✅ services criada';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Erro em services: %', SQLERRM;
    END;

    -- ============================================
    -- 4️⃣ PEOPLE
    -- ============================================
    BEGIN
        EXECUTE format($q$
            CREATE TABLE IF NOT EXISTS %I.people (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                full_name VARCHAR(255) NOT NULL,
                cpf VARCHAR(11) UNIQUE,
                phone VARCHAR(20),
                email VARCHAR(255) UNIQUE,
                address TEXT,
                city VARCHAR(100),
                state VARCHAR(100),
                zip_code VARCHAR(10),
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                created_by UUID REFERENCES %I.users(id),
                updated_by UUID REFERENCES %I.users(id),
                deleted_at TIMESTAMPTZ
            );
        $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_people_updated
                        BEFORE UPDATE ON %I.people
                        FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_set_deleted_at_people
                        BEFORE UPDATE OF deleted_at ON %I.people
                        FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL)
                        EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

        RAISE NOTICE '✅ people criada';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Erro em people: %', SQLERRM;
    END;

    -- ============================================
    -- 5️⃣ PROFESSIONALS / CUSTOMERS
    -- ============================================
    BEGIN
        EXECUTE format($q$
            CREATE TABLE IF NOT EXISTS %I.professionals (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                person_id UUID NOT NULL REFERENCES %I.people(id) UNIQUE,
                specialty VARCHAR(255),
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                created_by UUID REFERENCES %I.users(id),
                updated_by UUID REFERENCES %I.users(id),
                deleted_at TIMESTAMPTZ
            );
        $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_professionals_updated
                        BEFORE UPDATE ON %I.professionals
                        FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_set_deleted_at_professionals
                        BEFORE UPDATE OF deleted_at ON %I.professionals
                        FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL)
                        EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

        RAISE NOTICE '✅ professionals criada';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Erro em professionals: %', SQLERRM;
    END;

    BEGIN
        EXECUTE format($q$
            CREATE TABLE IF NOT EXISTS %I.customers (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                person_id UUID NOT NULL REFERENCES %I.people(id) UNIQUE,
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                created_by UUID REFERENCES %I.users(id),
                updated_by UUID REFERENCES %I.users(id),
                deleted_at TIMESTAMPTZ
            );
        $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_customers_updated
                        BEFORE UPDATE ON %I.customers
                        FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_set_deleted_at_customers
                        BEFORE UPDATE OF deleted_at ON %I.customers
                        FOR EACH ROW WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NULL)
                        EXECUTE FUNCTION public.set_deleted_at_timestamp();', tenant_subdomain);

        RAISE NOTICE '✅ customers criada';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Erro em customers: %', SQLERRM;
    END;

    -- ============================================
    -- 6️⃣ SCHEDULES (AGENDA)
    -- ============================================
    BEGIN
        EXECUTE format($q$
            CREATE TABLE IF NOT EXISTS %I.professional_schedules (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                professional_id UUID NOT NULL REFERENCES %I.professionals(id),
                unit_id UUID NOT NULL REFERENCES %I.units(id),
                day_of_week SMALLINT NOT NULL CHECK (day_of_week BETWEEN 0 AND 6),
                start_time TIME NOT NULL,
                end_time TIME NOT NULL,
                status schedule_status NOT NULL DEFAULT 'available',
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                created_by UUID REFERENCES %I.users(id),
                updated_by UUID REFERENCES %I.users(id),
                deleted_at TIMESTAMPTZ,
                UNIQUE (professional_id, unit_id, day_of_week, start_time, end_time)
            );
        $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_professional_schedules_updated
                        BEFORE UPDATE ON %I.professional_schedules
                        FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);

        RAISE NOTICE '✅ professional_schedules criada';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Erro em professional_schedules: %', SQLERRM;
    END;

    BEGIN
        EXECUTE format($q$
            CREATE TABLE IF NOT EXISTS %I.schedule_exceptions (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                professional_id UUID NOT NULL REFERENCES %I.professionals(id),
                unit_id UUID NOT NULL REFERENCES %I.units(id),
                date_from TIMESTAMPTZ NOT NULL,
                date_to TIMESTAMPTZ NOT NULL,
                status schedule_status NOT NULL DEFAULT 'blocked',
                reason VARCHAR(255),
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                created_by UUID REFERENCES %I.users(id),
                updated_by UUID REFERENCES %I.users(id),
                deleted_at TIMESTAMPTZ
            );
        $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_schedule_exceptions_updated
                        BEFORE UPDATE ON %I.schedule_exceptions
                        FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);

        RAISE NOTICE '✅ schedule_exceptions criada';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Erro em schedule_exceptions: %', SQLERRM;
    END;

    -- ============================================
    -- 7️⃣ BOOKINGS
    -- ============================================
    BEGIN
        EXECUTE format($q$
            CREATE TABLE IF NOT EXISTS %I.bookings (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                professional_id UUID NOT NULL REFERENCES %I.professionals(id),
                customer_id UUID NOT NULL REFERENCES %I.customers(id),
                service_id UUID NOT NULL REFERENCES %I.services(id),
                unit_id UUID NOT NULL REFERENCES %I.units(id),
                start_time TIMESTAMPTZ NOT NULL,
                end_time TIMESTAMPTZ NOT NULL,
                status booking_status NOT NULL DEFAULT 'scheduled',
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                created_by UUID REFERENCES %I.users(id),
                updated_by UUID REFERENCES %I.users(id),
                deleted_at TIMESTAMPTZ,
                EXCLUDE USING GIST (
                    professional_id WITH =,
                    TSTZRANGE(start_time, end_time) WITH &&
                )
            );
        $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain, tenant_subdomain);

        EXECUTE format('CREATE INDEX IF NOT EXISTS idx_bookings_prof_time ON %I.bookings (professional_id, start_time);', tenant_subdomain);
        EXECUTE format('CREATE INDEX IF NOT EXISTS idx_bookings_cust_time ON %I.bookings (customer_id, start_time);', tenant_subdomain);

        RAISE NOTICE '✅ bookings criada';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Erro em bookings: %', SQLERRM;
    END;

    -- ============================================
    -- 8️⃣ REFRESH TOKENS
    -- ============================================
    BEGIN
        EXECUTE format($q$
            CREATE TABLE IF NOT EXISTS %I.refresh_tokens (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                user_id UUID NOT NULL REFERENCES %I.users(id) ON DELETE CASCADE,
                token_hash VARCHAR(256) NOT NULL UNIQUE,
                expires_at_utc TIMESTAMPTZ NOT NULL,
                is_revoked BOOLEAN DEFAULT FALSE,
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ,
                deleted_at TIMESTAMPTZ
            );
        $q$, tenant_subdomain, tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_refresh_tokens_updated
                        BEFORE UPDATE ON %I.refresh_tokens
                        FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);

        RAISE NOTICE '✅ refresh_tokens criada';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Erro em refresh_tokens: %', SQLERRM;
    END;

    -- ============================================
    -- 9️⃣ TENANT SETTINGS
    -- ============================================
    BEGIN
        EXECUTE format($q$
            CREATE TABLE IF NOT EXISTS %I.tenant_settings (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                key setting_key NOT NULL,
                value TEXT,
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                created_by UUID REFERENCES %I.users(id),
                updated_by UUID REFERENCES %I.users(id),
                deleted_at TIMESTAMPTZ,
                UNIQUE (key)
            );
        $q$, tenant_subdomain, tenant_subdomain, tenant_subdomain);

        EXECUTE format('CREATE TRIGGER trg_tenant_settings_updated
                        BEFORE UPDATE ON %I.tenant_settings
                        FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();', tenant_subdomain);

        RAISE NOTICE '✅ tenant_settings criada';
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'Erro em tenant_settings: %', SQLERRM;
    END;

    RAISE NOTICE '🎉 Tenant % criado/atualizado com sucesso.', tenant_subdomain;
END;
$$;

-- =====================================================================================
-- ✅ EXEMPLO DE USO:
-- SELECT create_tenant_schema('empresa_teste');
-- =====================================================================================
