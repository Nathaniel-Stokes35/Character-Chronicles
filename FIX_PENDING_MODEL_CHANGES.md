# PendingModelChangesWarning fix

The previous revision called `db.Database.MigrateAsync()` at application startup after replacing ASP.NET Identity with the custom `Users` model.

The project still has historical EF migration metadata/snapshots that describe the old ASP.NET Identity tables. EF Core therefore compares the current model with that old snapshot and throws `PendingModelChangesWarning` before the site starts.

This revision does **not** run all EF migrations at startup. It connects to the PostgreSQL database from `DATABASE_URL` in `.env` and creates only the custom `Users` table and its unique normalized-email index with `CREATE ... IF NOT EXISTS`.

Authentication still uses the same PostgreSQL database. Existing character/campaign/note tables are left untouched.
