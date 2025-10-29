-- ==================================================================================================
-- SCRIPT: UNITS_AUDIT_SYNC.sql
-- OBJETIVO: Garantir que todas as tabelas {tenant}.units possuam colunas de auditoria
--           (created_by, updated_by, deleted_at, row_version) e trigger BEFORE UPDATE para
--           atualizar updated_at/row_version via public.fn_set_updated_at().
-- CONTEXTO: Multi-tenant por schema. Itera sobre public.tenants (ativos) aplicando ajustes.
-- ==================================================================================================

DO $$
DECLARE
    tenant_record    RECORD;
    table_name       TEXT;
    table_oid        OID;
    trigger_exists   BOOLEAN;
BEGIN
    FOR tenant_record IN
        SELECT slug
        FROM public.tenants
        WHERE deleted_at IS NULL
    LOOP
        table_name := format('%I.units', tenant_record.slug);

        EXECUTE format('SELECT to_regclass(%L)', table_name) INTO table_oid;
        IF table_oid IS NULL THEN
            RAISE NOTICE 'Tabela % não encontrada. Pulando tenant %. ', table_name, tenant_record.slug;
            CONTINUE;
        END IF;

        EXECUTE format('ALTER TABLE %s ADD COLUMN IF NOT EXISTS created_by uuid NULL;', table_name);
        EXECUTE format('ALTER TABLE %s ADD COLUMN IF NOT EXISTS updated_by uuid NULL;', table_name);
        EXECUTE format('ALTER TABLE %s ADD COLUMN IF NOT EXISTS deleted_at timestamptz NULL;', table_name);
        EXECUTE format('ALTER TABLE %s ADD COLUMN IF NOT EXISTS row_version bigint;', table_name);

        EXECUTE format('ALTER TABLE %s ALTER COLUMN created_by DROP NOT NULL;', table_name);
        EXECUTE format('ALTER TABLE %s ALTER COLUMN updated_by DROP NOT NULL;', table_name);

        EXECUTE format('ALTER TABLE %s ALTER COLUMN row_version SET DEFAULT 0;', table_name);
        EXECUTE format('UPDATE %s SET row_version = COALESCE(row_version, 0);', table_name);
        EXECUTE format('ALTER TABLE %s ALTER COLUMN row_version SET NOT NULL;', table_name);

        EXECUTE format('SELECT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = %L AND tgrelid = %s::regclass);',
                       'trg_units_set_updated_at', table_name) INTO trigger_exists;

        IF NOT trigger_exists THEN
            EXECUTE format(
                'CREATE TRIGGER trg_units_set_updated_at\n'
                || 'BEFORE UPDATE ON %s\n'
                || 'FOR EACH ROW EXECUTE FUNCTION public.fn_set_updated_at();',
                table_name
            );
        END IF;
    END LOOP;
END;
$$ LANGUAGE plpgsql;
