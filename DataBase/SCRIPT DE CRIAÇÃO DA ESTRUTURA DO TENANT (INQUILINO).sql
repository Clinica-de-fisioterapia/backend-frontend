-- ========================================================================================
-- SCRIPT DE CRIAÇÃO DA ESTRUTURA DO TENANT (INQUILINO)
-- Template a ser executado pelo backend para cada novo cliente no plano compartilhado.
-- ========================================================================================

-- 1. Cria o schema para isolar os dados do novo tenant.
--    O nome 'teste_inicial' DEVE ser substituído dinamicamente pelo backend.
CREATE SCHEMA IF NOT EXISTS teste_inicial;

-- 2. Define o schema recém-criado como o padrão para todos os comandos a seguir.
SET search_path TO teste_inicial;

-- Tabela de Usuários
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,                                     -- Ex: 1
    person_id INT UNIQUE,                                           -- Ex: 1
    username VARCHAR(100) NOT NULL UNIQUE,                          -- Ex: 'admin_ze'
    password_hash VARCHAR(255) NOT NULL,                            -- Ex: '$2a$12$KjZ...'
    is_active BOOLEAN NOT NULL DEFAULT TRUE,                        -- Ex: true
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    deleted_at TIMESTAMP WITH TIME ZONE                             -- Ex: NULL
);
COMMENT ON TABLE users IS 'Usuários que podem logar no sistema (admins, recepcionistas, etc).';

-- Tabela de Configurações por Tenant
CREATE TABLE tenant_settings (
    setting_key VARCHAR(100) PRIMARY KEY,                           -- Ex: 'booking.cancelation_window_hours'
    setting_value VARCHAR(255) NOT NULL,                            -- Ex: '24'
    description TEXT,                                               -- Ex: 'Tempo mínimo em horas para cancelamento.'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    updated_by INT REFERENCES users(user_id)                        -- Ex: 1
);
COMMENT ON TABLE tenant_settings IS 'Armazena configurações customizáveis para cada tenant.';
CREATE TRIGGER set_timestamp_tenant_settings BEFORE UPDATE ON tenant_settings FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Tabela de Unidades
CREATE TABLE units (
    unit_id SERIAL PRIMARY KEY,                                     -- Ex: 1
    name VARCHAR(255) NOT NULL,                                     -- Ex: 'Matriz Centro'
    address TEXT,                                                   -- Ex: 'Rua das Flores, 123, Centro'
    phone VARCHAR(20),                                              -- Ex: '11988887777'
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    deleted_at TIMESTAMP WITH TIME ZONE,                            -- Ex: NULL
    created_by INT REFERENCES users(user_id),                       -- Ex: 1
    updated_by INT REFERENCES users(user_id)                        -- Ex: NULL
);
CREATE TRIGGER set_timestamp_units BEFORE UPDATE ON units FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Tabela base para Pessoas
CREATE TABLE people (
    person_id SERIAL PRIMARY KEY,                                   -- Ex: 1
    full_name VARCHAR(255) NOT NULL,                                -- Ex: 'José da Silva'
    cpf CHAR(11),                                                   -- Ex: '11122233344'
    email VARCHAR(255),                                             -- Ex: 'ze.silva@email.com'
    phone VARCHAR(20),                                              -- Ex: '11988887777'
    birth_date DATE,                                                -- Ex: '1985-04-20'
    address TEXT,                                                   -- Ex: 'Avenida Principal, 456, Bairro Norte'
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    deleted_at TIMESTAMP WITH TIME ZONE,                            -- Ex: NULL
    created_by INT REFERENCES users(user_id),                       -- Ex: 1
    updated_by INT REFERENCES users(user_id)                        -- Ex: NULL
);
COMMENT ON COLUMN people.cpf IS 'Armazenar apenas dígitos. Recomenda-se criptografar este campo com pgcrypto.';
COMMENT ON COLUMN people.email IS 'Recomenda-se criptografar este campo com pgcrypto.';
CREATE TRIGGER set_timestamp_people BEFORE UPDATE ON people FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();
CREATE UNIQUE INDEX idx_people_cpf_unique_if_not_deleted ON people(cpf) WHERE deleted_at IS NULL;
CREATE UNIQUE INDEX idx_people_email_unique_if_not_deleted ON people(email) WHERE deleted_at IS NULL;
CREATE INDEX idx_people_phone ON people(phone);
CREATE INDEX idx_people_full_name_trgm ON people USING GIN (full_name gin_trgm_ops);

