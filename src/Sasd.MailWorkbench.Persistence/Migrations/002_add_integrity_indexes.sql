CREATE INDEX IF NOT EXISTS ix_raw_messages_account_imported
    ON raw_messages(account_id, imported_at_utc);
CREATE INDEX IF NOT EXISTS ix_import_attempts_account_source
    ON import_attempts(account_id, source_key, started_at_utc);
