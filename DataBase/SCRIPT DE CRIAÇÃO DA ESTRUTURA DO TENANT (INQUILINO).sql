-- =================================================================================
-- ARQUIVO: FUNÇÃO PARA CRIAR TENANTS.sql
-- OBJETIVO: Cria uma função que gera um schema completo para um novo tenant.
-- EXECUÇÃO: Deve ser executado apenas UMA VEZ para registrar a função no banco.
-- =================================================================================

CREATE OR REPLACE FUNCTION create_tenant_schema(tenant_subdomain TEXT)
RETURNS void LANGUAGE plpgsql AS $$
BEGIN
    -- Cria o schema para o novo tenant
    EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I', tenant_subdomain);

    -- Define o caminho de busca para que as tabelas sejam criadas no schema correto,
    -- mas ainda consigam acessar os tipos e funções do schema 'public'.
    EXECUTE format('SET LOCAL search_path TO %I, public', tenant_subdomain);

    -- Agora, criamos todas as tabelas dentro do schema do tenant.
    -- Note a remoção da coluna 'tenant_id' e das referências a ela.

    -- 👤 Usuários e Identidade
    CREATE TABLE users (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        full_name VARCHAR(255) NOT NULL,
        email VARCHAR(255) NOT NULL UNIQUE,
        password_hash VARCHAR(255) NOT NULL,
        role user_role NOT NULL DEFAULT 'receptionist',
        is_active BOOLEAN NOT NULL DEFAULT TRUE,
        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        row_version BIGINT NOT NULL DEFAULT 1,
        deleted_at TIMESTAMPTZ
    );
    CREATE TRIGGER trg_users_updated BEFORE UPDATE ON users FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

    CREATE TABLE user_refresh_tokens (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
        token_hash VARCHAR(255) NOT NULL UNIQUE,
        expires_at TIMESTAMPTZ NOT NULL,
        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        revoked_at TIMESTAMPTZ
    );

    -- 🧑 Pessoas, Profissionais e Clientes
    CREATE TABLE people (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        full_name VARCHAR(255) NOT NULL,
        cpf VARCHAR(11) UNIQUE,
        phone VARCHAR(20),
        email VARCHAR(255) UNIQUE,
        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id),
        deleted_at TIMESTAMPTZ
    );
    CREATE TRIGGER trg_people_updated BEFORE UPDATE ON people FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

    CREATE TABLE professionals (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        person_id UUID NOT NULL REFERENCES people(id) UNIQUE,
        specialty VARCHAR(255),
        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id),
        deleted_at TIMESTAMPTZ
    );
    CREATE TRIGGER trg_professionals_updated BEFORE UPDATE ON professionals FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

    CREATE TABLE customers (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        person_id UUID NOT NULL REFERENCES people(id) UNIQUE,
        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id),
        deleted_at TIMESTAMPTZ
    );
    CREATE TRIGGER trg_customers_updated BEFORE UPDATE ON customers FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

    -- 🏢 Unidades e Serviços
    CREATE TABLE units (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        name VARCHAR(255) NOT NULL,
        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id),
        deleted_at TIMESTAMPTZ
    );
    CREATE TRIGGER trg_units_updated BEFORE UPDATE ON units FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

    CREATE TABLE services (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        name VARCHAR(255) NOT NULL,
        duration_minutes INT NOT NULL,
        price NUMERIC(10,2) NOT NULL,
        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id),
        deleted_at TIMESTAMPTZ
    );
    CREATE TRIGGER trg_services_updated BEFORE UPDATE ON services FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

    -- 🔐 RBAC com Escopo
    CREATE TABLE roles (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        name VARCHAR(50) NOT NULL UNIQUE
    );

    CREATE TABLE user_roles (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        user_id UUID NOT NULL REFERENCES users(id),
        role_id UUID NOT NULL REFERENCES roles(id),
        unit_id UUID NOT NULL REFERENCES units(id),
        UNIQUE (user_id, role_id, unit_id)
    );

    -- 📅 Agendamentos
    CREATE TABLE bookings (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        professional_id UUID NOT NULL REFERENCES professionals(id),
        customer_id UUID NOT NULL REFERENCES customers(id),
        service_id UUID NOT NULL REFERENCES services(id),
        unit_id UUID NOT NULL REFERENCES units(id),
        start_time TIMESTAMPTZ NOT NULL,
        end_time TIMESTAMPTZ NOT NULL,
        status booking_status NOT NULL DEFAULT 'confirmed',
        row_version BIGINT NOT NULL DEFAULT 1,
        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        created_by UUID REFERENCES users(id),
        updated_by UUID REFERENCES users(id),
        CONSTRAINT no_overlapping_bookings EXCLUDE USING GIST (
            professional_id WITH =,
            TSTZRANGE(start_time, end_time) WITH &&
        )
    );
    CREATE TRIGGER trg_bookings_updated BEFORE UPDATE ON bookings FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
    
    -- ... e assim por diante para todas as outras tabelas do tenant.
    -- Adicione aqui as tabelas restantes (schedule_exceptions, professional_schedules, tenant_settings)
    -- seguindo o mesmo padrão (removendo tenant_id).

END;
$$;

-- ========================== FIM DA FUNÇÃO ==========================