# MySQL Setup (WellnessAPI)

This backend now supports MySQL via `Pomelo.EntityFrameworkCore.MySql`.

## 1) Local appsettings

`WellnessAPI/appsettings.json` now includes:

- `DatabaseProvider: "MySql"`
- `ConnectionStrings:MySqlConnection`

Update username/password if needed.

## 2) Run MySQL quickly with Docker

From `wellness-backend` folder:

```bash
docker compose up -d mysql
```

## 3) Create MySQL migrations

Because existing migrations were created for SQLite, generate a MySQL-specific set:

```bash
cd WellnessAPI
dotnet ef migrations add InitialMySql --output-dir MigrationsMySql
dotnet ef database update
```

If `dotnet` is not in PATH on Windows:

```bash
"C:\Program Files\dotnet\dotnet.exe" ef migrations add InitialMySql --output-dir MigrationsMySql
"C:\Program Files\dotnet\dotnet.exe" ef database update
```

## 4) Run API

```bash
dotnet run
```

API should connect to MySQL using `ConnectionStrings:MySqlConnection`.
