# dotauth.authserverpg (Auth server with Postgres storage)

## Purpose
Variant of the DotAuth server configured to use PostgreSQL as its primary storage backend. Contains integration and storage wiring for Postgres.

## Quickstart
- Location: `src/dotauth.authserverpg/dotauth.authserverpg.csproj`
- Build: `dotnet build ./src/dotauth.authserverpg/dotauth.authserverpg.csproj`
- Run locally: `dotnet run --project ./src/dotauth.authserverpg/dotauth.authserverpg.csproj`

## Notes
Requires a running PostgreSQL instance and appropriate connection strings (set via environment variables or appsettings). See the project's Dockerfile(s) for examples.
