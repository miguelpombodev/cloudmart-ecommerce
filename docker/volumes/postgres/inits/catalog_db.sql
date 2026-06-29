CREATE ROLE catalog WITH
    LOGIN
    PASSWORD 'postgres'
    NOSUPERUSER
    NOCREATEDB
    NOCREATEROLE;

CREATE
DATABASE "CatalogDB"
       WITH
       OWNER = catalog
       ENCODING = 'UTF8'
       LC_COLLATE = 'C.UTF-8'
       LC_CTYPE = 'C.UTF-8'
       TEMPLATE = template0
       CONNECTION LIMIT = 100;

COMMENT
ON DATABASE "CatalogDB" IS 'Cloudmart - Catalog Service';

\connect "CatalogDB";

CREATE
EXTENSION IF NOT EXISTS "uuid-ossp"; -- UUIDs
CREATE
EXTENSION IF NOT EXISTS "pg_trgm"; -- search for similarity
CREATE
EXTENSION IF NOT EXISTS "unaccent"; -- search without accent

CREATE
EXTENSION IF NOT EXISTS pg_stat_statements;

CREATE
EXTENSION IF NOT EXISTS pgstattuple;

CREATE SCHEMA IF NOT EXISTS catalog AUTHORIZATION catalog;
CREATE SCHEMA IF NOT EXISTS audit AUTHORIZATION catalog;

-- Readonly role (for statistics, analytics, external tooling)
DO
$$
BEGIN
	IF
NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'catalog_readonly') THEN
CREATE ROLE catalog_readonly NOLOGIN;
END IF;
END
$$;


-- App role
DO
$$
BEGIN
	IF
NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'catalog_app') THEN
CREATE ROLE catalog_app NOLOGIN;
END IF;
END
$$;

GRANT USAGE ON SCHEMA
catalog TO catalog_app;
GRANT USAGE ON SCHEMA
catalog TO catalog_readonly;

-- catalog_app reads and writes, but can´t affect the structure
GRANT
SELECT,
INSERT
,
UPDATE,
DELETE
ON ALL TABLES IN SCHEMA catalog TO catalog_app;
GRANT
USAGE,
SELECT
ON ALL SEQUENCES IN SCHEMA catalog TO catalog_app;


-- catalog_readonly only reads
GRANT
SELECT
ON ALL TABLES IN SCHEMA catalog TO catalog_readonly;

-- Garantee that future tables, like migrations table, herit permissions from others
ALTER
DEFAULT PRIVILEGES IN SCHEMA catalog
GRANT
SELECT,
INSERT
,
UPDATE,
DELETE
ON TABLES TO catalog_app;

ALTER
DEFAULT PRIVILEGES IN SCHEMA catalog
      GRANT USAGE,
SELECT
ON SEQUENCES TO catalog_app;

ALTER
DEFAULT PRIVILEGES IN SCHEMA catalog
      GRANT
SELECT
ON TABLES TO catalog_readonly;

-- default timezone
ALTER
DATABASE "CatalogDB" SET timezone TO 'UTC';

-- Avoid accidental full seq scan em bigger tables
ALTER
DATABASE "CatalogDB" SET enable_seqscan TO on;

-- More accurate statistics for query planner
ALTER
DATABASE "CatalogDB" SET default_statistics_target TO 100;

---------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------

ALTER
SYSTEM SET shared_buffers                = '256MB';   -- cache do postgres em memória
ALTER
SYSTEM SET effective_cache_size          = '768MB';   -- estimativa total de cache (SO + postgres)
ALTER
SYSTEM SET work_mem                      = '16MB';    -- memória por operação de sort/hash (cuidado: multiplica por conexão)
ALTER
SYSTEM SET maintenance_work_mem          = '128MB';   -- VACUUM, CREATE INDEX, ALTER TABLE
ALTER
SYSTEM SET wal_buffers                   = '16MB';    -- buffer do WAL (write-ahead log)
ALTER
SYSTEM SET checkpoint_completion_target  = '0.9';     -- distribui escrita do checkpoint
ALTER
SYSTEM SET random_page_cost              = '1.1';     -- SSD: use 1.1. HDD: use 4.0
ALTER
SYSTEM SET effective_io_concurrency      = '200';     -- SSD: 200. HDD: 2
ALTER
SYSTEM SET max_connections               = '100';     -- com PgBouncer na frente, pode ser menor
ALTER
SYSTEM SET min_wal_size                  = '1GB';
ALTER
SYSTEM SET max_wal_size                  = '4GB';

-- Paralelismo (ajuste para número de cores disponíveis)
ALTER
SYSTEM SET max_parallel_workers_per_gather = '2';
ALTER
SYSTEM SET max_parallel_workers            = '4';
ALTER
SYSTEM SET max_worker_processes            = '8';

-- Logging de queries lentas (essencial para debugging)
ALTER
SYSTEM SET log_min_duration_statement = '1000';  -- loga queries > 1 segundo
ALTER
SYSTEM SET log_checkpoints            = 'on';
ALTER
SYSTEM SET log_connections            = 'off';   -- muito verboso, liga só para debug
ALTER
SYSTEM SET log_lock_waits             = 'on';    -- loga quando há espera por lock
ALTER
SYSTEM SET deadlock_timeout           = '1s';

-- Estatísticas
ALTER
SYSTEM SET track_activities     = 'on';
ALTER
SYSTEM SET track_counts         = 'on';
ALTER
SYSTEM SET track_io_timing      = 'on';   -- mede tempo real de I/O nas queries
ALTER
SYSTEM SET track_wal_io_timing  = 'on';

SELECT pg_reload_conf();
-- aplica sem restart

-- OutboxMessages Table
CREATE TABLE IF NOT EXISTS catalog.outbox_messages
(
    id
    uuid
    NOT
    NULL,
    type
    text
    NOT
    NULL,
    payload
    jsonb
    NOT
    NULL,
    occurred_on
    timestamp
    with
    time
    zone
    NOT
    NULL,
    processed_on
    timestamp
    with
    time
    zone
    NULL,
    error
    text
    NULL,

    CONSTRAINT
    pk_outbox_messages
    PRIMARY
    KEY
(
    id
)
    );

CREATE INDEX idx_outbox_messages_pending
    ON catalog.outbox_messages (occurred_on ASC) WHERE processed_on IS NULL; -- índice parcial - só indexa pendentes

COMMENT
ON TABLE  catalog.outbox_messages               IS 'Outbox pattern — eventos pendentes de publicação';
COMMENT
ON COLUMN catalog.outbox_messages.type          IS 'AssemblyQualifiedName do tipo do evento para desserialização';
COMMENT
ON COLUMN catalog.outbox_messages.processed_on  IS 'NULL indica mensagem pendente. Preenchido após publicação no broker';
COMMENT
ON COLUMN catalog.outbox_messages.error         IS 'Preenchido quando a publicação falha após todas as tentativas';