-- Tabela de Profissionais
CREATE TABLE professionals (
    professional_id SERIAL PRIMARY KEY,                             -- Ex: 1
    person_id INT NOT NULL UNIQUE REFERENCES people(person_id) ON DELETE CASCADE, -- Ex: 1
    is_active BOOLEAN NOT NULL DEFAULT TRUE,                        -- Ex: true
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    created_by INT REFERENCES users(user_id),                       -- Ex: 1
    updated_by INT REFERENCES users(user_id)                        -- Ex: NULL
);
CREATE TRIGGER set_timestamp_professionals BEFORE UPDATE ON professionals FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Tabela de Clientes
CREATE TABLE customers (
    customer_id SERIAL PRIMARY KEY,                                 -- Ex: 1
    person_id INT NOT NULL UNIQUE REFERENCES people(person_id) ON DELETE CASCADE, -- Ex: 2
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    created_by INT REFERENCES users(user_id),                       -- Ex: 1
    updated_by INT REFERENCES users(user_id)                        -- Ex: NULL
);
CREATE TRIGGER set_timestamp_customers BEFORE UPDATE ON customers FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Tabela de Perfis/Funções
CREATE TABLE roles (
    role_id SERIAL PRIMARY KEY,                                     -- Ex: 1
    name VARCHAR(50) NOT NULL UNIQUE                                -- Ex: 'Admin'
);
INSERT INTO roles (name) VALUES ('Admin'), ('Professional'), ('Receptionist');

-- Tabela de associação User-Role-Unit
CREATE TABLE user_roles (
    user_id INT NOT NULL REFERENCES users(user_id) ON DELETE CASCADE, -- Ex: 1
    role_id INT NOT NULL REFERENCES roles(role_id) ON DELETE CASCADE, -- Ex: 1
    unit_id INT NOT NULL REFERENCES units(unit_id) ON DELETE CASCADE, -- Ex: 1
    assigned_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP, -- Ex: '2025-09-12 20:16:30-03'
    assigned_by INT NOT NULL REFERENCES users(user_id),             -- Ex: 1
    PRIMARY KEY (user_id, role_id, unit_id)
);

