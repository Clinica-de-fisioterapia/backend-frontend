-- ====================================================================================
-- PARTE 2: TEMPLATE COMPLETO E CORRIGIDO DA ESTRUTURA DO TENANT (SCHEMA ISOLADO)
-- Descrição: Cria um schema isolado com todas as tabelas para um novo tenant.
-- Execução: Deve ser executado para CADA NOVO CLIENTE, substituindo o nome do schema.
-- ====================================================================================

-- 1. Cria o schema para isolar os dados do novo tenant.
--    IMPORTANTE: O nome 'schema_do_novo_tenant' DEVE ser substituído dinamicamente
--    pelo backend por um nome único para cada cliente (ex: 'tenant_a1b2c3d4').
CREATE SCHEMA IF NOT EXISTS schema_do_novo_tenant;

-- 2. Define o search_path para o novo schema E TAMBÉM para o public.
--    Esta é a correção crucial para que as extensões sejam encontradas.
SET search_path TO schema_do_novo_tenant, public;

-- Tabela de Usuários
CREATE TABLE IF NOT EXISTS users (
    user_id SERIAL PRIMARY KEY,
    person_id INT UNIQUE,
    username VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE
);
COMMENT ON TABLE users IS 'Usuários que podem logar no sistema (admins, recepcionistas, etc).';

-- Tabela de Pessoas
CREATE TABLE IF NOT EXISTS people (
    person_id SERIAL PRIMARY KEY,
    full_name VARCHAR(255) NOT NULL,
    cpf CHAR(11),
    email VARCHAR(255),
    phone VARCHAR(20),
    birth_date DATE,
    address TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE,
    created_by INT REFERENCES users(user_id),
    updated_by INT REFERENCES users(user_id)
);
COMMENT ON COLUMN people.cpf IS 'Armazenar apenas dígitos. Recomenda-se criptografar este campo com pgcrypto.';
COMMENT ON COLUMN people.email IS 'Recomenda-se criptografar este campo com pgcrypto.';
DROP TRIGGER IF EXISTS set_timestamp_people ON people;
CREATE TRIGGER set_timestamp_people BEFORE UPDATE ON people FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp(); -- Note a referência a 'public'
CREATE UNIQUE INDEX IF NOT EXISTS idx_people_cpf_unique_if_not_deleted ON people(cpf) WHERE deleted_at IS NULL;
CREATE UNIQUE INDEX IF NOT EXISTS idx_people_email_unique_if_not_deleted ON people(email) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_people_phone ON people(phone);
CREATE INDEX IF NOT EXISTS idx_people_full_name_trgm ON people USING GIN (full_name gin_trgm_ops);

-- Adicionando a chave estrangeira de 'users' para 'people' apenas se ela não existir
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_name = 'fk_users_person_id' AND table_name = 'users' AND table_schema = 'schema_do_novo_tenant' -- Schema dinâmico aqui
    ) THEN
        ALTER TABLE users ADD CONSTRAINT fk_users_person_id
        FOREIGN KEY (person_id) REFERENCES people(person_id);
    END IF;
END$$;

