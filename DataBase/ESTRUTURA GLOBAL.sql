-- =================================================================================
-- ARQUIVO: ESTRUTURA GLOBAL.sql
-- OBJETIVO: Prepara o banco de dados com objetos compartilhados por todos os tenants.
-- EXECUÇÃO: Deve ser executado apenas UMA VEZ na configuração inicial do banco.
-- =================================================================================

-- 1. HABILITA EXTENSÕES GLOBAIS NECESSÁRIAS
-- Para gerar UUIDs, usar constraints de exclusão e busca de texto por similaridade.
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "btree_gist";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- 2. CRIA TIPOS ENUM GLOBAIS
-- Estes tipos serão usados por tabelas de tenants, mas sua definição é única.
CREATE TYPE user_role AS ENUM ('admin', 'professional', 'receptionist');
CREATE TYPE booking_status AS ENUM ('confirmed', 'canceled', 'completed', 'no_show');
CREATE TYPE schedule_status AS ENUM ('available', 'booked', 'blocked');

-- 3. FUNÇÕES GLOBAIS DE TRIGGER E UTILITÁRIAS

-- Função para atualizar a coluna 'updated_at' automaticamente.
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Função resiliente para criar partições da tabela 'bookings' se não existirem.
-- Um Worker Service chamará esta função para garantir que partições futuras sempre existam.
CREATE OR REPLACE FUNCTION create_bookings_partition_if_not_exists(start_ts timestamptz)
RETURNS void LANGUAGE plpgsql AS $$
DECLARE
    partition_name TEXT := 'bookings_y' || to_char(start_ts, 'YYYY');
    end_ts timestamptz := date_trunc('year', start_ts) + interval '1 year';
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = partition_name) THEN
        EXECUTE format('CREATE TABLE IF NOT EXISTS %I PARTITION OF bookings FOR VALUES FROM (%L) TO (%L);',
                       partition_name, start_ts::text, end_ts::text);
    END IF;
END;
$$;

-- 4. TABELAS GLOBAIS (Não pertencem a um tenant específico)

-- Tabela de Planos de Assinatura
CREATE TABLE IF NOT EXISTS plans (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    price_monthly NUMERIC(10, 2) NOT NULL,
    max_professionals INT NOT NULL DEFAULT 5,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ========================== FIM DA ESTRUTURA GLOBAL ==========================