-- Tabela de Serviços
CREATE TABLE services (
    service_id SERIAL PRIMARY KEY,                                  -- Ex: 1
    name VARCHAR(255) NOT NULL,                                     -- Ex: 'Corte de Cabelo Masculino'
    description TEXT,                                               -- Ex: 'Corte com máquina e tesoura.'
    duration_minutes INT NOT NULL CHECK (duration_minutes > 0),     -- Ex: 30
    price DECIMAL(10, 2) NOT NULL,                                  -- Ex: 50.00
    is_active BOOLEAN NOT NULL DEFAULT TRUE,                        -- Ex: true
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    deleted_at TIMESTAMP WITH TIME ZONE,                            -- Ex: NULL
    created_by INT REFERENCES users(user_id),                       -- Ex: 1
    updated_by INT REFERENCES users(user_id)                        -- Ex: NULL
);
CREATE TRIGGER set_timestamp_services BEFORE UPDATE ON services FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Tabela de Horários de Disponibilidade
CREATE TABLE schedule_slots (
    schedule_slot_id SERIAL PRIMARY KEY,                            -- Ex: 1
    professional_id INT NOT NULL REFERENCES professionals(professional_id) ON DELETE CASCADE, -- Ex: 1
    unit_id INT NOT NULL REFERENCES units(unit_id) ON DELETE CASCADE, -- Ex: 1
    weekday SMALLINT NOT NULL CHECK (weekday BETWEEN 0 AND 6),      -- Ex: 2 (Terça-feira)
    start_time TIME NOT NULL,                                       -- Ex: '09:00:00'
    end_time TIME NOT NULL,                                         -- Ex: '18:00:00'
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    deleted_at TIMESTAMP WITH TIME ZONE,                            -- Ex: NULL
    created_by INT REFERENCES users(user_id),                       -- Ex: 1
    updated_by INT REFERENCES users(user_id)                        -- Ex: NULL
);
CREATE TRIGGER set_timestamp_schedule_slots BEFORE UPDATE ON schedule_slots FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Tabela de Agendamentos (Particionada)
CREATE TABLE bookings (
    booking_id BIGSERIAL NOT NULL,                                  -- Ex: 10254
    customer_id INT NOT NULL REFERENCES customers(customer_id) ON DELETE RESTRICT, -- Ex: 1
    professional_id INT NOT NULL REFERENCES professionals(professional_id) ON DELETE RESTRICT, -- Ex: 1
    unit_id INT NOT NULL REFERENCES units(unit_id) ON DELETE RESTRICT, -- Ex: 1
    service_id INT NOT NULL REFERENCES services(service_id) ON DELETE RESTRICT, -- Ex: 1
    start_datetime TIMESTAMP WITH TIME ZONE NOT NULL,               -- Ex: '2025-09-15 14:00:00-03'
    end_datetime TIMESTAMP WITH TIME ZONE NOT NULL,                 -- Ex: '2025-09-15 14:30:00-03'
    status public.booking_status NOT NULL DEFAULT 'confirmed',      -- Ex: 'confirmed'
    notes TEXT,                                                     -- Ex: 'Cliente prefere máquina 2.'
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,  -- Ex: '2025-09-12 20:16:30-03'
    created_by INT REFERENCES users(user_id),                       -- Ex: 1 ou NULL
    updated_by INT REFERENCES users(user_id),                       -- Ex: NULL
    PRIMARY KEY (booking_id, start_datetime),                       -- Chave primária composta (obrigatória para partição)
    CONSTRAINT no_overlapping_bookings EXCLUDE USING GIST (
        professional_id WITH =,
        TSTZRANGE(start_datetime, end_datetime) WITH &&
    )
) PARTITION BY RANGE (start_datetime);

COMMENT ON TABLE bookings IS 'Tabela principal de agendamentos, particionada por data para performance em larga escala.';
CREATE TRIGGER set_timestamp_bookings BEFORE UPDATE ON bookings FOR EACH ROW EXECUTE PROCEDURE public.trigger_set_timestamp();

-- Partições de Exemplo para a Tabela Bookings (em produção, este processo deve ser automatizado)
CREATE TABLE bookings_y2025 PARTITION OF bookings FOR VALUES FROM ('2025-01-01 00:00:00-03') TO ('2026-01-01 00:00:00-03');
CREATE TABLE bookings_y2026 PARTITION OF bookings FOR VALUES FROM ('2026-01-01 00:00:00-03') TO ('2027-01-01 00:00:00-03');

-- Índices são criados na tabela principal e propagados automaticamente para as partições.
CREATE INDEX idx_bookings_professional_datetime ON bookings(professional_id, start_datetime);
CREATE INDEX idx_bookings_customer_datetime ON bookings(customer_id, start_datetime);
CREATE INDEX idx_bookings_unit_status_datetime ON bookings(unit_id, status, start_datetime);

-- Tabela de Módulos
CREATE TABLE modules (
    module_id SERIAL PRIMARY KEY,                                   -- Ex: 1
    name VARCHAR(100) NOT NULL UNIQUE,                              -- Ex: 'Prontuário Eletrônico'
    description TEXT                                                -- Ex: 'Módulo para gestão de prontuários.'
);

-- Tabela de Módulos Ativados
CREATE TABLE activated_modules (
    module_id INT PRIMARY KEY REFERENCES modules(module_id),        -- Ex: 1
    activated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,-- Ex: '2025-09-12 20:16:30-03'
    activated_by INT NOT NULL REFERENCES users(user_id)             -- Ex: 1
);