-- Tabela de Profissionais
CREATE TABLE IF NOT EXISTS professionals (
    professional_id SERIAL PRIMARY KEY,
    person_id INT NOT NULL UNIQUE REFERENCES people(person_id) ON DELETE CASCADE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by INT REFERENCES users(user_id),
    updated_by INT REFERENCES users(user_id)
);
DROP TRIGGER IF EXISTS set_timestamp_professionals ON professionals;
CREATE TRIGGER set_timestamp_professionals BEFORE UPDATE ON professionals FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Tabela de Clientes
CREATE TABLE IF NOT EXISTS customers (
    customer_id SERIAL PRIMARY KEY,
    person_id INT NOT NULL UNIQUE REFERENCES people(person_id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by INT REFERENCES users(user_id),
    updated_by INT REFERENCES users(user_id)
);
DROP TRIGGER IF EXISTS set_timestamp_customers ON customers;
CREATE TRIGGER set_timestamp_customers BEFORE UPDATE ON customers FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Tabela de Unidades
CREATE TABLE IF NOT EXISTS units (
    unit_id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    address TEXT,
    phone VARCHAR(20),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE,
    created_by INT REFERENCES users(user_id),
    updated_by INT REFERENCES users(user_id)
);
DROP TRIGGER IF EXISTS set_timestamp_units ON units;
CREATE TRIGGER set_timestamp_units BEFORE UPDATE ON units FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Tabela de Perfis/Funções
CREATE TABLE IF NOT EXISTS roles (
    role_id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);
INSERT INTO roles (name) VALUES ('Admin'), ('Professional'), ('Receptionist') ON CONFLICT (name) DO NOTHING;

-- Tabela de associação User-Role-Unit
CREATE TABLE IF NOT EXISTS user_roles (
    user_id INT NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    role_id INT NOT NULL REFERENCES roles(role_id) ON DELETE CASCADE,
    unit_id INT NOT NULL REFERENCES units(unit_id) ON DELETE CASCADE,
    assigned_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    assigned_by INT NOT NULL REFERENCES users(user_id),
    PRIMARY KEY (user_id, role_id, unit_id)
);

-- Tabela de Serviços
CREATE TABLE IF NOT EXISTS services (
    service_id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    duration_minutes INT NOT NULL CHECK (duration_minutes > 0),
    price DECIMAL(10, 2) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE,
    created_by INT REFERENCES users(user_id),
    updated_by INT REFERENCES users(user_id)
);
DROP TRIGGER IF EXISTS set_timestamp_services ON services;
CREATE TRIGGER set_timestamp_services BEFORE UPDATE ON services FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Tabela de Horários de Disponibilidade
CREATE TABLE IF NOT EXISTS schedule_slots (
    schedule_slot_id SERIAL PRIMARY KEY,
    professional_id INT NOT NULL REFERENCES professionals(professional_id) ON DELETE CASCADE,
    unit_id INT NOT NULL REFERENCES units(unit_id) ON DELETE CASCADE,
    weekday SMALLINT NOT NULL CHECK (weekday BETWEEN 0 AND 6),
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE,
    created_by INT REFERENCES users(user_id),
    updated_by INT REFERENCES users(user_id)
);
DROP TRIGGER IF EXISTS set_timestamp_schedule_slots ON schedule_slots;
CREATE TRIGGER set_timestamp_schedule_slots BEFORE UPDATE ON schedule_slots FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Tabela de Agendamentos (Particionada)
CREATE TABLE IF NOT EXISTS bookings (
    booking_id BIGSERIAL NOT NULL,
    customer_id INT NOT NULL REFERENCES customers(customer_id) ON DELETE RESTRICT,
    professional_id INT NOT NULL REFERENCES professionals(professional_id) ON DELETE RESTRICT,
    unit_id INT NOT NULL REFERENCES units(unit_id) ON DELETE RESTRICT,
    service_id INT NOT NULL REFERENCES services(service_id) ON DELETE RESTRICT,
    start_datetime TIMESTAMP WITH TIME ZONE NOT NULL,
    end_datetime TIMESTAMP WITH TIME ZONE NOT NULL,
    status public.booking_status NOT NULL DEFAULT 'confirmed', -- Note a referência a 'public'
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by INT REFERENCES users(user_id),
    updated_by INT REFERENCES users(user_id),
    PRIMARY KEY (booking_id, start_datetime),
    CONSTRAINT no_overlapping_bookings EXCLUDE USING GIST (
        professional_id WITH =,
        start_datetime WITH =,
        TSTZRANGE(start_datetime, end_datetime) WITH &&
    )
) PARTITION BY RANGE (start_datetime);
COMMENT ON TABLE bookings IS 'Tabela principal de agendamentos, particionada por data para performance.';
DROP TRIGGER IF EXISTS set_timestamp_bookings ON bookings;
CREATE TRIGGER set_timestamp_bookings BEFORE UPDATE ON bookings FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Partições de Exemplo para a Tabela Bookings
CREATE TABLE IF NOT EXISTS bookings_y2025 PARTITION OF bookings FOR VALUES FROM ('2025-01-01 00:00:00-03') TO ('2026-01-01 00:00:00-03');
CREATE TABLE IF NOT EXISTS bookings_y2026 PARTITION OF bookings FOR VALUES FROM ('2026-01-01 00:00:00-03') TO ('2027-01-01 00:00:00-03');

-- Índices da tabela de agendamentos
CREATE INDEX IF NOT EXISTS idx_bookings_professional_datetime ON bookings(professional_id, start_datetime);
CREATE INDEX IF NOT EXISTS idx_bookings_customer_datetime ON bookings(customer_id, start_datetime);
CREATE INDEX IF NOT EXISTS idx_bookings_unit_status_datetime ON bookings(unit_id, status, start_datetime);

-- Tabela de Configurações
CREATE TABLE IF NOT EXISTS tenant_settings (
    setting_key VARCHAR(100) PRIMARY KEY,
    setting_value VARCHAR(255) NOT NULL,
    description TEXT,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_by INT REFERENCES users(user_id)
);
COMMENT ON TABLE tenant_settings IS 'Armazena configurações customizáveis para o sistema.';
DROP TRIGGER IF EXISTS set_timestamp_tenant_settings ON tenant_settings;
CREATE TRIGGER set_timestamp_tenant_settings BEFORE UPDATE ON tenant_settings FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Tabela de Módulos
CREATE TABLE IF NOT EXISTS modules (
    module_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT
);

-- Tabela de Módulos Ativados
CREATE TABLE IF NOT EXISTS activated_modules (
    module_id INT PRIMARY KEY REFERENCES modules(module_id),
    activated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    activated_by INT NOT NULL REFERENCES users(user_id)
);


-- 3. Reseta o caminho de busca para o padrão, evitando efeitos colaterais em outras queries.
RESET search_path;