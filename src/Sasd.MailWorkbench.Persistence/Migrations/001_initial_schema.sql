PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS raw_messages (
    id TEXT PRIMARY KEY,
    account_id TEXT NOT NULL,
    sha256 TEXT NOT NULL,
    byte_length INTEGER NOT NULL CHECK (byte_length >= 0),
    relative_path TEXT NOT NULL,
    imported_at_utc TEXT NOT NULL,
    UNIQUE(account_id, sha256, byte_length),
    UNIQUE(relative_path)
);

CREATE TABLE IF NOT EXISTS source_observations (
    account_id TEXT NOT NULL,
    source_key TEXT NOT NULL,
    message_id TEXT NOT NULL,
    first_seen_at_utc TEXT NOT NULL,
    last_seen_at_utc TEXT NOT NULL,
    PRIMARY KEY(account_id, source_key),
    FOREIGN KEY(message_id) REFERENCES raw_messages(id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS import_attempts (
    id TEXT PRIMARY KEY,
    account_id TEXT NOT NULL,
    source_key TEXT NOT NULL,
    state INTEGER NOT NULL,
    staging_relative_path TEXT NULL,
    target_relative_path TEXT NULL,
    sha256 TEXT NULL,
    byte_length INTEGER NULL,
    existing_message_id TEXT NULL,
    started_at_utc TEXT NOT NULL,
    updated_at_utc TEXT NOT NULL,
    completed_at_utc TEXT NULL,
    retry_count INTEGER NOT NULL DEFAULT 0,
    error_code TEXT NULL,
    error_message TEXT NULL
);

CREATE INDEX IF NOT EXISTS ix_import_attempts_state
    ON import_attempts(state);
CREATE INDEX IF NOT EXISTS ix_source_observations_message_id
    ON source_observations(message_id);
