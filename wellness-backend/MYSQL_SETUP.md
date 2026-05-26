# Database Setup — pick ANY ONE: XAMPP · Laragon · Docker

The backend runs on **MySQL / MariaDB** via `Pomelo.EntityFrameworkCore.MySql`
with `ServerVersion.AutoDetect(...)`, so it adapts automatically to whichever
engine/version you run.

**The default connection is identical for all three options**
(`root` user · empty password · port `3306` · database `wellnessdb`), so the
backend runs with **no code or config changes** whichever you pick.

> Run only **one** of them at a time — they all use port **3306**.

You only need two things: (1) the DB server running, (2) a database named
`wellnessdb`. On startup the API applies EF Core migrations automatically and
seeds demo data.

---

## Option 1 — XAMPP

1. Open the XAMPP control panel → **Start MySQL**.
2. Create the database (phpMyAdmin → **New** → `wellnessdb`), or via terminal:
   ```bash
   # macOS
   /Applications/XAMPP/xamppfiles/bin/mysql -u root -e "CREATE DATABASE IF NOT EXISTS wellnessdb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
   # Windows
   "C:\xampp\mysql\bin\mysql.exe" -u root -e "CREATE DATABASE IF NOT EXISTS wellnessdb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
   ```
3. Run the app (`./START.sh` or `./START.ps1`).

## Option 2 — Laragon

1. Start Laragon → **Start All** (starts MySQL/MariaDB).
2. Menu → **Database** (or HeidiSQL) → create database `wellnessdb`.
3. Run the app (`./START.ps1` on Windows, `./START.sh` on macOS/Linux).

> Laragon's `root` has an empty password by default (matches the default config).
> If yours has a password, set it in `.env`:
> `ConnectionStrings__DefaultConnection=Server=localhost;Port=3306;Database=wellnessdb;User=root;Password=YOURPASS;`

## Option 3 — Docker

From the `wellness-backend` folder (port 3306 must be free, i.e. XAMPP/Laragon
MySQL stopped):

```bash
docker compose up -d        # MySQL 8.4, db "wellnessdb", root + empty password
docker compose down         # stop (data kept in the named volume)
docker compose down -v      # stop and DELETE all data
```

The container is preconfigured to match the default connection string, so the
database `wellnessdb` is created for you automatically.

---

## Migrations

Applied automatically on startup (`db.Database.Migrate()`). To run manually or
add a schema change (the DB must be running):

```bash
cd WellnessAPI
dotnet ef database update            # apply
dotnet ef migrations add <Name>      # create a new migration, then update
```

> Windows without `dotnet` on PATH: `& "C:\Program Files\dotnet\dotnet.exe" ef database update`
>
> The old SQLite migration set is archived (unused) under `wellness-backend/_migrations_backup/`.
